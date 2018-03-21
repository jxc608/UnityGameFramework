using UnityEngine;
using System.IO;
using System;
using System.Collections;
using Snaplingo.SaveData;
using LitJson;
using System.Collections.Generic;

public class UpdateBundleManager : Manager
{
	// 注意，请不要在Awake中使用Instance，否则会出现死循环
	public static UpdateBundleManager Instance { get { return GetManager<UpdateBundleManager>(); } }

	private string m_AssetBundlePath = "";

	protected override void Init()
	{
		base.Init();

		m_AssetBundlePath = Application.persistentDataPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";

		m_Status = Status.Idle;
	}

	void OnDestroy()
	{
		FingerPrint.Flush();
	}

	IProgress m_Progress;
	Action m_CompleteCallback;

	void CheckCompleteCallback()
	{
		if (m_CompleteCallback != null)
			m_CompleteCallback();
	}

	public void StartCheckLocalFiles(IProgress progress, Action callback)
	{
		if (!MiscUtils.IsOnline()) {
			NetworkFail();
		} else {
			m_Progress = progress;
			if (m_Progress != null) {
				m_Progress.ResetProgress();
				m_Progress.SetTextFunc2(delegate(long inSize, long outSize, string extra) {
					return LocalizationConfig.Instance.GetStringById(4004, extra, inSize, outSize);
				});
			}
			m_CompleteCallback = callback;
			CheckFingerPrint();
		}
	}

	private enum Status
	{
		Idle,
		CopyingFiles,
		DownloadingSmallBundles,
		Ready,
	}

	private Status m_Status;

	private WWW m_CurrentWWW;

	List<int> _speedHistory = new List<int>();
	int MAX_SPEED_HISTORY_COUNT = 50;

	void AddSpeed(int speed)
	{
		_speedHistory.Add(speed);
		if (_speedHistory.Count > MAX_SPEED_HISTORY_COUNT)
			_speedHistory.RemoveAt(0);
	}

	int GetAverageSpeed()
	{
		int sum = 0;
		for (int i = 0; i < _speedHistory.Count; ++i) {
			sum += _speedHistory[i];
		}

		if (_speedHistory.Count == 0) {
			return 0;
		} else {
			return sum / _speedHistory.Count;
		}
	}

	int m_TotalFileNum = 0;
	int m_DownloadedNum = 0;
	int m_FinishedBytes = 0;
	int m_LastFrameDownloadBytes = 0;
	int m_ThisFrameDownloadBytes = 0;
	private float m_Timer;

	private void Update()
	{
		if (m_Status == Status.DownloadingSmallBundles && m_CurrentWWW != null) {
			m_LastFrameDownloadBytes = m_ThisFrameDownloadBytes;
			m_ThisFrameDownloadBytes = m_CurrentWWW.bytesDownloaded + m_FinishedBytes;
			int speed = (int)((m_ThisFrameDownloadBytes - m_LastFrameDownloadBytes) / (1000 * Time.deltaTime));
			AddSpeed(speed);

			m_Timer += Time.deltaTime;
			if (m_Timer > 1f) {
				m_Timer -= 1f;
				if (m_Progress != null)
					m_Progress.SetProgress(m_DownloadedNum, m_TotalFileNum, GetAverageSpeed().ToString());
			}
		}
	}

	public void SetCopyFilesComplete()
	{
		m_Status = Status.Idle;
	}

	private void CheckFingerPrint()
	{
		var fingerPrintPath = m_AssetBundlePath + "/FingerPrint.txt";
		if (File.Exists(fingerPrintPath)) {
			LogManager.Log("fingerPrint.txt exist!!");
			CheckFingerPrintWithServer(File.ReadAllText(fingerPrintPath));
		} else {
			LogManager.Log("did not find the file: fingerPrint.txt!!");
			CheckFingerPrintWithServer("");
		}
	}

	private void CheckFingerPrintWithServer(string json)
	{
		string url = "Assetbundle/CheckBundle";
		JsonData tempData = new JsonData();
		#if UNITY_ANDROID
		tempData["local_files"] = json;
		tempData["no_compression"] = "1";
		#else
		tempData["local_files"] = Convert.ToBase64String(GZipUtil.Zip(json));
		#endif

		byte[] Data = System.Text.Encoding.UTF8.GetBytes(tempData.ToJson());
        StaticMonoBehaviour.Instance.StartCoroutine(HttpRequest.Instance.WebRequest(HttpRequest.HttpReqType.POST, url, Data, (string Json)=>{
			JsonData tempJson = JsonMapper.ToObject(Json);
			JsonData data = tempJson["data"];
			#if UNITY_ANDROID
				string filesUpdate = data ["file_to_update"].ToString ();
				string filesDelete = data ["file_to_delete"].ToString ();
			#else
			string filesUpdate = GZipUtil.UnZipToString(Convert.FromBase64String(data["file_to_update"].ToString()));
			string filesDelete = GZipUtil.UnZipToString(Convert.FromBase64String(data["file_to_delete"].ToString()));
			LogManager.Log("file_to_update: " + filesUpdate);
			LogManager.Log("file_to_delete: " + filesDelete);
			#endif
			FingerPrint.StartWriting(m_AssetBundlePath);
			if (!string.IsNullOrEmpty(filesDelete))
			{
				JsonData jdFilesDelete = JsonMapper.ToObject(filesDelete);
				for (int i = 0; i < jdFilesDelete.Count; ++i)
				{
					var filePath = m_AssetBundlePath + jdFilesDelete[i].ToString();
					if (File.Exists(filePath))
						File.Delete(filePath);
					FingerPrint.DeleteItem(filePath, m_AssetBundlePath);
				}
			}
			FingerPrint.Flush();
			//有需要更新的文件
			if (!string.IsNullOrEmpty(filesUpdate))
			{
				JsonData jdFilesUpdate = JsonMapper.ToObject(filesUpdate);
				string staticServer = data["static_server"].ToString();
				string platformPath = data["platform_path"].ToString();
				List<string> paths = GetPathFromJson(jdFilesUpdate, "");
				if (paths.Count > 0)
				{
					DownloadFiles(paths, staticServer, platformPath);
				}
				else
				{
					Ready();
				}
			}
		}, NetworkFail));
	}

	void NetworkFail()
	{
		if (AssetBundleSaveData.Instance.IsReady(MiscUtils.GetVersion())) {
			Ready();
		} else {
			CheckFingerPrint();
		}
	}

#region download small bundles

	public struct BundleInfo
	{
		public string _url { get; set; }

		public string _path { get; set; }

		public override bool Equals(object bi)
		{
			return bi is BundleInfo && _url == ((BundleInfo)bi)._url;
		}

		public override int GetHashCode()
		{
			return _url.GetHashCode();
		}
	}

	List<BundleInfo> m_FailedBundles = new List<BundleInfo>();
	List<BundleInfo> m_WaitForDownloadBundles = new List<BundleInfo>();
	private int m_DownloadIndex = 0;

	private void DownloadFiles(List<string> paths, string staticServer, string platformPath)
	{
		m_FailedBundles.Clear();
		m_WaitForDownloadBundles.Clear();
		FingerPrint.StartWriting(m_AssetBundlePath);

		for (int i = 0; i < paths.Count; ++i) {
			var url = staticServer + "/" + platformPath + "/" + paths[i];
			var bundleInfo = new BundleInfo() { _url = url, _path = paths[i] };
			m_WaitForDownloadBundles.Add(bundleInfo);
		}

		if (m_WaitForDownloadBundles.Count == 0) {
			Ready();
		} else {
			m_DownloadIndex = 0;
			m_Status = Status.DownloadingSmallBundles;
			LogManager.Log("Start download bundle!");
			if (!Directory.Exists(m_AssetBundlePath)) {
				Directory.CreateDirectory(m_AssetBundlePath);
			}
			string[] files = Directory.GetFiles(m_AssetBundlePath, "*.*", SearchOption.AllDirectories);
			m_DownloadedNum = files.Length;
			m_TotalFileNum = m_WaitForDownloadBundles.Count + files.Length;

			AssetBundleSaveData.Instance.RefreshDownloadStatus((int)AssetBundleSaveData.BigZipDownloadStatus.Downloading);
			StartDownloadAtIndex();
		}
	}

	public IEnumerator DownloadBundleFile(BundleInfo info)
	{
		WWW www = new WWW(info._url);
		m_CurrentWWW = www;
		yield return www;

		if (string.IsNullOrEmpty(www.error)) {
			if (www.responseHeaders.ContainsKey("X-CACHE"))
				LogManager.Log(info._path + ": " + www.responseHeaders["X-CACHE"]);
			try {
				string filePath = m_AssetBundlePath + info._path;
				if (!Directory.Exists(Path.GetDirectoryName(filePath)))
					Directory.CreateDirectory(Path.GetDirectoryName(filePath));
				File.WriteAllBytes(filePath, www.bytes);
				FingerPrint.AddItem(filePath, MiscUtils.GetMd5HashFromBytes(www.bytes), m_AssetBundlePath);
				m_DownloadedNum++;
				m_FinishedBytes += www.bytes.Length;

				www.Dispose();
			} catch (Exception e) {
				LogManager.Log("下载失败：url: " + info._url + ", error: " + e.Message);
				m_FailedBundles.Add(info);
			}
		} else {
			LogManager.Log("下载失败：url: " + info._url + ", error: " + www.error);
			m_FailedBundles.Add(info);
		}

		m_DownloadIndex++;
		if (m_DownloadIndex == m_WaitForDownloadBundles.Count) {
			if (m_FailedBundles.Count > 0) {
				LogManager.Log(m_FailedBundles.Count + " files failed! ");
				m_WaitForDownloadBundles.Clear();
				for (int i = 0; i < m_FailedBundles.Count; ++i) {
					m_WaitForDownloadBundles.Add(new BundleInfo() {
						_url = m_FailedBundles[i]._url,
						_path = m_FailedBundles[i]._path
					});
				}
				m_FailedBundles.Clear();
				m_DownloadIndex = 0;
				StartDownloadAtIndex();
			} else {
				LogManager.Log("all files completed!!");
				Ready();
				FingerPrint.Flush();
			}
		} else {
			StartDownloadAtIndex();
		}
		Resources.UnloadUnusedAssets();
	}

	private void StartDownloadAtIndex()
	{
		var bundle = m_WaitForDownloadBundles[m_DownloadIndex];
		StartCoroutine(DownloadBundleFile(bundle));
	}

	List<string> GetPathFromJson(JsonData data, string currentPath)
	{
		List<string> paths = new List<string>();
		foreach (string key in data.Keys) {
			if (key.Equals("f")) {
				for (int i = 0; i < data["f"].Count; ++i)
					paths.Add("" + data["f"][i]);
			} else {
				currentPath = key;
				List<string> subPaths = GetPathFromJson(data[key], currentPath);
				for (int i = 0; i < subPaths.Count; ++i) {
					paths.Add(currentPath + "/" + subPaths[i]);
				}
			}
		}
		return paths;
	}

#endregion

	private void Ready()
	{
		AssetBundleSaveData.Instance.RefreshDownloadStatus((int)AssetBundleSaveData.BigZipDownloadStatus.Downloaded, true);
		m_Status = Status.Ready;
		m_CurrentWWW = null;
		AssetBundleManager.Instance.TryGetManifest();
		CheckCompleteCallback(); 
	}
}
