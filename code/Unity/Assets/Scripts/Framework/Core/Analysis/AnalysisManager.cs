using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Umeng;

public class AnalysisManager : Manager
{
	// 注意，请不要在Awake中使用Instance，否则会出现死循环
	public static AnalysisManager Instance { get { return GetManager<AnalysisManager>(); } }

	[RuntimeInitializeOnLoadMethod]
	static void InitializeOnLoad()
	{
		Instance.SetAccount();
	}

	protected override void Init()
	{
		base.Init();

        // Umeng
#if UNITY_ANDROID
		GA.StartWithAppKeyAndChannelId ("58dc7f65f29d982d4c0010da", MiscUtils.GetChannel ());
#elif UNITY_IPHONE
		GA.StartWithAppKeyAndChannelId("59fa7810734be4235d000030", MiscUtils.GetChannel());
#endif
        GA.SetLogEnabled(DebugConfigController.Instance._Debug);
	}

	static bool FilterPlatform()
	{
		return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
	}

	public void SetAccount()
	{
		if (FilterPlatform())
			GA.ProfileSignIn(GA.GetDeviceInfo());
	}

	public void SetLevel(int level)
	{
		if (FilterPlatform())
			GA.SetUserLevel(level);
	}

	public void OnEvent(string actionId, string actionName, string key1, string val1, string key2, string val2)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>();
		parameters.Add(key1, val1);
		parameters.Add(key2, val2);
		OnEvent(actionId, actionName, parameters);
	}

	public void OnEvent(string actionId, string actionName, Dictionary<string, object> parameters)
	{
		if (FilterPlatform()) {
			if (parameters.Count > 0) {
				Dictionary<string, string> newParams = new Dictionary<string, string>();
				foreach (var p in parameters) {
					newParams.Add(p.Key, p.Value.ToString());
				}
				GA.Event(actionId, newParams);
			} else
				GA.Event(actionId);
		}
	}

	public void OnEvent(string actionId, string actionName)
	{
		OnEvent(actionId, actionName, new Dictionary<string, object>());
	}

	public void OnEvent(string actionId, string actionName, string key, string val)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>();
		parameters.Add(key, val);
		OnEvent(actionId, actionName, parameters);
	}

	public void OnEvent(string actionId, string actionName, string key, double val)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>();
		parameters.Add(key, val);
		OnEvent(actionId, actionName, parameters);
	}

	public void OnLevelBegin(string levelName)
	{
		if (FilterPlatform()) {
			LogManager.Log("OnLevelBegin: " + levelName);
			GA.StartLevel(levelName);
		}
	}

	public void OnLevelCompleted(string levelName)
	{
		if (FilterPlatform()) {
			LogManager.Log("OnLevelCompleted: " + levelName);
			GA.FinishLevel(levelName);
		}
	}

	public void OnLevelFailed(string levelName, string reason)
	{
		if (FilterPlatform()) {
			LogManager.Log("OnLevelFailed: " + levelName);
			GA.FailLevel(levelName);
		}
	}

	public void GAPay(double amount, GA.PaySource source, string item)
	{
		if (FilterPlatform()) {
			GA.Pay(amount, source, item, 1, amount);
		}
	}

}