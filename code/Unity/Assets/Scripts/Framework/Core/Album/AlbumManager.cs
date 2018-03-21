using System.Collections;
using UnityEngine;
using System;
using System.IO;

#if UNITY_IPHONE
using System.Runtime.InteropServices;
#endif

public class AlbumManager : Manager
{
	public static AlbumManager Instance = GetManager<AlbumManager>();

	Action<bool> _callback = null;

#if UNITY_IPHONE
	[DllImport("__Internal")]
	static extern void _SaveImageToAlbum(IntPtr ptr, int size);
#endif

    public void SaveImageToAlbum(Texture2D texture, Action<bool> callback = null)
	{
		SaveImageToAlbum(texture.EncodeToJPG(), callback);
	}

	public void SaveImageToAlbum(byte[] data, Action<bool> callback = null)
	{
		_callback = callback;
		#if UNITY_IPHONE
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			IntPtr array = Marshal.AllocHGlobal(data.Length);
			Marshal.Copy(data, 0, array, data.Length);
			_SaveImageToAlbum(array, data.Length);
		} else {
			SaveImageToAlbumCallback("false");
		}
		#elif UNITY_ANDROID
		if (Application.platform == RuntimePlatform.Android) {
			try {
				string destination = "/sdcard/DCIM/Wanchuanglab";
				//判断目录是否存在，不存在则会创建目录
				if (!Directory.Exists (destination)) {
					Directory.CreateDirectory (destination);
				}
				System.DateTime now = System.DateTime.Now;
				string times = now.ToString ();
				times = times.Trim ();
				times = times.Replace ("/", "-").Replace (" ", "-").Replace (":", "-");
				string filename = "Image" + times + ".jpg";
				var filePath = destination + "/" + filename;
				//存图片
				File.WriteAllBytes (filePath, data);

				AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
				AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
				AndroidJavaClass utils = new AndroidJavaClass ("com.youngoutliers.rc.Utils");
				utils.CallStatic ("refreshMediaFile", currentActivity, filePath);
				SaveImageToAlbumCallback ("true");
			} catch (Exception e) {
				Debug.LogError (e.Message);
				SaveImageToAlbumCallback ("false");
			}
		} else {
			SaveImageToAlbumCallback ("false");
		}
		#endif
	}

	void SaveImageToAlbumCallback(string result)
	{
		bool ok = bool.Parse(result);
		if (_callback != null) {
			_callback(ok);
			_callback = null;
		}
	}

}
