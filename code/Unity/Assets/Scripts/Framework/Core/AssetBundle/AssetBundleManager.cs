using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;

[ResourceLoadAttribute(0)]
public class AssetBundleManager : Manager, IResourceLoad
{
	#region Instance

	public static AssetBundleManager Instance { get { return GetManager<AssetBundleManager>(); } }

	IProgress m_Progress;
	private Action m_FailCallback;
	private Action m_ReadyCallback;

	void Exit()
	{
		m_FailCallback = null;
		m_ReadyCallback = null;
	}

	public void CheckIfTesting(IProgress progress, Action readyCallback, Action failCallback)
	{
		if (!DebugConfigController.Instance.CheckBundleInStreamingAssets) {
			if (failCallback != null)
				failCallback();
			return;
		}

		if (m_Status == Status.Initialized) {
			if (readyCallback != null)
				readyCallback();
			return;
		}

		m_Progress = progress;
		if (m_Progress != null) {
			m_Progress.ResetProgress();
			m_Progress.SetTextFunc(delegate(long inSize, long outSize) {
				return LocalizationConfig.Instance.GetStringById(4004, "", inSize, outSize);
			});
		}

#if UNITY_ANDROID && !UNITY_EDITOR
		string path = m_BundleNameTxtPath;
#else
		string path = "file://" + m_BundleNameTxtPath;
#endif
		m_ReadyCallback = readyCallback;
		m_FailCallback = failCallback;
		StartCoroutine(CheckStreamingAsset(path));
	}

	private List<string> m_LocalFilesNeedCopy = new List<string>();
	private int m_LocalFileLoadingIndex = 0;

	private IEnumerator CheckStreamingAsset(string path)
	{
		WWW www = new WWW(path);
		yield return www;

		if (string.IsNullOrEmpty(www.error)) {//streamingAsset 文件夹里面有资源，即判断为测试环境
			string[] files = www.text.Split('|');
			m_LocalFilesNeedCopy.Clear();
			foreach (string filePath in files) {
				if (!string.IsNullOrEmpty(filePath)) {
					m_LocalFilesNeedCopy.Add(filePath);
				}
			}
			if (m_LocalFilesNeedCopy.Count > 0) {
				m_LocalFileLoadingIndex = 0;
				StartCoroutine(LoadStreamingAssetFiles());
			}
		} else {
			Debug.LogWarning("CheckStreamingAsset error: " + www.error);
			if (m_FailCallback != null) {
				m_FailCallback();
				Exit();
			}
		}
	}

	private IEnumerator LoadStreamingAssetFiles()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
        string path = m_StreamingAssetPath + m_LocalFilesNeedCopy[m_LocalFileLoadingIndex];
#else
		string path = "file://" + m_StreamingAssetPath + m_LocalFilesNeedCopy[m_LocalFileLoadingIndex];
#endif
		WWW www = new WWW(path);
		yield return www;

		if (!string.IsNullOrEmpty(www.error)) {
			LogManager.LogError("Copy local files error:" + www.error);
		} else {
			CreateBundleFile(m_PersistentDataPath + m_LocalFilesNeedCopy[m_LocalFileLoadingIndex], www.bytes);
			www.Dispose();
			m_LocalFileLoadingIndex++;
			if (m_Progress != null)
				m_Progress.SetProgress(m_LocalFileLoadingIndex, m_LocalFilesNeedCopy.Count);
			if (m_LocalFileLoadingIndex == m_LocalFilesNeedCopy.Count) {
				UpdateBundleManager.Instance.SetCopyFilesComplete();
				TryGetManifest();
			} else {
				StartCoroutine(LoadStreamingAssetFiles());
			}
		}
	}

	private void CreateBundleFile(string path, byte[] bytes)
	{
		if (!Directory.Exists(Path.GetDirectoryName(path))) {
			Directory.CreateDirectory(Path.GetDirectoryName(path));
		}
		File.WriteAllBytes(path, bytes);
	}

	public void TryGetManifest()
	{
		if (m_Status == Status.Initialized)
			return;

		var filePath = m_PersistentDataPath + MiscUtils.GetCurrentPlatform();
		if (File.Exists(filePath)) {
			LoadManifest(filePath);
		} else {
			LogManager.Log("NO Main manifest in persistent path!!");
		}
	}

	private void LoadManifest(string filePath)
	{
		AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
		m_MainManifest = bundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
		m_Status = Status.Initialized;
		if (m_ReadyCallback != null) {
			m_ReadyCallback();
			Exit();
		}
	}

	#endregion

	private AssetBundleManifest m_MainManifest = null;

	public AssetBundleManifest Manifest {
		get { return m_MainManifest; }
	}

	private enum Status
	{
		Idle,
		Initialized
	}

	private Status m_Status = Status.Idle;

	string m_PersistentDataPath = "";
	string m_StreamingAssetPath = "";
	string m_BundleNameTxtPath = "";

	private void Awake()
	{
		m_PersistentDataPath = Application.persistentDataPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
		m_StreamingAssetPath = Application.streamingAssetsPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
		m_BundleNameTxtPath = Application.streamingAssetsPath + "/AssetBundle/BundleNameList.txt";
	}

	#region 用户接口

	public string[] GetDependencies(string bundleName)
	{
		if (m_MainManifest != null) {
			string[] paths = m_MainManifest.GetAllDependencies(bundleName);
			List<string> newPaths = new List<string>();
			foreach (string s in paths) {
				string fileName = s.Replace(m_StreamingAssetPath, m_PersistentDataPath);
				newPaths.Add(fileName);
			}
			return newPaths.ToArray();
		} else {
			LogManager.LogError("not init yet!");
			return null;
		}
	}

	public void ClearBundleMap()
	{
		foreach (KeyValuePair<string, AssetBundle> kv in m_BundleMap) {
			kv.Value.Unload(false);
		}
		m_BundleMap.Clear();
	}

	public void ClearAudioMap()
	{
		foreach (KeyValuePair<string, AssetBundle> kv in m_AudioMap) {
			kv.Value.Unload(false);
		}
		m_AudioMap.Clear();
	}

	public string GetResourceFolder()
	{
		return m_PersistentDataPath.Replace("\\", "/") + "assetbundle/";
	}

	private Dictionary<string, AssetBundle> m_BundleMap = new Dictionary<string, AssetBundle>();
	private Dictionary<string, AssetBundle> m_AudioMap = new Dictionary<string, AssetBundle>();

	public T Load<T>(string path) where T : UnityEngine.Object
	{
		#if UNITY_EDITOR
		if (!DebugConfigController.Instance.TestBundleInEditor)
			return null;
		#endif

		#if !UNITY_EDITOR
		if (!ConfigurationController.Instance.BuildAssetBundle)
			return Resources.Load<T> (Path.Combine ("AssetBundle", path));
		#endif

		var bundle = GetBundle<T>(path);
		if (bundle != null)
			return bundle.LoadAsset<T>(Path.GetFileName(path));
		return null;
	}

	AssetBundle GetBundle<T>(string relativePath, bool ignoreDependencies = false) where T : UnityEngine.Object
	{
		if (m_Status != Status.Initialized)
			return null;
		
		relativePath = relativePath.ToLower();
		string assetPath = GetResourceFolder() + relativePath;
		var bundleMap = GetBundleMap<T>();
		if (!bundleMap.ContainsKey(relativePath)) {
			if (File.Exists(assetPath)) {
				AssetBundle mainBundle = AssetBundle.LoadFromFile(assetPath);
				bundleMap.Add(relativePath, mainBundle);
				if (!ignoreDependencies) {
					string[] paths = GetDependencies("assetbundle/" + relativePath);
					List<AssetBundle> dependencies = new List<AssetBundle>();
					foreach (string path in paths) {
						if (!bundleMap.ContainsKey(path)) {
							AssetBundle bundle = AssetBundle.LoadFromFile(m_PersistentDataPath + path);
							dependencies.Add(bundle);
							bundleMap.Add(path, bundle);
						}
					}
				}
				return mainBundle;
			} else {
				return null;
			}
		} else {
			return bundleMap[relativePath];
		}
	}

	Dictionary<string, AssetBundle> GetBundleMap<T>() where T : UnityEngine.Object
	{
		if (typeof(T) == typeof(AudioClip))
			return m_AudioMap;
		else
			return m_BundleMap;
	}

	#endregion
}
