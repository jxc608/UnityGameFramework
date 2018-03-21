using System;
using Snaplingo.UI;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using System.Collections;
using System.Reflection;
using StructUtils;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Json attribute. 目前仅适用于非静态成员
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class JsonAttribute : Attribute
{
	// 保留关键字:
	//   class: 用作类名
	public static string[] ReservedWords = new string[] { ReservedWordClass };
	public static string ReservedWordClass = "class";

	public static string FunctionTag = "func";

	public string _key { get; private set; }

	public string _parentKey { get; private set; }

	public string _label { get; private set; }

	public JsonAttribute(string key, string parentKey = "", string label = "")
	{
		_key = key;
		_parentKey = parentKey;
		_label = label;
	}

	public string[] GetLabels(object inst)
	{
		var eles = _label.Split(new char[]{ ':' }, StringSplitOptions.RemoveEmptyEntries);
		if (eles.Length == 2) {
			for (int i = 0; i < eles.Length; ++i)
				eles[i] = eles[i].Trim();
			if (eles[0] == FunctionTag && inst != null) {
				var method = inst.GetType().GetMethod(eles[1]);
				if (method != null) {
					if (method.ReturnType == typeof(string[])) {
						return (string[])method.Invoke(inst, new object[]{ });
					} else {
						LogManager.LogError("JsonAttribute GetLabels error: method return type not fit: " + method.ReturnType.ToString());
					}
				} else {
					LogManager.LogError("JsonAttribute GetLabels error: cannot find method: " + eles[1]);
				}
			}
		}

		var labels = _label.Split(new char[]{ ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < labels.Length; ++i)
			labels[i] = labels[i].Trim();
		return labels;
	}
}

[AttributeUsage(AttributeTargets.Class)]
public class JsonClassAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class JsonEditorAttribute : Attribute
{
	public enum SpecialType
	{
		NodeIndex,
		SoundIndex,
	}

	public SpecialType _specialType { get; private set; }

	public string _parentKey { get; private set; }

	public JsonEditorAttribute(SpecialType type, string parentKey)
	{
		_specialType = type;
		_parentKey = parentKey;
	}
}


public static class JsonUtils
{
	#region reflection utilities

	public static BindingFlags _bindingFlag = BindingFlags.Instance
	                                          | BindingFlags.Public
	                                          | BindingFlags.NonPublic;

	// 通过反射获取成员的值
	public static object GetValueOfMember(object inst, MemberInfo member)
	{
		if (member != null) {
			if (member is FieldInfo) {
				return ((FieldInfo)member).GetValue(inst);
			} else if (member is PropertyInfo) {
				return ((PropertyInfo)member).GetValue(inst, null);
			}
		}

		return null;
	}

	public static void SetValueOfMember(ref object inst, MemberInfo member, object val)
	{
		if (member != null) {
			if (member is FieldInfo) {
				((FieldInfo)member).SetValue(inst, val);
			} else if (member is PropertyInfo) {
				((PropertyInfo)member).SetValue(inst, val, null);
			}
		}
	}

	public static Type GetTypeOfMember(MemberInfo member)
	{
		if (member != null) {
			if (member is FieldInfo) {
				return ((FieldInfo)member).FieldType;
			} else if (member is PropertyInfo) {
				return ((PropertyInfo)member).PropertyType;
			}
		}
		return default(Type);
	}

	public static MemberInfo GetJsonAttributeMemberByKey(Type type, string key)
	{
		foreach (var field in type.GetFields (_bindingFlag)) {
			var objs = field.GetCustomAttributes(typeof(JsonAttribute), true);
			if (objs != null && objs.Length > 0) {
				var customAttr = ((JsonAttribute)objs[0]);
				if (customAttr._key == key) {
					return field;
				}
			}
		}
		foreach (var prop in type.GetProperties (_bindingFlag)) {
			var objs = prop.GetCustomAttributes(typeof(JsonAttribute), true);
			if (objs != null && objs.Length > 0) {
				var customAttr = ((JsonAttribute)objs[0]);
				if (customAttr._key == key) {
					return prop;
				}
			}
		}

		return null;
	}

	#endregion

	#region get json from instance

	#if UNITY_EDITOR

	static Dictionary<MemberInfo, Tuple<MemberInfo, bool>> _parentMemberCache = new Dictionary<MemberInfo, Tuple<MemberInfo, bool>>();

	static MemberInfo GetParentMember(Type type, MemberInfo member, ref bool negative)
	{
		negative = false;
		if (member != null) {
			if (_parentMemberCache.ContainsKey(member)) {
				negative = _parentMemberCache[member]._item2;
				return _parentMemberCache[member]._item1;
			} else {
				MemberInfo parentMember = null;
				var objs = member.GetCustomAttributes(typeof(JsonAttribute), true);
				if (objs != null && objs.Length > 0) {
					var customAttr = ((JsonAttribute)objs[0]);
					var parentKey = customAttr._parentKey;
					if (!string.IsNullOrEmpty(parentKey)) {
						if (parentKey.StartsWith("!")) {
							parentKey = parentKey.Substring(1).Trim();
							negative = true;
						}
						parentMember = GetJsonAttributeMemberByKey(type, parentKey);
					}
				}
				_parentMemberCache.Add(member, new Tuple<MemberInfo, bool>(parentMember, negative));
				return parentMember;
			}
		}
		return null;
	}

	public static bool IsMemberActive<T>(object inst, MemberInfo member) where T : Attribute
	{
		if (member == null)
			return true;

		if (typeof(T) == typeof(JsonAttribute)) {
			bool negative = false;
			var parentMember = GetParentMember(inst.GetType(), member, ref negative);
			var checkMember = parentMember;
			var parentEditorMember = GetEditorAttributeMember(parentMember, inst);
			if (parentEditorMember != null) {
				checkMember = parentEditorMember;
			}
			var val = GetValueOfMember(inst, checkMember);
			if (val is Boolean) {
				return (negative == !(bool)val) && IsMemberActive<T>(inst, parentMember);
			} else {
				return true;
			}
		} else
			return true;
	}

	// 将object入一个Json数据
	static readonly int DecimalPrecision = 3;

	static double GetRoundedFloat(double val)
	{
		return Math.Round(val, DecimalPrecision);
	}

	public static void SetJson(JsonData data, string key, object val)
	{
		if (val is JsonData)
			data[key] = (JsonData)val;
		else
			data[key] = new JsonData(val);
	}

	static Dictionary<Type, Dictionary<Type, List<MemberInfo>>> _memberInfoCache = new Dictionary<Type, Dictionary<Type, List<MemberInfo>>>();

	static List<MemberInfo> GetAttributeMembers<T>(object inst) where T : Attribute
	{
		var type = inst.GetType();
		List<MemberInfo> allMembers = null;
		if (_memberInfoCache.ContainsKey(type)) {
			if (_memberInfoCache[type].ContainsKey(typeof(T))) {
				allMembers = _memberInfoCache[type][typeof(T)];
			}
		}
		if (allMembers == null) {
			allMembers = new List<MemberInfo>();
			foreach (var field in type.GetFields (_bindingFlag)) {
				var objs = field.GetCustomAttributes(typeof(T), true);
				if (objs != null && objs.Length > 0) {
					allMembers.Add(field);
				}
			}
			foreach (var prop in type.GetProperties (_bindingFlag)) {
				var objs = prop.GetCustomAttributes(typeof(T), true);
				if (objs != null && objs.Length > 0) {
					allMembers.Add(prop);
				}
			}
			if (!_memberInfoCache.ContainsKey(type)) {
				var dict = new Dictionary<Type, List<MemberInfo>>();
				dict.Add(typeof(T), allMembers);
				_memberInfoCache.Add(type, dict);
			} else {
				_memberInfoCache[type].Add(typeof(T), allMembers);
			}
		}

		var members = new List<MemberInfo>();
		for (int i = 0; i < allMembers.Count; ++i) {
			if (IsMemberActive<T>(inst, allMembers[i]))
				members.Add(allMembers[i]);
		}

		return members;
	}

	public static object GetJsonData(object inst)
	{
		if (inst is IList) {
			var list = (IList)inst;
			if (list.Count > 0) {
				var jdArray = new JsonData();
				foreach (var item in list) {
					jdArray.Add(GetJsonData(item));
				}
				return jdArray;
			} else {
				return JsonMapper.ToObject("[]");
			}
		} else if (inst is IDictionary) {
			var dict = (IDictionary)inst;
			if (dict.Count > 0) {
				var jdDict = new JsonData();
				foreach (var k in dict.Keys) {
					var jdKey = GetJsonData(k);
					string key = k.ToString();
					if (jdKey is JsonData)
						key = ((JsonData)jdKey).ToJson();
					var val = dict[k];
					var jdVal = GetJsonData(val);
					SetJson(jdDict, key, jdVal);
				}
				return jdDict;
			} else {
				return JsonMapper.ToObject("{}");
			}
		} else {
			if (inst is Boolean || inst is Int32 || inst is Int64 || inst is String) {
				return inst;
			} else if (inst is Double) {
				return GetRoundedFloat((double)inst);
			} else if (inst is Single) {
				return GetRoundedFloat((double)(float)inst);
			} else if (inst is Enum) {
				return (int)inst;
			} else if (inst is Rect) {
				var rect = (Rect)inst;
				JsonData rectData = new JsonData();
				rectData["x"] = GetRoundedFloat(rect.x);
				rectData["y"] = GetRoundedFloat(rect.y);
				rectData["w"] = GetRoundedFloat(rect.width);
				rectData["h"] = GetRoundedFloat(rect.height);
				return rectData;
			} else if (inst is Vector2) {
				var vec2 = (Vector2)inst;
				JsonData vec2Data = new JsonData();
				vec2Data["x"] = GetRoundedFloat(vec2.x);
				vec2Data["y"] = GetRoundedFloat(vec2.y);
				return vec2Data;
			} else if (inst is Vector3) {
				var vec3 = (Vector3)inst;
				JsonData vec3Data = new JsonData();
				vec3Data["x"] = GetRoundedFloat(vec3.x);
				vec3Data["y"] = GetRoundedFloat(vec3.y);
				vec3Data["z"] = GetRoundedFloat(vec3.z);
				return vec3Data;
			} else if (inst is Vector4) {
				var vec4 = (Vector4)inst;
				JsonData vec4Data = new JsonData();
				vec4Data["x"] = GetRoundedFloat(vec4.x);
				vec4Data["y"] = GetRoundedFloat(vec4.y);
				vec4Data["z"] = GetRoundedFloat(vec4.z);
				vec4Data["w"] = GetRoundedFloat(vec4.w);
				return vec4Data;
			} else {
				JsonData data = new JsonData();
				Type type = inst.GetType();
				var classAttribute = Attribute.GetCustomAttribute(type, typeof(JsonClassAttribute), true) as JsonClassAttribute;
				if (classAttribute != null) {
					data[JsonAttribute.ReservedWordClass] = type.ToString();
				}

				var list = GetAttributeMembers<JsonAttribute>(inst);

				foreach (var member in list) {
					var attribute = GetAttributeByMember<JsonAttribute>(member);
					foreach (var word in JsonAttribute.ReservedWords) {
						if (attribute._key == word) {
							throw new Exception("In json attribute, we keep \"" + word + "\" as a preserved word, please choose another key.");
						}
					}

					SetJson(data, attribute._key, GetJsonData(GetValueOfMember(inst, member)));
				}
				return data;
			}
		}
	}
	#endif

	#endregion

	#region read from json to instance

	static Dictionary<MemberInfo, List<Attribute>> _attributeCache = new Dictionary<MemberInfo, List<Attribute>>();

	static T GetAttributeByMember<T>(MemberInfo member) where T : Attribute
	{
		if (member != null) {
			if (_attributeCache.ContainsKey(member)) {
				for (int i = 0; i < _attributeCache[member].Count; ++i) {
					if (_attributeCache[member][i] is T) {
						return (T)_attributeCache[member][i];
					}
				}
			}

			var objs = member.GetCustomAttributes(typeof(T), true);
			if (objs != null && objs.Length > 0) {
				T attr = (T)objs[0];
				if (!_attributeCache.ContainsKey(member))
					_attributeCache.Add(member, new List<Attribute>() { attr });
				else
					_attributeCache[member].Add(attr);
				return attr;
			}
		}

		return null;
	}

	public static float Json2Float(JsonData data)
	{
		return (float)(double)data;
	}

	// 从Json数据读取到一个对象
	public static bool ReadFromJson(ref object inst, Type type, JsonData data, int fixedLength = 0)
	{
		var success = true;
		if (data.IsArray) {
			ReadFromJsonArray(ref inst, type, data, fixedLength);
		} else {
			// 数组或List将会兼容单个数据
			if (MiscUtils.HasInterface(type, typeof(IList))) {
				var jdArray = new JsonData();
				jdArray.Add(data);
				ReadFromJsonArray(ref inst, type, jdArray, fixedLength);
			} else {
				object val = null;
				if (data.IsBoolean) {
					val = (bool)data;
				} else if (data.IsDouble) {
					ReadFromDouble(ref val, type, data);
				} else if (data.IsInt) {
					val = (int)data;
				} else if (data.IsLong) {
					val = (long)data;
				} else if (data.IsString) {
					ReadFromString(ref val, type, (string)data);
				} else if (data.IsObject) {
					if (ReadFromJsonSpecialObject(ref val, type, data)) {
					} else if (MiscUtils.HasInterface(type, typeof(IDictionary))) {
						ReadFromJsonDictionary(ref val, type, data);
					} else {
						ReadFromJsonObject(ref val, type, data);
					}
				} else {
					LogManager.LogWarning("Warning! Unsupported json type: " + data.GetJsonType().ToString());
					success = false;
				}
				inst = val;
			}
		}

		return success;
	}

	// 读取字符串
	static void ReadFromString(ref object inst, Type type, string str)
	{
		if (type == typeof(Boolean))
			inst = bool.Parse(str);
		else if (type == typeof(Int32))
			inst = int.Parse(str);
		else if (type == typeof(Int64))
			inst = long.Parse(str);
		else if (type == typeof(Double))
			inst = double.Parse(str);
		else if (type == typeof(Single))
			inst = float.Parse(str);
		else if (type.IsEnum)
			inst = int.Parse(str);
		else
			inst = str;
	}

	// 读取特殊对象
	static bool ReadFromJsonSpecialObject(ref object inst, Type type, JsonData data)
	{
		if (type == typeof(Rect)) {
			inst = new Rect(Json2Float(data["x"]), Json2Float(data["y"]), Json2Float(data["w"]), Json2Float(data["h"]));
			return true;
		} else if (type == typeof(Vector2)) {
			inst = new Vector2(Json2Float(data["x"]), Json2Float(data["y"]));
			return true;
		} else if (type == typeof(Vector3)) {
			inst = new Vector3(Json2Float(data["x"]), Json2Float(data["y"]), Json2Float(data["z"]));
			return true;
		} else if (type == typeof(Vector4)) {
			inst = new Vector4(Json2Float(data["x"]), Json2Float(data["y"]), Json2Float(data["z"]), Json2Float(data["w"]));
			return true;
		}
		return false;
	}

	// 创建数组或List对象
	static void ReadFromJsonArray(ref object inst, Type type, JsonData data, int fixedLength = 0)
	{
		inst = Activator.CreateInstance(type);
		Type eleType = null;
		if (inst is Array) { // Array
			eleType = type.GetElementType();
			type.InvokeMember("Set", BindingFlags.CreateInstance, null, inst, new object[] { data.Count });
		} else { // List<?>
			eleType = type.GetGenericArguments()[0];
			type.GetMethod("Clear").Invoke(inst, new object[]{ });
		}
		for (int i = 0; i < (fixedLength > 0 ? fixedLength : data.Count); ++i) {
			object ele = null;
			if (data.Count > i)
				ReadFromJson(ref ele, eleType, data[i]);
			else {
				ele = GetDefaultValue(eleType);
			}
			if (inst is Array) { // Array
				type.GetMethod("SetValue").Invoke(inst, new object[]{ ele, i });
			} else { // List<?>
				type.GetMethod("Add").Invoke(inst, new object[]{ ele });
			}
		}
	}

	static object GetDefaultValue(Type type)
	{
		object defaultVal = null;
		if (type == typeof(string))
			defaultVal = "";
		else {
			defaultVal = Activator.CreateInstance(type);
		}
		return defaultVal;
	}

	// 创建浮点数对象
	static void ReadFromDouble(ref object inst, Type type, JsonData data)
	{
		if (type == typeof(Single))
			inst = Json2Float(data);
		else if (type == typeof(Double))
			inst = (double)data;
		else
			Debug.LogError("Error read double in ReadFromJsonData:" + data.ToJson());
	}

	// 从Json数据中创建对应的Dictionary对象
	static void ReadFromJsonDictionary(ref object inst, Type type, JsonData data)
	{
		inst = Activator.CreateInstance(type);
		type.GetMethod("Clear").Invoke(inst, new object[] { });
		var keyType = type.GetGenericArguments()[0];
		var valType = type.GetGenericArguments()[1];
		foreach (string k in data.Keys) {
			object keyVal = null;
			object valVal = null;
			JsonData jd = null;
			try {
				jd = JsonMapper.ToObject(k);
			} catch {
				jd = new JsonData(k);
			}
			ReadFromJson(ref keyVal, keyType, jd);
			ReadFromJson(ref valVal, valType, data[k]);
			type.GetMethod("Add", new Type[] { keyType, valType }).Invoke(inst, new object[] { keyVal, valVal });
		}
	}

	// 从Json中将数据读取到对象中
	static void ReadFromJsonObject(ref object inst, Type instType, JsonData data)
	{
		if (data.Keys.Contains(JsonAttribute.ReservedWordClass))
			instType = Type.GetType(data[JsonAttribute.ReservedWordClass].ToString());
		if (!instType.IsAbstract)
			inst = Activator.CreateInstance(instType);
		else {
			Debug.LogError("Error read json object in ReadFromJsonData:" + data.ToJson());
			return;
		}
		foreach (string key in data.Keys) {
			var subdata = data[key];
			var member = GetJsonAttributeMemberByKey(inst.GetType(), key);
			if (member == null) {
				continue;
			}
			var type = GetTypeOfMember(member);
			object val = null;
			var fixedLength = 0;
			if (MiscUtils.HasInterface(type, typeof(IList))) {
				var ja = GetAttributeByMember<JsonAttribute>(member);
				fixedLength = ja.GetLabels(inst).Length;
			}
			ReadFromJson(ref val, type, subdata, fixedLength);
			SetValueOfMember(ref inst, member, val);
		}

		#if UNITY_EDITOR
		SetEditorValueOfMember(ref inst);
		#endif
	}

	#if UNITY_EDITOR
	// 设置仅在编辑器中使用的变量
	public static void SetEditorValueOfMember(ref object inst)
	{
		var list = GetAttributeMembers<JsonEditorAttribute>(inst);

		foreach (var member in list) {
			var attribute = GetAttributeByMember<JsonEditorAttribute>(member);
			var val = GetValueOfMember(inst, GetJsonAttributeMemberByKey(inst.GetType(), attribute._parentKey));
			switch (attribute._specialType) {
				case JsonEditorAttribute.SpecialType.NodeIndex:
					var nodeName = (string)val;
					var typeFromName = Type.GetType(nodeName);
					if (typeFromName != null && MiscUtils.IsSubclassOf(typeFromName, typeof(Node))) {
						var subclasses = MiscUtils.GetSubclassTypes(typeof(Node));
						for (int i = 0; i < subclasses.Length; ++i) {
							if (subclasses[i].ToString().Equals(nodeName)) {
								SetValueOfMember(ref inst, member, i);
								break;
							}
						}
					}
					break;
				case JsonEditorAttribute.SpecialType.SoundIndex:
					try {
						var soundId = (int)val;
						var soundIndex = AudioConfig.Instance.GetIndexById(soundId);
						if (soundId == 0)
							Debug.Log(soundIndex);
						SetValueOfMember(ref inst, member, soundIndex);
					} catch {
						SetValueOfMember(ref inst, member, 0);
					}
					break;
			}
		}
	}

	#endif

	#endregion

	#region for editor

	#if UNITY_EDITOR
	// 对编辑器中显示的条目排序
	static void SortMemberList(Type type, List<MemberInfo> members, ref List<MemberInfo> newMembers)
	{
		var lastMembers = new List<MemberInfo>(newMembers);
		List<MemberInfo> otherMembers = new List<MemberInfo>();
		Dictionary<MemberInfo, int> offsets = new Dictionary<MemberInfo, int>();
		int superMemberCount = 0;
		foreach (var member in members) {
			bool negative = false;
			var parentMember = GetParentMember(type, member, ref negative);
			if (parentMember == null) { // 无父属性的属性放在最前面
				// 父类属性在前, 本类属性在后
				if (member.DeclaringType == type)
					newMembers.Add(member);
				else
					newMembers.Insert(superMemberCount++, member);
			} else if (lastMembers.Contains(parentMember)) { // 将属性加到父属性后面
				var offset = 0;
				if (!offsets.ContainsKey(parentMember)) {
					offsets.Add(parentMember, ++offset);
				} else
					offset = ++offsets[parentMember];
				newMembers.Insert(newMembers.IndexOf(parentMember) + offset, member);
			} else
				otherMembers.Add(member);
		}

		if (otherMembers.Count > 0)
			SortMemberList(type, otherMembers, ref newMembers);
	}

	// 显示在编辑器面板中
	public static void DisplayInEditor(object inst, float standardWidth)
	{
		var oldAttributeMembers = GetAttributeMembers<JsonAttribute>(inst);
		List<MemberInfo> attributeMembers = new List<MemberInfo>();
		SortMemberList(inst.GetType(), oldAttributeMembers, ref attributeMembers);

		var editorAttributeMembers = GetAttributeMembers<JsonEditorAttribute>(inst);
		List<MemberInfo> handledEditorAttributes = new List<MemberInfo>();
		foreach (var member in attributeMembers) {
			var attribute = GetAttributeByMember<JsonAttribute>(member);
			var labels = attribute.GetLabels(inst);
			string label = labels[0];

			var jeaMember = GetEditorAttributeMember(member, editorAttributeMembers);
			var jeaAttribute = GetAttributeByMember <JsonEditorAttribute>(jeaMember);
			if (jeaAttribute != null) {
				if (!handledEditorAttributes.Contains(jeaMember)) {
					var val = GetValueOfMember(inst, jeaMember);
					switch (jeaAttribute._specialType) {
						case JsonEditorAttribute.SpecialType.NodeIndex:
							var subclassNames = MiscUtils.GetSubclassNames(typeof(Node));
							var nodeIndex = EditorGUILayout.Popup(label, (int)val, subclassNames, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
							SetValueOfMember(ref inst, jeaMember, nodeIndex);
							SetValueOfMember(ref inst, member, subclassNames[nodeIndex]);
							break;
						case JsonEditorAttribute.SpecialType.SoundIndex:
							var keys = AudioConfig.Instance.GetAllKeys();
							int soundIndex = EditorGUILayout.Popup(label, (int)val, keys, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
							SetValueOfMember(ref inst, jeaMember, soundIndex);
							SetValueOfMember(ref inst, member, AudioConfig.Instance.GetIdByKey(keys[soundIndex]));
							break;
					}
					handledEditorAttributes.Add(jeaMember);
				}
			} else {
				var val = GetValueOfMember(inst, member);
				var typeMember = GetTypeOfMember(member);
				var basicModifiedVal = DisplayBasicInEditor(typeMember, label, val, standardWidth);
				if (basicModifiedVal != null) {
					SetValueOfMember(ref inst, member, basicModifiedVal);
				} else {
					if (val is IList) {
						if (((IList)val).Count < labels.Length) {
							if (val is Array) { // Array
								typeMember.InvokeMember("Set", BindingFlags.CreateInstance, null, val, new object[] { labels.Length });
							} else { // List<?>
								for (int i = ((IList)val).Count; i < labels.Length; ++i) {
									object defaultVal = GetDefaultValue(typeMember.GetGenericArguments()[0]);
									typeMember.GetMethod("Add").Invoke(val, new object[]{ defaultVal });
								}
							}
						}
						for (int i = 0; i < labels.Length; ++i) {
							var eleVal = ((IList)val)[i];
							var newEleVal = DisplayBasicInEditor(eleVal.GetType(), labels[i], eleVal, standardWidth);
							if (newEleVal != null) {
								((IList)val)[i] = newEleVal;
							} else {
								DisplayInEditor(val, standardWidth);
							}
						}
						SetValueOfMember(ref inst, member, val);
					} else {
						DisplayInEditor(val, standardWidth);
					}
				}
			}
		}
	}

	static object DisplayBasicInEditor(Type type, string label, object val, float standardWidth)
	{
		if (type == typeof(Int32)) {
			return EditorGUILayout.IntField(label, (int)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Int64)) {
			return EditorGUILayout.LongField(label, (long)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Boolean)) {
			return EditorGUILayout.Toggle(label, (bool)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(String)) {
			return EditorGUILayout.TextField(label, (string)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Single)) {
			return EditorGUILayout.FloatField(label, (float)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Double)) {
			return EditorGUILayout.DoubleField(label, (double)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type.IsEnum) {
			return EditorGUILayout.EnumPopup(label, (Enum)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Rect)) {
			return EditorGUILayout.RectField(label, (Rect)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Vector2)) {
			return EditorGUILayout.Vector2Field(label, (Vector2)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Vector3)) {
			return EditorGUILayout.Vector3Field(label, (Vector3)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		} else if (type == typeof(Vector4)) {
			return EditorGUILayout.Vector4Field(label, (Vector4)val, GUILayout.Width(standardWidth), GUILayout.ExpandWidth(true));
		}

		return null;
	}

	static MemberInfo GetEditorAttributeMember(MemberInfo jsonAttributeMember, List<MemberInfo> jsonEditorAttributeList)
	{
		var jsonAttribute = GetAttributeByMember<JsonAttribute>(jsonAttributeMember);
		if (jsonAttribute != null) {
			foreach (var member in jsonEditorAttributeList) {
				var jsonEditorAttribute = GetAttributeByMember<JsonEditorAttribute>(member);
				if (jsonEditorAttribute._parentKey == jsonAttribute._key) {
					return member;
				}
			}
		}

		return null;
	}

	static MemberInfo GetEditorAttributeMember(MemberInfo jsonAttributeMember, object inst)
	{
		var jsonEditorAttributeList = GetAttributeMembers<JsonEditorAttribute>(inst);
		return GetEditorAttributeMember(jsonAttributeMember, jsonEditorAttributeList);
	}

	#endif

	#endregion

}
