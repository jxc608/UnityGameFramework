using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Reflection;
#endif

public class ConfigurationController : ScriptableObject
{
	public static string ConfigurationControllerPath = "Settings/ConfigurationController";

	private static ConfigurationController _instance = null;

	public static ConfigurationController Instance {
		get {
			if (_instance == null) {
				_instance = Resources.Load<ConfigurationController>(ConfigurationControllerPath);
				// 注意：因为该副本是存储关键参数的，所以不要自动生成副本并在脚本中指定参数，否则如被反编译将泄漏关键参数
			}
			return _instance;
		}
	}

	#if UNITY_EDITOR
	public Dictionary<string, object> _tempFields = null;

	public void Pack()
	{
		_tempFields = new Dictionary<string, object>();
		
		var fields = GetType().GetFields();
		foreach (var field in fields) {
			if (field.Name == "ConfigurationControllerPath" || field.Name == "_tempFields" || field.Name == "_instance")
				continue;
				
			_tempFields.Add(field.Name, field.GetValue(Instance));
		}
	}

	public void Unpack()
	{
		var fields = GetType().GetFields();
		foreach (var field in fields) {
			if (field.Name == "ConfigurationControllerPath" || field.Name == "_tempFields" || field.Name == "_instance")
				continue;

			if (_tempFields != null && _tempFields.ContainsKey(field.Name))
				field.SetValue(Instance, _tempFields[field.Name]);
		}
	}
	#endif

	[Space(10)]
	[Header("Http配置")]
	public string[] HttpHostList;
	public string HttpProductionHost;

    [Space(10)]
    public string HttpSecurityType;
    public string HttpSecuritySecret;
    public string HttpSessionField;
    public string HttpToken;
	public float HttpTimeOutDuration = 15f;

	[Space(10)]
	public string ServerProjectName = "server_framework";

	[Space(10)]
	[Header("默认语言")]
	public Language _DefaultLanguage = Language.Chinese;

	[Space(10)]
	[Header("在加载多个表格时是否清除无用的表格")]
	public bool _DestroyUnusedConfigs = true;

	[Space(10)]
	[Header("AES秘钥")]
	public string _AesKey;

	[Space(10)]
	[Header("分享相关App ID")]
	public string _QQAppId_iOS;
	public string _QQAppId_Android;
	public string _WechatAppId;

	[Space(10)]
	[Header("存档索引Key")]
	public string _SaveDataIndexFileKey;

	[Space(10)]
	[Header("标准Canvas分辨率")]
	public Vector2 _CanvasResolution;

	[Space(10)]
	[Header("Android签名的别名")]
	public string _AndroidKeyAlias = "snaplingo";

	[Space(10)]
	[Header("首次跳转场景")]
	public string _FirstScene = "LevelMap";
	public string _FirstScenePage = "LevelMap";

	[Space(10)]
	[Header("烘焙时光强")]
	public float _BakeIntensity = 1.99f;
	[Header("烘焙时阴影距离")]
	public float _ShadowDistance = 300f;

	[Space(10)]
	[Header("是否打包资源为Bundle")]
	public bool _BuildAssetBundle_iOS = true;
	public bool _BuildAssetBundle_Android = true;

	public bool BuildAssetBundle {
		get {
			#if UNITY_IPHONE
			return _BuildAssetBundle_iOS;
			#elif UNITY_ANDROID
			return _BuildAssetBundle_Android;
			#else
			return _BuildAssetBundle_iOS;
			#endif
		}
	}

	[Space(10)]
	[Header("是否打包场景为Bundle")]
	public bool _BuildSceneAsBundle_iOS = true;
	public bool _BuildSceneAsBundle_Android = true;

	public bool BuildSceneAsBundle {
		get {
			#if UNITY_IPHONE
			return _BuildSceneAsBundle_iOS;
			#elif UNITY_ANDROID
			return _BuildSceneAsBundle_Android;
			#else
			return _BuildSceneAsBundle_iOS;
			#endif
		}
	}

	string _StartScene = "Assets/Scenes/Framework/Core/StartScene.unity";
	string _LoadingScene = "Assets/Scenes/Framework/Core/LoadingScene.unity";

	public bool SceneIsBundle(string scenePath)
	{
		if (!BuildSceneAsBundle)
			return false;
		if (scenePath == _StartScene || scenePath == _LoadingScene)
			return false;
		if (SceneIsDebug(scenePath))
			return false;

		return true;
	}

	public bool SceneIsDebug(string scenePath)
	{
		return scenePath.Contains("/Debug/");
	}

}
