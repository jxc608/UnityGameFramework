using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Snaplingo.Tutorial;
using System.Collections.Generic;

public class EditorAddTutorialAction : EditorWindow
{
	static EditorAddTutorialAction _actionWindow = null;

	EditorTutorialScripts _owner = null;
	TutorialStep _step;
	TutorialScript _script;
	float _buttonWidth;

	public static void Open(EditorTutorialScripts owner, TutorialScript script, TutorialStep step, Vector2 position, float buttonWidth)
	{
		var rect = new Rect(position, new Vector2(200, 200));
		if (_actionWindow == null)
			_actionWindow = EditorWindow.GetWindowWithRect<EditorAddTutorialAction>(rect, true, "添加行为");
		else
			_actionWindow.position = rect;
		
		_actionWindow._owner = owner;
		_actionWindow._step = step;
		_actionWindow._script = script;
		_actionWindow._buttonWidth = buttonWidth;
		_actionWindow.Show();
	}

	Type[] _actionSubclasses = null;
	string[] _actionSubclassNames = null;
	int index = 0;

	void Awake()
	{
		_actionSubclasses = MiscUtils.GetSubclassTypes(typeof(TutorialAction));
		_actionSubclassNames = MiscUtils.GetSubclassNames(typeof(TutorialAction), _actionSubclasses);
	}

	void OnGUI()
	{
		index = EditorGUILayout.Popup(index, _actionSubclassNames);
		if (GUILayout.Button("添加行为", GUILayout.Width(_buttonWidth))) {
			if (_step != null)
				_owner.CreateAction(_step, _actionSubclasses[index]);
			if (_script != null)
				_owner.CreateAction(_script, _actionSubclasses[index]);
			_actionWindow.Close();
		}
	}
}

public class EditorAddTutorialEvent : EditorWindow
{
	static EditorAddTutorialEvent _eventWindow = null;

	EditorTutorialScripts _owner = null;
	TutorialStep _step;
	TutorialScript _script;
	float _buttonWidth;

	public static void Open(EditorTutorialScripts owner, TutorialScript script, TutorialStep step, Vector2 position, float buttonWidth)
	{
		var rect = new Rect(position, new Vector2(200, 200));
		if (_eventWindow == null)
			_eventWindow = EditorWindow.GetWindowWithRect<EditorAddTutorialEvent>(rect, true, "添加事件");
		else
			_eventWindow.position = rect;

		_eventWindow._owner = owner;
		_eventWindow._step = step;
		_eventWindow._script = script;
		_eventWindow._buttonWidth = buttonWidth;
		_eventWindow.Show();
	}

	Type[] _eventSubclasses = null;
	string[] _eventSubclassNames = null;
	int index = 0;

	void Awake()
	{
		_eventSubclasses = MiscUtils.GetSubclassTypes(typeof(TutorialEvent));
		_eventSubclassNames = MiscUtils.GetSubclassNames(typeof(TutorialEvent), _eventSubclasses);
	}

	void OnGUI()
	{
		index = EditorGUILayout.Popup(index, _eventSubclassNames);
		if (GUILayout.Button("添加事件", GUILayout.Width(_buttonWidth))) {
			if (_step != null)
				_owner.CreateEvent(_step, _eventSubclasses[index]);
			if (_script != null)
				_owner.CreateEvent(_script, _eventSubclasses[index]);
			_eventWindow.Close();
		}
	}
}

public class EditorAddTutorialScript : EditorWindow
{
	static EditorAddTutorialScript _eventWindow = null;

	EditorTutorialScripts _owner = null;
	float _buttonWidth;

	public static void Open(EditorTutorialScripts owner, Vector2 position, float buttonWidth)
	{
		var rect = new Rect(position, new Vector2(300, 200));
		if (_eventWindow == null)
			_eventWindow = EditorWindow.GetWindowWithRect<EditorAddTutorialScript>(rect, true, "添加教学脚本");
		else
			_eventWindow.position = rect;

		_eventWindow._owner = owner;
		_eventWindow._buttonWidth = buttonWidth;
		_eventWindow.Show();
	}

	string[] _resourceFolders = null;
	string[] _resourceFolderNames = null;
	string[] _resourceSubfolders = null;
	string[] _resourceSubfolderNames = null;

	void Awake()
	{
		_resourceFolders = ResourceLoadUtils.GetResourceFolders();
		_resourceFolderNames = new string[_resourceFolders.Length];
		for (int i = 0; i < _resourceFolders.Length; ++i) {
			_resourceFolderNames[i] = _resourceFolders[i].Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/") + "/", "").Replace("/", "");
		}
		_resourceSubfolders = ResourceLoadUtils.GetSubfolders();
		_resourceSubfolderNames = new string[_resourceSubfolders.Length];
		for (int i = 0; i < _resourceSubfolders.Length; ++i) {
			if (_resourceSubfolders[i].Length == 0)
				_resourceSubfolderNames[i] = "root";
			else
				_resourceSubfolderNames[i] = _resourceSubfolders[i].Replace("/", "-");
		}
	}

	string _newScriptName = "";
	int _resourceFolderIndex = 0;
	int _resourceSubfolderIndex = 0;

	void OnGUI()
	{
		GUILayout.Label("脚本名");
		_newScriptName = GUILayout.TextField(_newScriptName, GUILayout.Width(_buttonWidth), GUILayout.ExpandWidth(true));
		_resourceFolderIndex = EditorGUILayout.Popup("资源文件夹", _resourceFolderIndex, _resourceFolderNames, GUILayout.Width(_buttonWidth), GUILayout.ExpandWidth(true));
		_resourceSubfolderIndex = EditorGUILayout.Popup("子文件夹", _resourceSubfolderIndex, _resourceSubfolderNames, GUILayout.Width(_buttonWidth), GUILayout.ExpandWidth(true));
		if (GUILayout.Button("添加教学脚本", GUILayout.Width(_buttonWidth))) {
			_owner.CreateScript(Path.Combine(_resourceFolders[_resourceFolderIndex], _resourceSubfolders[_resourceSubfolderIndex]), _newScriptName);
			_eventWindow.Close();
		}
	}

}

public class EditorTutorialScripts : EditorWindow
{
	static EditorTutorialScripts _currentWindow = null;

	static float _margin = 10f;
	static float _areaHeight = 600f;
	static float _scriptTreeWidth = 200f;
	static float _scriptInfoHeight = 160f;
	static float _minScriptInfoWidth = 800f;
	static float _buttonWidth = 100f;
	static float _buttonWidthSmall = 70f;

	[MenuItem("自定义/教学/脚本编辑器 %t", false, 4)]
	static void OpenTutorialScript()
	{
		_currentWindow = EditorWindow.GetWindow<EditorTutorialScripts>(false, "教学编辑器");
		_currentWindow.minSize = new Vector2(_margin * 3 + _scriptTreeWidth + _minScriptInfoWidth, _areaHeight + _margin * 2);
		_currentWindow.Show();
	}

	int _selectedScriptIndex = 0;
	int _lastScriptIndex = 0;
	Dictionary<string, List<FileInfo>> _scriptGroupFileInfos = new Dictionary<string, List<FileInfo>>();
	Dictionary<string, bool> _scriptGroupOpen = new Dictionary<string, bool>();
	string _currentKey = "";
	string _lastKey = "";

	TutorialScript _currentScript = null;
	Vector2 _scriptListScrollPos = new Vector2();
	Vector2 _scriptScrollPos = new Vector2();
	Vector2 _scriptInfoScrollPos = new Vector2();
	string newTag = "";
	List<bool> _stepShowFlags = new List<bool>();
	List<bool> _stepActionShowFlags = new List<bool>();
	List<bool> _stepEventShowFlags = new List<bool>();
	List<string> _stepNewTags = new List<string>();
	bool _scriptActionShowFlags = true;
	bool _scriptEventShowFlags = true;

	void ResetVariables(TutorialScript tutorialScript)
	{
		_stepShowFlags.Clear();
		_stepActionShowFlags.Clear();
		_stepEventShowFlags.Clear();
		_stepNewTags.Clear();
		foreach (var step in tutorialScript._steps) {
			_stepShowFlags.Add(true);
			_stepActionShowFlags.Add(true);
			_stepEventShowFlags.Add(true);
			_stepNewTags.Add(step._tag);
		}
	}

	void Awake()
	{
		RefreshFileList();
	}

	void OnDestroy()
	{
		CheckSaveCurrentScript("退出");
	}

	void OnFocus()
	{
		if (_currentWindow)
			_currentWindow.Repaint();
	}

	void OnGUI()
	{
		if (_currentWindow == null)
			return;
		
		_lastScriptIndex = _selectedScriptIndex;
		_lastKey = _currentKey;

		// 左侧脚本列表
		if (DrawScriptList())
			return;

		// 脚本信息
		if (HasFileInCurrentGroup(_currentKey)) {
			if (DrawScriptInfo())
				return;
			if (DrawScriptStepList())
				return;
		}
	}

	bool HasFileInCurrentGroup(string key)
	{
		return !string.IsNullOrEmpty(key) && _scriptGroupFileInfos.ContainsKey(key) && _scriptGroupFileInfos[key].Count > 0;
	}

	void SetCurrentKey(string key)
	{
		_currentKey = key;
		foreach (var k in _scriptGroupFileInfos.Keys) {
			_scriptGroupOpen[k] = k == key;
		}
	}

	// 左侧脚本列表
	bool DrawScriptList()
	{
		GUILayout.BeginArea(new Rect(_margin, _margin, 
		                               _scriptTreeWidth, _currentWindow.position.height - _margin * 2), 
		                     GUI.skin.GetStyle("AS TextArea"));
		_scriptListScrollPos = EditorGUILayout.BeginScrollView(_scriptListScrollPos);

		if (GUILayout.Button("添加脚本", GUILayout.ExpandWidth(true))) {
			EditorAddTutorialScript.Open(this, new Vector2(_scriptTreeWidth, _margin), _buttonWidth);
		}

		EditorGUILayout.Space();

		// 所有脚本列表
		foreach (var pair in _scriptGroupFileInfos) {
			var key = pair.Key;
			var list = pair.Value;
			bool foldout = _scriptGroupOpen[key];
			if (list.Count > 0)
				_scriptGroupOpen[key] = EditorGUILayout.Foldout(_scriptGroupOpen[key], key.Trim('/'));
			else
				_scriptGroupOpen[key] = false;
			if (_scriptGroupOpen[key]) {
				if (foldout == false) {
					_selectedScriptIndex = 0;
					_currentScript = null;
				}
				SetCurrentKey(key);

				var files = new string[list.Count];
				for (int i = 0; i < list.Count; ++i) {
					files[i] = list[i].FullName.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/") + "/", "")
						.Replace(".json", "").Replace(key, "").Replace(TutorialManager.TutorialScriptPath, "").Trim('/');
				}
				var oldIndex = _selectedScriptIndex;
				_selectedScriptIndex = GUILayout.SelectionGrid(_selectedScriptIndex, files, 1);
				if (oldIndex != _selectedScriptIndex)
					_currentScript = null;
			}
		}
		PrepareCurrentScript();

		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();

		return false;
	}

	// 绘制当前脚本信息
	bool DrawScriptInfo()
	{
		if (_currentScript != null) {
			if (_currentScript._actions.Count > 0 || _currentScript._events.Count > 0)
				_scriptInfoHeight = 240f;
			else
				_scriptInfoHeight = 160f;
		}
		
		GUILayout.BeginArea(new Rect(_margin * 2 + _scriptTreeWidth, _margin, 
		                               _currentWindow.position.width - _margin * 3 - _scriptTreeWidth, _scriptInfoHeight), 
		                     GUI.skin.GetStyle("RL Background"));
		_scriptInfoScrollPos = EditorGUILayout.BeginScrollView(_scriptInfoScrollPos);

		EditorGUILayout.Space();
		
		// 脚本操作
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("保存所有脚本", GUILayout.Width(_buttonWidth))) {
			foreach (var infoList in _scriptGroupFileInfos.Values) {
				foreach (var info in infoList) {
					string json = File.ReadAllText(info.FullName);
					var script = TutorialScript.LoadFromJson(json);
					SaveScript(script, info.FullName);
				}
			}
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("删除脚本", GUILayout.Width(_buttonWidthSmall))) {
			if (HasFileInCurrentGroup(_currentKey) && EditorUtility.DisplayDialog("⚠️ 提示!", string.Format("确定要删除名为{0}的脚本?", _scriptGroupFileInfos[_currentKey][_selectedScriptIndex].Name), 
			                                                                        "确认", "取消")) {
				File.Delete(_scriptGroupFileInfos[_currentKey][_selectedScriptIndex].FullName);
				AssetDatabase.Refresh();
				_currentScript = null;
				RefreshFileList();
			}
			return true;
		}
		if (HasFileInCurrentGroup(_currentKey) && GUILayout.Button("选择脚本文件", GUILayout.Width(_buttonWidth))) {
			var relativePath = EditorUtils.GetRelativePathByAbsolutePath(_scriptGroupFileInfos[_currentKey][_selectedScriptIndex].FullName);
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
		}
		if (HasFileInCurrentGroup(_currentKey) && GUILayout.Button("保存脚本", GUILayout.Width(_buttonWidthSmall))) {
			SaveCurrentScript(_scriptGroupFileInfos[_currentKey][_selectedScriptIndex].FullName);
		}
		EditorGUILayout.EndHorizontal();

		// 显示脚本名称和初始标签
		EditorGUILayout.Space();
		_currentScript._startTag = EditorGUILayout.TextField("初始标签", _currentScript._startTag, 
		                                                      GUILayout.Width(_buttonWidth), GUILayout.ExpandWidth(true));

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		// 增加一个步骤
		EditorGUILayout.BeginHorizontal();
		newTag = EditorGUILayout.TextField(newTag, GUILayout.Width(_buttonWidth), GUILayout.ExpandWidth(true));
		if (GUILayout.Button("添加步骤", GUILayout.Width(_buttonWidth))) {
			if (string.IsNullOrEmpty(newTag)) {
				EditorUtility.DisplayDialog("❌错误!", "未指定tag", "确认");
			} else {
				if (!_currentScript.HasStep(newTag)) {
					_currentScript._steps.Add(new TutorialStep() { _tag = newTag });
					_stepShowFlags.Add(true);
					_stepActionShowFlags.Add(true);
					_stepEventShowFlags.Add(true);
					_stepNewTags.Add(newTag);
				} else
					EditorUtility.DisplayDialog("❌错误!", "tag已存在", "确认");
			}
		}
		// 展开和收起所有
		if (GUILayout.Button("展开所有", GUILayout.Width(_buttonWidthSmall))) {
			for (int i = 0; i < _stepShowFlags.Count; ++i) {
				_stepShowFlags[i] = true;
			}
		}
		if (GUILayout.Button("收起所有", GUILayout.Width(_buttonWidthSmall))) {
			for (int i = 0; i < _stepShowFlags.Count; ++i) {
				_stepShowFlags[i] = false;
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		var areaRect = new Rect(_margin * 2 + _scriptTreeWidth, _margin * 2 + _scriptInfoHeight,
		                         _currentWindow.position.width - _margin * 3 - _scriptTreeWidth,
		                         _currentWindow.position.height - _margin * 3 - _scriptInfoHeight);
		EditorGUILayout.BeginHorizontal();
		// 行为列表
		DrawActions(ref _currentScript._actions, ref _scriptActionShowFlags, _currentScript, null, areaRect.center, true);
		GUILayout.FlexibleSpace();
		DrawEvents(ref _currentScript._events, ref _scriptEventShowFlags, _currentScript, null, areaRect.center, true);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		GUILayout.EndArea();

		return false;
	}

	void DrawActions(ref List<TutorialAction> actions, ref bool show, TutorialScript ts, TutorialStep step, Vector2 pos, bool isPublic)
	{
		// 行为列表
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		show = EditorGUILayout.Foldout(show, (isPublic ? "公共" : "") + "行为列表");
		if (GUILayout.Button("添加" + (isPublic ? "公共" : "") + "行为", GUILayout.Width(_buttonWidth))) {
			EditorAddTutorialAction.Open(_currentWindow, ts, step, pos, _buttonWidth);
		}
		EditorGUILayout.EndHorizontal();
		if (show) {
			List<TutorialAction> removeActions = new List<TutorialAction>();
			for (int j = 0; j < actions.Count; ++j) {
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("行为" + (j + 1) + ": " + actions[j].GetType().ToString());
				GUILayout.FlexibleSpace(); 
				if (GUILayout.Button("删除行为", GUILayout.Width(_buttonWidth))) {
					removeActions.Add(actions[j]);
					break;
				}
				EditorGUILayout.EndHorizontal();
				JsonUtils.DisplayInEditor(actions[j], _buttonWidth);
			}
			for (int j = 0; j < removeActions.Count; ++j) {
				actions.Remove(removeActions[j]);
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

	void DrawEvents(ref List<TutorialEvent> events, ref bool show, TutorialScript ts, TutorialStep step, Vector2 pos, bool isPublic)
	{
		// 事件列表
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		show = EditorGUILayout.Foldout(show, (isPublic ? "公共" : "") + "事件列表");
		if (GUILayout.Button("添加" + (isPublic ? "公共" : "") + "事件", GUILayout.Width(_buttonWidth))) {
			EditorAddTutorialEvent.Open(_currentWindow, ts, step, pos, _buttonWidth);
		}
		EditorGUILayout.EndHorizontal();
		if (show) {
			List<TutorialEvent> removeEvents = new List<TutorialEvent>();
			for (int j = 0; j < events.Count; ++j) {
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("事件" + (j + 1) + ": " + events[j].GetType().ToString());
				GUILayout.FlexibleSpace(); 
				if (GUILayout.Button("删除事件", GUILayout.Width(_buttonWidth))) {
					removeEvents.Add(events[j]);
					break;
				}
				EditorGUILayout.EndHorizontal();
				JsonUtils.DisplayInEditor(events[j], _buttonWidth);
			}
			for (int j = 0; j < removeEvents.Count; ++j) {
				events.Remove(removeEvents[j]);
			}
		}
		EditorGUILayout.EndVertical();
	}

	// 绘制当前脚本步骤列表
	bool DrawScriptStepList()
	{
		var areaRect = new Rect(_margin * 2 + _scriptTreeWidth, _margin * 2 + _scriptInfoHeight, 
		                         _currentWindow.position.width - _margin * 3 - _scriptTreeWidth, 
		                         _currentWindow.position.height - _margin * 3 - _scriptInfoHeight);
		GUILayout.BeginArea(areaRect, GUI.skin.GetStyle("RL Background"));
		_scriptScrollPos = EditorGUILayout.BeginScrollView(_scriptScrollPos);

		GUIStyle style = new GUIStyle();
		style.richText = true;
		
		for (int i = 0; i < _currentScript._steps.Count; ++i) {
			var tag = _currentScript._steps[i]._tag;
			var step = _currentScript._steps[i];

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			// 步骤折叠及其标签
			string seperator = "==============================================================================================================================================================================================================";
			_stepShowFlags[i] = EditorGUILayout.Foldout(_stepShowFlags[i], tag + (_stepShowFlags[i] ? seperator : ""));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			// 重命名步骤标签
			_stepNewTags[i] = EditorGUILayout.TextField(_stepNewTags[i], GUILayout.Width(_buttonWidth), GUILayout.ExpandWidth(true));
			if (GUILayout.Button("重命名", GUILayout.Width(_buttonWidthSmall))) {
				if (!_currentScript.HasStep(_stepNewTags[i]) || step._tag == _stepNewTags[i]) {
					for (int j = 0; j < _currentScript._steps.Count; ++j) {
						for (int k = 0; k < _currentScript._steps[j]._events.Count; ++k) {
							if (_currentScript._steps[j]._events[k]._nextTag == step._tag) {
								_currentScript._steps[j]._events[k]._nextTag = _stepNewTags[i];
							}
						}
					}
					step._tag = _stepNewTags[i];
				} else
					EditorUtility.DisplayDialog("❌错误!", "tag已存在", "确认");
			}
			// 移除步骤按钮
			if (GUILayout.Button("删除", GUILayout.Width(_buttonWidthSmall))) {
				RemoveStep(i, tag);
				return true;
			}
			if (i > 0 && GUILayout.Button("上移", GUILayout.Width(_buttonWidthSmall))) {
				var temp = _currentScript._steps[i - 1];
				_currentScript._steps[i - 1] = _currentScript._steps[i];
				_currentScript._steps[i] = temp;
			}
			if (i < _currentScript._steps.Count - 1 && GUILayout.Button("下移", GUILayout.Width(_buttonWidthSmall))) {
				var temp = _currentScript._steps[i + 1];
				_currentScript._steps[i + 1] = _currentScript._steps[i];
				_currentScript._steps[i] = temp;
			}
			EditorGUILayout.EndHorizontal();
			if (_stepShowFlags[i]) {

				EditorGUILayout.Space();


				EditorGUILayout.BeginHorizontal();

				bool show = _stepActionShowFlags[i];
				DrawActions(ref step._actions, ref show, null, step, areaRect.center, false);
				_stepActionShowFlags[i] = show;

				GUILayout.FlexibleSpace();

				show = _stepEventShowFlags[i];
				DrawEvents(ref step._events, ref show, null, step, areaRect.center, false);
				_stepEventShowFlags[i] = show;

				EditorGUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();

		return false;
	}

	public void CreateAction(TutorialStep step, Type type)
	{
		step._actions.Add(Activator.CreateInstance(type) as TutorialAction);
	}

	public void CreateAction(TutorialScript script, Type type)
	{
		script._actions.Add(Activator.CreateInstance(type) as TutorialAction);
	}

	public void CreateEvent(TutorialStep step, Type type)
	{
		step._events.Add(Activator.CreateInstance(type) as TutorialEvent);
	}

	public void CreateEvent(TutorialScript script, Type type)
	{
		script._events.Add(Activator.CreateInstance(type) as TutorialEvent);
	}

	public void CreateScript(string folder, string newScriptName)
	{
		bool create = true;
		var fileFullPath = Path.Combine(Path.Combine(folder, TutorialManager.TutorialScriptPath), newScriptName + ".json");
		if (File.Exists(fileFullPath)) {
			if (!EditorUtility.DisplayDialog("⚠️ 警告!", string.Format("已存在名为{0}的脚本, 是否覆盖?", newScriptName), "确认", "取消")) {
				create = false;
			}
		}
		if (create) {
			var parentFolder = Path.GetDirectoryName(fileFullPath);
			if (!Directory.Exists(parentFolder))
				Directory.CreateDirectory(parentFolder);
			File.Create(fileFullPath).Close();
			AssetDatabase.Refresh();
			_currentScript = null;
			RefreshFileList(fileFullPath);
			SaveCurrentScript(fileFullPath);
		}
	}

	void RemoveStep(int i, string tag)
	{
		if (EditorUtility.DisplayDialog("⚠️ 提示!", string.Format("确定要删除步骤{0}?", tag), "确认", "取消")) {
			_stepShowFlags.RemoveAt(i);
			_stepActionShowFlags.RemoveAt(i);
			_stepEventShowFlags.RemoveAt(i);
			_stepNewTags.RemoveAt(i);
			_currentScript._steps.RemoveAt(i);
		}
	}

	// 准备当前脚本信息
	void PrepareCurrentScript()
	{
		if (HasFileInCurrentGroup(_currentKey)) {
			bool needReset = false;
			if (_currentScript == null)
				needReset = true;
			else {
				if (_selectedScriptIndex != _lastScriptIndex || _currentKey != _lastKey) {
					CheckSaveCurrentScript("切换");
					needReset = true;
				}
			}
			if (needReset) {
				if (_scriptGroupFileInfos[_currentKey].Count > _selectedScriptIndex) {
					string json = File.ReadAllText(_scriptGroupFileInfos[_currentKey][_selectedScriptIndex].FullName);
					_currentScript = TutorialScript.LoadFromJson(json);
					ResetVariables(_currentScript);
				}
			}
		}
	}

	void CheckSaveCurrentScript(string nextActionName)
	{
		if (_currentWindow && HasFileInCurrentGroup(_lastKey)) {
			if (_lastScriptIndex >= 0 && _lastScriptIndex < _scriptGroupFileInfos[_lastKey].Count) {
				FileInfo lastFileInfo = _scriptGroupFileInfos[_lastKey][_lastScriptIndex];
				if (_currentScript != null && File.Exists(lastFileInfo.FullName)) {
					var fileContent = File.ReadAllText(lastFileInfo.FullName);
					var newFileContent = _currentScript.SaveAsJson();
					if (!fileContent.Equals(newFileContent)) {
						Debug.Log(fileContent);
						Debug.Log(newFileContent);
						if (EditorUtility.DisplayDialog("⚠️ 警告!", string.Format("当前脚本{0}有改动, 是否保存?", lastFileInfo.Name), 
						                                 "保存并" + nextActionName, "放弃并" + nextActionName)) {
							SaveCurrentScript(_scriptGroupFileInfos[_currentKey][_lastScriptIndex].FullName);
						}
					}
				}
			} else {
				Debug.Log(_lastKey);
				Debug.Log(_scriptGroupFileInfos[_lastKey].Count);
				Debug.Log(_lastScriptIndex);
			}
		}
	}

	void SaveCurrentScript(string path)
	{
		SaveScript(_currentScript, path);
	}

	void SaveScript(TutorialScript script, string path)
	{
		if (script != null) {
			var json = script.SaveAsJson();
			File.WriteAllText(path, json);
			AssetDatabase.Refresh();
		}
	}

	// 刷新文件信息
	void RefreshFileList(string focusFilePath = "")
	{
		_scriptGroupFileInfos.Clear();
		foreach (var folder in ResourceLoadUtils.GetSubfolderFullPaths ()) {
			var fileInfos = MiscUtils.GetFileInfoFromFolder(Path.Combine(folder, TutorialManager.TutorialScriptPath), "*.json", SearchOption.AllDirectories, ".meta");

			var key = folder.Replace(Application.dataPath.Replace("\\", "/") + "/", "");
			_scriptGroupFileInfos[key] = fileInfos;
			if (!_scriptGroupOpen.ContainsKey(key))
				_scriptGroupOpen.Add(key, false);
		}
		ChooseAppropriateScript(focusFilePath);
	}

	// 选择一个合适的脚本
	void ChooseAppropriateScript(string focusFilePath = "")
	{
		if (!string.IsNullOrEmpty(focusFilePath)) {
			foreach (var pair in _scriptGroupFileInfos) {
				for (int i = 0; i < pair.Value.Count; ++i) {
					if (focusFilePath.Equals(pair.Value[i].FullName)) {
						SetCurrentKey(pair.Key);
						_selectedScriptIndex = i;
						PrepareCurrentScript();
						return;
					}
				}
			}
		}

		if (string.IsNullOrEmpty(_currentKey) || _scriptGroupFileInfos[_currentKey].Count <= 0) {
			foreach (var key in _scriptGroupFileInfos.Keys) {
				if (_scriptGroupFileInfos[key].Count > 0) {
					SetCurrentKey(key);
					_scriptGroupOpen[key] = true;
					_selectedScriptIndex = 0;
					break;
				}
			}
		} else
			_selectedScriptIndex = Mathf.Clamp(_selectedScriptIndex, 0, _scriptGroupFileInfos[_currentKey].Count);
	}

}
