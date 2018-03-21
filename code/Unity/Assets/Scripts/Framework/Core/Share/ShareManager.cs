using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class ShareManager : Manager
{
	#region instance

	// 注意，请不要在Awake中使用Instance，否则会出现死循环
	public static ShareManager Instance { get { return GetManager<ShareManager>(); } }

	protected override void Init()
	{
		base.Init();

		RegisterAppWechat();
		RegisterAppQQ();
	}

	void OnApplicationFocus(bool focus)
	{
		if (focus) {
			StartCoroutine(CheckCallbackAfterFocus(1f));
		}
	}

	#endregion

	#region qq

	public static string QQAppId_iOS = ConfigurationController.Instance._QQAppId_iOS;
	//"1105786589"
	public static string QQAppId_Android = ConfigurationController.Instance._QQAppId_Android;
	//"1105959357"

	public enum QQResult
	{
		Error,
		Completed,
		Cancel,
	}

	Action<QQResult> _qqCallback;

	#if UNITY_IPHONE
	[DllImport("__Internal")]
	static extern void _RegisterAppQQ(string appId);

	[DllImport("__Internal")]
	static extern void _ShareImageQQ(IntPtr ptr, int size, IntPtr ptrThumb, int sizeThumb);

	[DllImport("__Internal")]
	static extern void _ShareImageQzone(IntPtr ptr, int size, string summary);

	[DllImport("__Internal")]
	static extern bool _IsQQInstalled();

	[DllImport("__Internal")]
	static extern bool _IsQQAppSupportApi();
	#endif

	public void RegisterAppQQ()
	{
		#if UNITY_IPHONE
		_RegisterAppQQ(QQAppId_iOS);
		#elif UNITY_ANDROID
		AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.QQUtils");
		utils.CallStatic ("RegisterToQQ", currentActivity, QQAppId_Android);
		#endif
	}

	public void ShareImageQQ(byte[] data, byte[] dataThumb, Action<QQResult> callback = null)
	{
		_qqCallback = callback;
		#if UNITY_IPHONE
		IntPtr array = Marshal.AllocHGlobal(data.Length);
		Marshal.Copy(data, 0, array, data.Length);
		IntPtr arrayThumb = Marshal.AllocHGlobal(dataThumb.Length);
		Marshal.Copy(dataThumb, 0, arrayThumb, dataThumb.Length);
		_ShareImageQQ(array, data.Length, arrayThumb, dataThumb.Length);
		#elif UNITY_ANDROID
		AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.QQUtils");
		utils.CallStatic ("ShareImageQQ", currentActivity, data);
		#endif
	}

	public void ShareImageQzone(byte[] data, string summary, Action<QQResult> callback = null)
	{
		_qqCallback = callback;
		#if UNITY_IPHONE
		IntPtr array = Marshal.AllocHGlobal(data.Length);
		Marshal.Copy(data, 0, array, data.Length);
		_ShareImageQzone(array, data.Length, summary);
		#elif UNITY_ANDROID
		AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.QQUtils");
		utils.CallStatic ("ShareImageQzone", currentActivity, data, summary);
		#endif
	}

	public bool IsQQInstalled()
	{
		#if UNITY_IPHONE
		return _IsQQInstalled();
		#elif UNITY_ANDROID
		return true;
		#else
		return false;
		#endif
	}

	public bool IsQQAppSupportApi() // 测试结构表明该返回值可能有问题，暂时废弃不用
	{
		#if UNITY_IPHONE
		return _IsQQAppSupportApi();
		#elif UNITY_ANDROID
		return true;
		#else
		return false;
		#endif
	}

	void QQCallback(string code)
	{
		LogManager.Log("QQCallback: " + ((QQResult)int.Parse(code)).ToString());
		if (_qqCallback != null) {
			_qqCallback((QQResult)int.Parse(code));
			_qqCallback = null;
		}
	}

	#endregion

	#region wechat

	public enum WXScene
	{
		/**< 聊天界面    */
		WXSceneSession = 0,
		/**< 朋友圈      */
		WXSceneTimeline = 1,
		/**< 收藏       */
		WXSceneFavorite = 2,
	}

	public enum WechatErrCode
	{
		Success = 0,
		ErrorCommon = -1,
		ErrorUserCancel = -2,
		ErrorSentFail = -3,
		ErrorAuthDenied = -4,
		ErrorUnsupport = -5,
		ErrorBan = -6,
	}
	
	#if UNITY_IPHONE
	[DllImport("__Internal")]
	static extern void _RegisterAppWechat(string appId);

	[DllImport("__Internal")]
	static extern void _ShareImageWechat(int scene, IntPtr ptr, int size, IntPtr ptrThumb, int sizeThumb);

	[DllImport("__Internal")]
	static extern bool _IsWechatInstalled();

	[DllImport("__Internal")]
	static extern bool _IsWechatAppSupportApi();
	#endif

	public static string WechatAppId = ConfigurationController.Instance._WechatAppId;
	//"wx189e87daaea3eea3"

	Action<WechatErrCode> _wechatCallback;

	IEnumerator CheckCallbackAfterFocus(float time)
	{
		yield return new WaitForSeconds(time);

		WechatCallBack(((int)WechatErrCode.ErrorUserCancel).ToString());
	}

	public void RegisterAppWechat()
	{
		#if UNITY_IPHONE
		_RegisterAppWechat(WechatAppId);
		#elif UNITY_ANDROID
		AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.WechatUtils");
		utils.CallStatic ("RegisterToWechat", currentActivity, WechatAppId);
		#endif
	}

	public void ShareImageWechat(WXScene scene, byte[] data, byte[] dataThumb, Action<WechatErrCode> callback = null)
	{
		_wechatCallback = callback;
		#if UNITY_IPHONE
		IntPtr array = Marshal.AllocHGlobal(data.Length);
		Marshal.Copy(data, 0, array, data.Length);
		IntPtr arrayThumb = Marshal.AllocHGlobal(dataThumb.Length);
		Marshal.Copy(dataThumb, 0, arrayThumb, dataThumb.Length);
		_ShareImageWechat((int)scene, array, data.Length, arrayThumb, dataThumb.Length);
		#elif UNITY_ANDROID
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.WechatUtils");
		utils.CallStatic ("ShareImageWechat", (int)scene, data, dataThumb);
		#endif
	}

	public bool IsWechatInstalled()
	{
		#if UNITY_IPHONE
		return _IsWechatInstalled();
		#elif UNITY_ANDROID
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.WechatUtils");
		return utils.CallStatic<bool> ("IsWechatInstalled");
		#else
		return false;
		#endif
	}

	public bool IsWechatAppSupportApi() // 测试结构表明该返回值可能有问题，暂时废弃不用
	{
		#if UNITY_IPHONE
		return _IsWechatAppSupportApi();
		#elif UNITY_ANDROID
		AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.WechatUtils");
		return utils.CallStatic<bool> ("IsWechatAppSupportAPI");
		#else
		return false;
		#endif
	}

	void WechatCallBack(string errCode)
	{
		WechatErrCode code = (WechatErrCode)int.Parse(errCode);
		if (_wechatCallback != null) {
			LogManager.Log("WechatCallBack: error code: " + code.ToString());
			_wechatCallback(code);
			_wechatCallback = null;
		}
	}

	#endregion
	
}
