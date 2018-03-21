using UnityEngine;
using System.IO;
using System;
using System.Collections;
using Snaplingo.SaveData;
using LitJson;
using System.Collections.Generic;
using System.Threading;
using System.Net;

public class UpkManager : Manager, ISaveData
{
	#region instance

	public static UpkManager Instance { get { return GetManager<UpkManager>(); } }

	protected override void Init()
	{
		base.Init();

		SaveDataUtils.LoadTo(this);

		_upkFolder = Application.persistentDataPath + "/AssetBundle/";
		_upkPath = _upkFolder + MiscUtils.GetCurrentPlatform() + ".upk";
		_bundleFolder = _upkFolder + MiscUtils.GetCurrentPlatform() + "/";
	}

	void Update()
	{
		UpdateAction();
		UpdateDownload();
	}

	void OnDestroy()
	{
		StopThread();
	}

	Thread _thread = null;

	void StopThread()
	{
		if (_thread != null) {
			_thread.Abort();
			_thread = null;
		}
	}

	void StartThread(Action action)
	{
		_thread = new Thread(new ThreadStart(action));
		_thread.Start();
	}

	#endregion

	#region logic

	string _upkFolder = "";
	string _upkPath = "";
	string _bundleFolder = "";

	string _latestUpkVersion = "";

	public string LatestUpkVersion {
		get { return _latestUpkVersion; }
		set {
			var old = _latestUpkVersion;
			_latestUpkVersion = value;
			if (old != _latestUpkVersion)
				SaveDataUtils.SaveFrom(this);
		}
	}

	void ChangeStatus(Status status)
	{
		var old = _currentStatus;
		_currentStatus = status;
		if (old != _currentStatus)
			SaveDataUtils.SaveFrom(this);
		_needAction = true;
	}

	bool _needAction = false;

	void UpdateAction()
	{
		if (IsInProgress && _needAction) {
			switch (_currentStatus) {
				case Status.Request:
					RequestUpk();
					break;
				case Status.Extract:
					ExtractUpk();
					break;
				case Status.Done:
					Succeed();
					break;
			}
			_needAction = false;
		}
		if (_failed) {
			ExecuteFail();
			_failed = false;
		}
	}

	bool _isInProgress = false;

	public bool IsInProgress {
		get { return _isInProgress; }
		private set { _isInProgress = value; }
	}

	IProgress _progress = null;
	Action _successCallback = null;

	void Succeed()
	{
		LatestUpkVersion = MiscUtils.GetVersion();

		_failureCallback = null;
		IsInProgress = false;

		if (_successCallback != null) {
			_successCallback();
			_successCallback = null;
		}
	}

	Action<Status> _failureCallback = null;
	bool _failed = false;

	void Fail()
	{
		_failed = true;
	}

	void ExecuteFail()
	{
		_successCallback = null;
		IsInProgress = false;
		StopThread();

		if (_failureCallback != null) {
			_failureCallback(_currentStatus);
			_failureCallback = null;
		}
	}

	public enum Status
	{
		Request,
		Extract,
		Done,
	}

	Status _currentStatus = Status.Request;

	string GetSizeMbString(long size)
	{
		return Math.Round(size / 1000000f, 1).ToString("F1") + "M";
	}

	void ResetProgress(Status status)
	{
		if (_progress != null) {
			_progress.ResetProgress();
			if (status == Status.Request) {
				string text = LocalizationConfig.Instance.GetStringById(4007);
				_progress.SetTextFunc2(delegate (long inSize, long outSize, string extra) {
					return string.Format(text, extra, GetSizeMbString(inSize), GetSizeMbString(outSize));
				});
			} else if (status == Status.Extract) {
				string text = LocalizationConfig.Instance.GetStringById(4009);
				_progress.SetTextFunc(delegate (long inSize, long outSize) {
					return string.Format(text, GetSizeMbString(inSize), GetSizeMbString(outSize));
				});
			}
		}
	}

	public void CheckUpk(IProgress progress, Action successCallback, Action<Status> failureCallback)
	{
		if (IsInProgress)
			return;
		
		IsInProgress = true;
		_progress = progress;
		_successCallback = successCallback;
		_failureCallback = failureCallback;

		if (MiscUtils.GetVersion() != LatestUpkVersion) {
			ChangeStatus(Status.Request);
		}

		_needAction = true;
	}

	void RequestUpk()
	{
		string tempurl = "Assetbundle/CheckUpk";
		JsonData tempData = new JsonData();
		tempData["platform"] = MiscUtils.GetCurrentPlatform();
		tempData["last_version"] = LatestUpkVersion;
		byte[] Data = System.Text.Encoding.UTF8.GetBytes(tempData.ToJson());

        StaticMonoBehaviour.Instance.StartCoroutine(HttpRequest.Instance.WebRequest(HttpRequest.HttpReqType.POST, tempurl, Data, (string json)=> {
			JsonData Json = JsonMapper.ToObject(json);
			JsonData data = Json["data"];
			if (data.Keys.Contains("url"))
			{
				var size = (long)(int)data["size"];
				if (UpkSize > 0 && UpkSize != size)
				{
					if (File.Exists(_upkPath))
						File.Delete(_upkPath);
				}
				UpkSize = size;
				var url = (string)data["url"];
				LogManager.Log("Upk url: " + url);
				DownloadUpk(url);
			}
			else
			{
				ChangeStatus(Status.Done);
			}
		}, Fail));
	}

	string _xCache = "";

	void DownloadUpk(string url)
	{
		ResetProgress(Status.Request);
		StartThread(delegate {
			if (!Directory.Exists(Path.GetDirectoryName(_upkPath))) {
				Directory.CreateDirectory(Path.GetDirectoryName(_upkPath));
			}
			FileStream fs = new FileStream(_upkPath, FileMode.OpenOrCreate, FileAccess.Write);
			try {
				long fileLength = fs.Length;

				if (fileLength < UpkSize) {
					//断点续传核心，设置本地文件流的起始位置
					fs.Seek(fileLength, SeekOrigin.Begin);
					HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
					//断点续传核心，设置远程访问文件流的起始位置
					request.AddRange((int)fileLength);
					HttpWebResponse response = request.GetResponse() as HttpWebResponse;
					_xCache = response.GetResponseHeader("X-CACHE");
					Stream stream = response.GetResponseStream();
					try {
						byte[] buffer = new byte[1024];
						//使用流读取内容到buffer中
						//注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
						int length = stream.Read(buffer, 0, buffer.Length);
						while (length > 0) {
							//将内容再写入本地文件中
							fs.Write(buffer, 0, length);
							//计算进度
							fileLength += length;
							_upkDownloadedSize = fileLength;
							//类似尾递归
							length = stream.Read(buffer, 0, buffer.Length);
						}
					} finally {
						stream.Close();
						stream.Dispose();
					}
				} else {
					_upkDownloadedSize = fileLength;
				}
			} finally {
				fs.Close();
				fs.Dispose();
			}
		});
	}

	long _lastDownloaded = 0;
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

	float _progressUpdateTimer = 0;
	long _upkDownloadedSize = 0;
	long _upkSize = 0;

	long UpkSize {
		get { return _upkSize; }
		set {
			var old = _upkSize;
			_upkSize = value;
			if (old != _upkSize)
				SaveDataUtils.SaveFrom(this);
		}
	}

	float _noSpeedTime = 0;

	void UpdateDownload()
	{
		if (_currentStatus == Status.Request) {
			var bytesOfFrame = _upkDownloadedSize - _lastDownloaded;
			var speed = (int)(bytesOfFrame / (1000 * Time.deltaTime));
			AddSpeed(speed);
			if (speed > 0) {
				_noSpeedTime = 0;
			} else {
				_noSpeedTime += Time.deltaTime;
				if (_noSpeedTime >= 5f) {
					_noSpeedTime = 0;
					Fail();
					return;
				}
			}

			_progressUpdateTimer += Time.deltaTime;
			if (_progressUpdateTimer > 1f) {
				_progressUpdateTimer -= 1f;
				if (_progress != null) {
					_progress.SetProgress(_upkDownloadedSize, UpkSize, GetAverageSpeed().ToString());
				}
			}
			_lastDownloaded = _upkDownloadedSize;

			if (UpkSize > 0 && _upkDownloadedSize >= UpkSize) {
				UpkSize = 0;
				ChangeStatus(Status.Extract);
			}

			if (!string.IsNullOrEmpty(_xCache)) {
				LogManager.Log("X-Cache: " + _xCache);
				_xCache = "";
			}
		}
	}

	void ExtractUpk()
	{
		ResetProgress(Status.Extract);
		StartThread(delegate {
			if (File.Exists(_upkPath)) {
				FingerPrint.StartWriting(_bundleFolder);
				if (UPKExtra.ExtraUPK(_upkPath, _bundleFolder, _progress, true, delegate (string path, byte[] bytes) {
					FingerPrint.AddItem(path, MiscUtils.GetMd5HashFromBytes(bytes), _bundleFolder);
				})) {
					FingerPrint.Flush();
					ChangeStatus(Status.Done);
					File.Delete(_upkPath);
				} else {
					Fail();
				}
			} else {
				ChangeStatus(Status.Request);
			}
		});
	}

	#endregion

	#region save data

	public string SaveAsJson()
	{
		JsonData data = new JsonData();
		data["latest_version"] = _latestUpkVersion;
		data["status"] = (int)_currentStatus;
		data["upk_size"] = UpkSize;
		return data.ToJson();
	}

	public void LoadFromJson(string json)
	{
		JsonData data = JsonMapper.ToObject(json);
		_latestUpkVersion = (string)data["latest_version"];
		_currentStatus = (Status)(int)data["status"];
		_upkSize = long.Parse(data.TryGetString("upk_size", "0"));
	}

	public string SaveTag()
	{
		return "upk";
	}

	#endregion

}
