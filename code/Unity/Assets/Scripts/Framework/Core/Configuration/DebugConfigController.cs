using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugConfigController : ScriptableObject
{
	public static string DebugConfigControllerPath = "Settings/DebugConfigController";

	private static DebugConfigController _instance = null;

	public static DebugConfigController Instance {
		get {
			if (_instance == null) {
				_instance = Resources.Load<DebugConfigController>(DebugConfigControllerPath);
				#if UNITY_EDITOR
				if (_instance == null) {
					EditorUtils.CreateAsset<DebugConfigController>(DebugConfigController.DebugConfigControllerPath);
					_instance = Resources.Load<DebugConfigController>(DebugConfigControllerPath);
				}
				#endif
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
			if (field.Name == "DebugConfigControllerPath" || field.Name == "_tempFields" || field.Name == "_instance")
				continue;

			_tempFields.Add(field.Name, field.GetValue(Instance));
		}
	}

	public void Unpack()
	{
		var fields = GetType().GetFields();
		foreach (var field in fields) {
			if (field.Name == "DebugConfigControllerPath" || field.Name == "_tempFields" || field.Name == "_instance")
				continue;

			if (_tempFields != null && _tempFields.ContainsKey(field.Name))
				field.SetValue(Instance, _tempFields[field.Name]);
		}
	}
	#endif

	[Space(10)]

	// Debug
	// 总开关
	public bool _Debug = true;

	[Space(10)]
	public int HttpHostIndex = 0;

	public bool ForceProductionServer()
	{
		for (int i = 0; i < ConfigurationController.Instance.HttpHostList.Length; ++i) {
			var server = ConfigurationController.Instance.HttpHostList[i];
			if (server == ConfigurationController.Instance.HttpProductionHost) {
				HttpHostIndex = i;
				return true;
			}
		}
		return false;
	}


	[Space(10)]
	public bool _TestBundleInEditor = false;

	public bool TestBundleInEditor {
		get { return _Debug && _TestBundleInEditor; }
	}

	[Space(10)]
	public bool _CheckBundleInStreamingAssets = false;

	public bool CheckBundleInStreamingAssets {
		get { return _Debug && _CheckBundleInStreamingAssets; }
	}

    [Space(10)]
    public bool _autoTest = false;
    public bool AutoTest
    {
        get { return _autoTest; }
    }
}
