using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Collections.Generic;

public class AutoBuild : EditorWindow
{
	static AutoBuild _currentWindow = null;

	[MenuItem("自定义/按渠道打包 %y", false, 4)]
	static void AddWindow()
	{
		_currentWindow = (AutoBuild)EditorWindow.GetWindowWithRect(typeof(AutoBuild), new Rect(0, 0, 350, 500), true, "按渠道打包");	
		_currentWindow.Show();
	}


	// Key: target platform, it can be Android or iOS.
	// Value: collection of channel, e.g. AppStore, Tencent, Qihu, Baidu, Xiaomi, Huawei, Own or Undefined.
	Dictionary<string, List<string>> _channels = new Dictionary<string, List<string>>();
	Dictionary<string, Dictionary<string, bool>> _needBuild = new Dictionary<string, Dictionary<string, bool>>();

	// Key: target platform, it can be Android, iOS or WP8Player.
	static Dictionary<string, bool> _isDebug = new Dictionary<string, bool>();
	static Dictionary<string, bool> _useStreamingAssetsBundle = new Dictionary<string, bool>();

	void SaveDebug()
	{
		EditorPrefs.SetString("debug", GetStringByDictionary(_isDebug));
	}

	void LoadDebug()
	{
		foreach (var pair in GetDictionaryString(EditorPrefs.GetString("debug"))) {
			_isDebug[pair.Key] = pair.Value;
		}
	}

	// Key: target platform, it can be Android or iOS.
	static Dictionary<string, bool> _isProductionServer = new Dictionary<string, bool>();

	void SaveProductionServer()
	{
		EditorPrefs.SetString("productionServer", GetStringByDictionary(_isProductionServer));
	}

	void LoadProductionServer()
	{
		foreach (var pair in GetDictionaryString(EditorPrefs.GetString("productionServer"))) {
			_isProductionServer[pair.Key] = pair.Value;
		}
	}

	static bool _dontExportXcodeProj = false;
	static bool _exportIpaForAdHoc = false;
	static bool _exportIpaForDistribution = false;

	void Awake()
	{
		_channels.Clear();
		List<string> iOS = new List<string>() { "AppStore" };
		List<string> Android = new List<string>() {
			"Own",
			"Xiaomi",
			"Huawei",
			"360",
			"Tencent",
			"Baidu",
			"Wandoujia",
			"Tencent1",
			"Baidu1",
			"Huawei1"
		};
		_channels.Add("iOS", iOS);
		_channels.Add("Android", Android);
		foreach (var pair in _channels) {
			var platform = pair.Key;
			var channels = pair.Value;
			Dictionary<string, bool> needBuild = new Dictionary<string, bool>();
			foreach (var channel in channels) {
				needBuild.Add(channel, false);
			}
			_needBuild.Add(platform, needBuild);

			_isDebug.Add(platform, true);
			_isProductionServer.Add(platform, false);
			_useStreamingAssetsBundle.Add(platform, true);
		}

		LoadDebug();
		LoadProductionServer();
	}

	string GetStringByDictionary(Dictionary<string, bool> dict)
	{
		string s = "";
		bool first = true;
		foreach (var pair in dict) {
			if (first)
				first = false;
			else
				s += "|";
			s += pair.Key + ":" + pair.Value.ToString();
		}
		return s;
	}

	Dictionary<string, bool> GetDictionaryString(string dictStr)
	{
		Dictionary<string, bool> dict = new Dictionary<string, bool>();
		string[] pairStrs = dictStr.Split('|');
		foreach (string pairStr in pairStrs) {
			string[] keyValue = pairStr.Split(':');
			if (keyValue.Length == 2) {
				dict.Add(keyValue[0], bool.Parse(keyValue[1]));
			}
		}
		return dict;
	}

	void OnGUI()
	{
		bool ok = false;
		foreach (var pair in _channels) {
			var platform = pair.Key;
			if (EditorUserBuildSettings.activeBuildTarget != GetBuildTargetByString(platform))
				continue;

			ok = true;
			GUILayout.Label(pair.Key, EditorStyles.boldLabel);
			var oldProductionServer = _isProductionServer[platform];
			_isProductionServer[platform] = GUILayout.Toggle(_isProductionServer[platform], "正式服务器");
			if (_isProductionServer[platform] != oldProductionServer)
				SaveProductionServer();

			if (!_isProductionServer[platform]) {
				GUILayout.BeginHorizontal();
				GUILayout.Label("当前服务器: " + ConfigurationController.Instance.HttpHostList[DebugConfigController.Instance.HttpHostIndex]);
				if (GUILayout.Button("选中以修改", GUILayout.Width(80))) {
					var serverConfigPath = "Assets/Resources/" + DebugConfigController.DebugConfigControllerPath + ".asset";
					Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(serverConfigPath);
				}
				GUILayout.EndHorizontal();
			}
			
			var oldDebug = _isDebug[platform];
			_isDebug[platform] = GUILayout.Toggle(_isDebug[platform], "Debug模式");
			if (_isDebug[platform] != oldDebug)
				SaveDebug();

			_useStreamingAssetsBundle[platform] = GUILayout.Toggle(_useStreamingAssetsBundle[platform], "使用StreamingAssets中的Bundle");

			var target = GetBuildTargetByString(platform);
			if (GUILayout.Button("Build", GUILayout.Width(250), GUILayout.ExpandWidth(true))) {
				CheckPlatformAndBuild(target, platform, pair.Value, delegate {
					BuildForChannels(pair.Value, platform, false);
				});
			}
			int count = 0;
			foreach (var channel in pair.Value) {
				_needBuild[platform][channel] = GUILayout.Toggle(_needBuild[platform][channel], channel);
				if (_needBuild[platform][channel])
					++count;
			}
			if (target == BuildTarget.iOS) {
				EditorGUILayout.Space();
				_dontExportXcodeProj = GUILayout.Toggle(_dontExportXcodeProj, "不重新导出Xcode工程");
				_exportIpaForAdHoc = GUILayout.Toggle(_exportIpaForAdHoc, "导出为测试ipa包");
				_exportIpaForDistribution = GUILayout.Toggle(_exportIpaForDistribution, "导出为发布ipa包");
			} else {
				if (count == 1) {
					if (GUILayout.Button("Build And Run", GUILayout.Width(250), GUILayout.ExpandWidth(true))) {
						CheckPlatformAndBuild(target, platform, pair.Value, delegate {
							BuildForChannels(pair.Value, platform, true);
						});
					}
				}
			}
		}

		if (!ok) {
			GUILayout.Label("自动打包不支持当前平台: " + EditorUserBuildSettings.activeBuildTarget.ToString());
		}
	}

    static void BuildForIOS()
    {
        _isDebug.Add("iOS", true);
        _useStreamingAssetsBundle.Add("iOS", false);
        _isProductionServer.Add("iOS", false);
        _exportIpaForAdHoc = true;
        _dontExportXcodeProj = false;
        _exportIpaForDistribution = false;
        BuildForChannel("AppStore", "iOS", false);
    }

	void CheckPlatformAndBuild(BuildTarget target, string platform, List<string> channels, Action callback)
	{
		if (EditorUserBuildSettings.activeBuildTarget == target) {
			_currentWindow.Close();
			if (callback != null)
				callback();
		} else {
			EditorUtility.DisplayDialog("Error!", string.Format("请先切换到{0}平台", platform), "确认");
		}
	}

	void BuildForChannels(List<string> channels, string platform, bool buildAndRun)
	{
		foreach (var channel in channels) {
			if (_needBuild[platform][channel])
				BuildForChannel(channel, platform, buildAndRun);
		}
	}

	[PostProcessBuild(0)]
	public static void OnPostprocessBuild(BuildTarget target, string path)
	{
		if (target == BuildTarget.Android) {
			Debug.Log("OnPostprocessBuild: " + path);
			SwitchChannelDefine(target, BuildTargetGroup.Android);
		}

		if (!ConfigurationController.Instance.BuildAssetBundle) {
			AssetDatabase.MoveAsset("Assets/Resources/AssetBundle", "Assets/AssetBundle");
			AssetDatabase.Refresh();
		}
	}

	static void BuildForChannel(string channel, string platform, bool buildAndRun)
	{
		if (!ConfigurationController.Instance.BuildAssetBundle) {
			AssetDatabase.MoveAsset("Assets/AssetBundle", "Assets/Resources/AssetBundle");
			AssetDatabase.Refresh();
		}

		ConfigurationController.Instance.Pack();
		DebugConfigController.Instance.Pack();

		var target = GetBuildTargetByString(platform);

		var platformFolder = Path.GetFullPath(Application.dataPath + "/../" + platform);
		var folder = platformFolder + "/build/";
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
		
		DebugConfigController.Instance._Debug = _isDebug[platform];

		if (!_useStreamingAssetsBundle[platform]) {
			var assetBundleFolder = Application.streamingAssetsPath + "/AssetBundle";
			if (Directory.Exists(assetBundleFolder))
				Directory.Delete(assetBundleFolder, true);
		} else {
			DebugConfigController.Instance._CheckBundleInStreamingAssets = true;
		}

		if (_isProductionServer[platform])
			DebugConfigController.Instance.ForceProductionServer();

		BuildOptions bo = BuildOptions.None;
		if (!_isProductionServer[platform] && _isDebug[platform])
			bo |= BuildOptions.Development;
		if (buildAndRun)
			bo |= BuildOptions.AutoRunPlayer;

		List<string> sceneList = new List<string>();
		foreach (var scene in EditorUtils.GetSceneArrayByEditorScenes ()) {
			//if (!ConfigurationController.Instance.SceneIsBundle(scene)) {
			//	if (DebugConfigController.Instance._Debug || !ConfigurationController.Instance.SceneIsDebug(scene)) {
			//		sceneList.Add(scene);
			//	}
			//}
            sceneList.Add(scene);
		}
		string[] scenes = sceneList.ToArray();
		switch (target) {
			case BuildTarget.Android:
				PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7;
				PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel18;
				PlayerSettings.Android.forceSDCardPermission = true;
				EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
			
				SwitchChannelDefine(target, BuildTargetGroup.Android, channel);

                PlayerSettings.Android.keystoreName = "Android/slmg";
				PlayerSettings.Android.keystorePass = "qwer1234";
				PlayerSettings.Android.keyaliasName = ConfigurationController.Instance._AndroidKeyAlias;
				PlayerSettings.Android.keyaliasPass = "qwer1234";
				
				var apkPath = folder + PlayerSettings.productName + "_" + channel.ToLower() + "_" + MiscUtils.GetVersion() + ".apk";
				BuildPipeline.BuildPlayer(scenes, apkPath, target, bo);
				break;
			case BuildTarget.iOS:
				PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2); // 0 - None, 1 - ARM64, 2 - Universal
				string projectPath = Path.GetFullPath(folder + channel);
				if (!_dontExportXcodeProj) {
					KillXcode();
					if (Directory.Exists(folder + channel))
						Directory.Delete(folder + channel, true);
					BuildPipeline.BuildPlayer(scenes, folder + channel, target, bo);
					OpenXcode(projectPath + "/Unity-iPhone.xcodeproj");
				}
                string scriptName = Application.dataPath + "/Tools/Framework/AutoBuild/export_ipa";
				if (_exportIpaForAdHoc) {
					EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Debug;
					string log = scriptName + "_adhoc.log";
					if (!File.Exists(log))
						File.Create(log);
					EditorUtils.ProcessCommand(scriptName, new string[] {
						projectPath,
						channel + "_adhoc",
						log,
						Path.Combine(platformFolder, "export_ad_hoc.plist"),
					});
				}
				if (_exportIpaForDistribution) {
					EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release;
					string log = scriptName + "_dist.log";
					if (!File.Exists(log))
						File.Create(log);
					EditorUtils.ProcessCommand(scriptName, new string[] {
						projectPath,
						channel + "_dist",
						log,
						Path.Combine(platformFolder, "export_app_store.plist"),
					});
				}
				break;
		}

		ConfigurationController.Instance.Unpack();
		DebugConfigController.Instance.Unpack();
	}

	static void KillXcode()
	{
		EditorUtils.ProcessCommand(Application.dataPath + "/Tools/Framework/AutoBuild/kill_xcode", new string[] { });
	}

	static void OpenXcode(string path)
	{
		EditorUtils.ProcessCommand(Application.dataPath + "/Tools/Framework/AutoBuild/open_xcode", new string[] { path });
	}

	static BuildTarget GetBuildTargetByString(string platform)
	{
		if (platform.Equals("iOS"))
			return BuildTarget.iOS;
		else if (platform.Equals("Android"))
			return BuildTarget.Android;

		return BuildTarget.Android;
	}

	static void SwitchChannelDefine(BuildTarget target, BuildTargetGroup targetGroup, string define = "")
	{
		var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';');
		List<string> newDefines = new List<string>();
		foreach (var d in defines) {
			var dd = d.Trim();
			bool isChannelDefine = false;
			foreach (var channel in _currentWindow._channels[target.ToString()]) {
				if (dd.Equals(channel) || dd.Equals(define)) {
					isChannelDefine = true;
					break;
				}
			}
			if (!isChannelDefine) {
				newDefines.Add(dd);
			}
		}
		if (!string.IsNullOrEmpty(define))
			newDefines.Add(define);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, MiscUtils.AssembleObjectsAsString<string>(newDefines, ";"));
	}

}
