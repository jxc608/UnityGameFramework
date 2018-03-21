using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.UI;
using LitJson;
using System;
using System.Text.RegularExpressions;

public class MiscUtils
{
	static Dictionary<string, int> _milliseconds = new Dictionary<string, int>();

	public static string StartRecordTime()
	{
		var key = Guid.NewGuid().ToString("N");
		_milliseconds.Add(key, DateTime.Now.Millisecond);
		return key;
	}

	public static void EndRecordTime(string key, string label)
	{
		Debug.Log(label + " takes " + (DateTime.Now.Millisecond - _milliseconds[key]) + " milliseconds");
		_milliseconds.Remove(key);
	}

	#region reflection utils

	public static string[] GetSubclassNames(Type type, Type[] subclasses = null)
	{
		if (subclasses == null)
			subclasses = GetSubclassTypes(type);
		var subclassNames = new string[subclasses.Length];
		for (int i = 0; i < subclasses.Length; ++i) {
			subclassNames[i] = subclasses[i].ToString();
		}
		return subclassNames;
	}

	public static Type[] GetSubclassTypes(Type type)
	{
		List<Type> types = new List<Type>();
		var allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
		foreach (var t in allTypes) {
			if (IsSubclassOf(t, type)) {
				types.Add(t);
			}
		}
		return types.ToArray();
	}

	public static bool IsSubclassOf(Type type, Type baseType)
	{
		var b = type.BaseType;
		while (b != null) {
			if (b.Equals(baseType)) {
				return true;
			}
			b = b.BaseType;
		}
		return false;
	}

	public static Type[] GetTypesHaveInterface(Type interfaceType)
	{
		List<Type> types = new List<Type>();
		var allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
		foreach (var t in allTypes) {
			if (HasInterface(t, interfaceType)) {
				types.Add(t);
			}
		}
		return types.ToArray();
	}

	public static bool HasInterface(Type type, Type interfaceType)
	{
		return type.GetInterface(interfaceType.ToString()) != null;
	}

	#endregion

	// 双线性插值法缩放图片
	public static Texture2D ScaleTextureBilinear(Texture2D originalTexture, float scaleFactor)
	{
		Texture2D newTexture = new Texture2D(Mathf.CeilToInt(originalTexture.width * scaleFactor), Mathf.CeilToInt(originalTexture.height * scaleFactor));
		float scale = 1.0f / scaleFactor;
		int maxX = originalTexture.width - 1;
		int maxY = originalTexture.height - 1;
		for (int y = 0; y < newTexture.height; y++) {
			for (int x = 0; x < newTexture.width; x++) {
				// Bilinear Interpolation  
				float targetX = x * scale;
				float targetY = y * scale;
				int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
				int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
				int x2 = Mathf.Min(maxX, x1 + 1);
				int y2 = Mathf.Min(maxY, y1 + 1);

				float u = targetX - x1;
				float v = targetY - y1;
				float w1 = (1 - u) * (1 - v);
				float w2 = u * (1 - v);
				float w3 = (1 - u) * v;
				float w4 = u * v;
				Color color1 = originalTexture.GetPixel(x1, y1);
				Color color2 = originalTexture.GetPixel(x2, y1);
				Color color3 = originalTexture.GetPixel(x1, y2);
				Color color4 = originalTexture.GetPixel(x2, y2);
				Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
				                         Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
				                         Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
				                         Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
				              );
				newTexture.SetPixel(x, y, color);

			}
		}
		newTexture.Apply();
		return newTexture;
	}

	public static DateTime GetDateTimeByTimeStamp(int timeStamp)
	{
		DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
		return startTime.AddSeconds(timeStamp);
	}

	public static List<FileInfo> GetFileInfoFromFolder(string folderPath, string searchPattern, SearchOption option, params string[] ignoreSuffix)
	{
		List<FileInfo> fileInfos = new List<FileInfo>();
		DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
		if (dirInfo.Exists) {
			FileInfo[] fis = dirInfo.GetFiles(searchPattern, option);
			if (fis.Length > 0) {
				for (int i = 0; i < fis.Length; i++) {
					bool ignore = false;
					if (fis[i].Name.EndsWith(".DS_Store"))
						ignore = true;
					else {
						for (int j = 0; j < ignoreSuffix.Length; ++j) {
							if (fis[i].Name.EndsWith(ignoreSuffix[j])) {
								ignore = true;
								break;
							}
						}
					}
					if (!ignore) {
						fileInfos.Add(fis[i]);
					}
				}
			}
		}

		return fileInfos;
	}

	public static string GetMD5HashFromFile(string fileName)
	{
		FileStream file = new FileStream(fileName, FileMode.Open);
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] retVal = md5.ComputeHash(file);
		file.Close();
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < retVal.Length; i++)
			sb.Append(retVal[i].ToString("x2"));
		return sb.ToString();
	}

	public static string GetSha1Hash(string input)
	{
		SHA1 sha1 = new SHA1CryptoServiceProvider();
		byte[] inputBytes = System.Text.UTF8Encoding.Default.GetBytes(input);
		byte[] outputBytes = sha1.ComputeHash(inputBytes);
		string output = System.BitConverter.ToString(outputBytes).Replace("-", "");
		return output.ToLower();
	}

	public static string GetMd5Hash(string input)
	{
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] inputBytes = System.Text.UTF8Encoding.Default.GetBytes(input);
		byte[] outputBytes = md5.ComputeHash(inputBytes);
		string output = System.BitConverter.ToString(outputBytes).Replace("-", "");
		return output.ToLower();
	}

	public static string GetMd5HashFromBytes(byte[] inputBytes)
	{
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] outputBytes = md5.ComputeHash(inputBytes);
		string output = System.BitConverter.ToString(outputBytes).Replace("-", "");
		return output.ToLower();
	}

	#if UNITY_IPHONE
	[DllImport("__Internal")]
	static extern string _GetVersionNumber();
	#endif
	public static string GetVersion()
	{
		string version = "";
		#if UNITY_EDITOR
		version = UnityEditor.PlayerSettings.bundleVersion;
		#elif UNITY_ANDROID
		AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.Utils");
		version = utils.CallStatic<string>("getVersionName", currentActivity);
		#elif UNITY_IPHONE
		version = _GetVersionNumber ();
		#endif
		return version;
	}
	
	#if UNITY_IPHONE
	[DllImport("__Internal")]
	static extern string _GetIPWithHostName(string hostName);
	#endif
	public static string GetIPWithHostName(string hostName)
	{
		#if UNITY_EDITOR
		return "";
		#elif UNITY_IPHONE
		return _GetIPWithHostName (hostName);
		#elif UNITY_ANDROID
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.wckt.Utils");
		return utils.CallStatic<string>("getInetAddress", hostName);
		#else
		return "";
		#endif
	}

	public static string GetChannel()
	{
		string channel = "";
		bool ignoreDebug = false;
		#if UNITY_EDITOR
		channel = "Editor";
		#elif UNITY_ANDROID
		#if Tencent
		channel = "Tencent";
		#elif Qihu
		channel = "Qihu";
		#elif Baidu
		channel = "Baidu";
		#elif Xiaomi
		channel = "Xiaomi";
		#elif Huawei
		channel = "Huawei";
		#elif Own
		channel = "Own";
		#elif Tencent1
		channel = "Tencent1";
		ignoreDebug = true;
		#elif Baidu1
		channel = "Baidu1";
		ignoreDebug = true;
		#elif Huawei1
		channel = "Huawei1";
		ignoreDebug = true;
		#endif
		#elif UNITY_IPHONE
		channel = "AppStore";
		#endif
		if (DebugConfigController.Instance._Debug && !ignoreDebug)
			channel += "_Debug";
		return channel;
	}

	public static string GetCurrentPlatform()
	{
		#if UNITY_ANDROID
		return "Android";
		#elif UNITY_IPHONE
		return "iOS";
		#else
		return "iOS";
		#endif
	}
	
	// Key和Value都需支持ToString
	public static JsonData GetJsonDataByDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
	{
		JsonData data = new JsonData();
		foreach (var p in dict) {
			data[p.Key.ToString()] = p.Value.ToString();
		}
		return data;
	}

	public static void GetCameraRawImage(ref RawImage img, Camera ca)
	{
		if (img && ca) {
			RenderTexture rt = new RenderTexture((int)img.GetComponent<RectTransform>().rect.width, (int)img.GetComponent<RectTransform>().rect.height, 24, RenderTextureFormat.ARGB32);
			rt.useMipMap = true;
			rt.filterMode = FilterMode.Trilinear;
			rt.antiAliasing = 4;
			rt.Create();
			ca.targetTexture = rt;
			img.texture = rt;
		} else
			LogManager.Log("RowImage或Camera不存在！");
	}

	/// <summary>
	/// 判断当前网络状态
	/// </summary>
	/// <returns></returns>
	public static bool IsOnline()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			return false;
		}
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) {
			return true;
		}
		if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) {
			return true;
		}
		return false;
	}

	public static string AssembleObjectsAsString<T>(List<T> list, string separator)
	{
		string s = "";
		bool first = true;
		foreach (T item in list) {
			if (first)
				first = false;
			else
				s += separator;
			s += item.ToString();
		}
		return s;
	}

	public static void NickNameLengthCheck(InputField input)
	{
		string txt = input.text;
		string res = "";
		int count = 0;
		for (int i = 0; i < txt.Length; i++) {
			if (Regex.IsMatch(txt[i].ToString(), @"[\u4e00-\u9fbb]+$"))
				count += 2;
			else
				count++;
			if (count <= 12)
				res += txt[i];
			else
				break;
		}
		input.text = res;
	}

	public static bool IsValidNickName(string name)
	{
		Regex nameRegex = new Regex(@"^[\u4e00-\u9fa5_a-zA-Z0-9]+$");
		if (nameRegex.Match(name).Success && name.Length < 30)
			return true;
		PromptManager.Instance.MessageBox(PromptManager.Type.FloatingTip, LocalizationConfig.Instance.GetStringById(2030));
		return false;
	}
}
