using LitJson;
using System.IO;
using Snaplingo.SaveData;
using Snaplingo.Config;

public class AssetBundleSaveData : ISaveData
{
	public static AssetBundleSaveData Instance {
		get { return SaveDataUtils.GetSaveData<AssetBundleSaveData>(); }
	}

	public enum BigZipDownloadStatus
	{
		NotDownloaded,
		Downloading,
		Downloaded

	}

	public string CurrentDownloadVersion { get; set; }

	public int DownloadStatus { get; set; }

	public void RefreshDownloadStatus(int status, bool refreshVersion = false)
	{
		DownloadStatus = status;
		if (refreshVersion) {
			CurrentDownloadVersion = MiscUtils.GetVersion();
		}
		SaveDataUtils.Save<AssetBundleSaveData>();
	}

	public bool IsReady(string version)
	{
		return CurrentDownloadVersion == version && DownloadStatus == (int)BigZipDownloadStatus.Downloaded;
	}

	public string SaveTag()
	{
		return "AssetBundleSaveData";
	}

	public string SaveAsJson()
	{
		JsonData data = new JsonData();
		data["currentVersion"] = string.IsNullOrEmpty(CurrentDownloadVersion) ? "0" : CurrentDownloadVersion;
		data["downloadStatus"] = DownloadStatus;
		return data.ToJson();
	}

	public void LoadFromJson(string json)
	{
		JsonData data = JsonMapper.ToObject(json);
		CurrentDownloadVersion = string.IsNullOrEmpty(data.TryGetString("currentVersion")) ? "" : data.TryGetString("currentVersion");
		DownloadStatus = string.IsNullOrEmpty(data.TryGetString("downloadStatus")) ? 0 : int.Parse(data.TryGetString("downloadStatus"));
	}
}
