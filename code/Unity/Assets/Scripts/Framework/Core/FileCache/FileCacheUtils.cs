using UnityEngine;
using System.Collections;
using System.IO;

public class FileCacheUtils
{
	static string Folder {
		get { return Application.persistentDataPath + "/files"; }
	}

	static string KeyPrefix {
		get { return "FileCacheVersion_"; }
	}

	static string GetPath(string key)
	{
		return Folder + "/" + key;
	}


	// 	===============
	//	存储缓存文件
	//	rawData: 文件内容
	//	timeStamp: 文件附属信息-时间戳
	//	===============
	public static void Store(string key, byte[] rawData, int version)
	{
		StoreFile(key, rawData);
		StoreFileVersion(key, version);
	}

	public static void Store(string key, byte[] rawData)
	{
		StoreFile(key, rawData);
	}

	static void StoreFile(string key, byte[] rawData)
	{
		if (!Directory.Exists(Folder))
			Directory.CreateDirectory(Folder);
		string path = GetPath(key);
		File.WriteAllBytes(path, rawData);
	}

	static void StoreFileVersion(string key, int version)
	{
		PlayerPrefs.SetInt(KeyPrefix + key, version);
		PlayerPrefs.Save();
	}

	// 	===============
	//	删除缓存文件
	//	===============
	public static void ClearAll()
	{
		if (Directory.Exists(Folder)) {
			var fileInfos = MiscUtils.GetFileInfoFromFolder(Folder, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < fileInfos.Count; ++i) {
				Delete(fileInfos[i].Name);
			}
		}
	}

	public static void Delete(string key)
	{
		DeleteFile(key);
		DeleteFileVersion(key);
	}

	static void DeleteFile(string key)
	{
		string path = GetPath(key);
		if (File.Exists(path)) {
			File.Delete(path);
		}
	}

	static void DeleteFileVersion(string key)
	{
		PlayerPrefs.DeleteKey(KeyPrefix + key);
	}

	// 	===============
	//	读取缓存文件
	//	===============
	public static byte[] GetFileContent(string key)
	{
		if (IsFileExist(key)) {
			string path = GetPath(key);
			return File.ReadAllBytes(path);
		}
		return null;
	}

	public static string GetFilePath(string key)
	{
		string path = GetPath(key);
		if (File.Exists(path)) {
			return "file:///" + path;
		}
		return null;
	}

	public static bool IsFileExist(string key)
	{
		string path = GetPath(key);
		return File.Exists(path);
	}

	// 	===============
	//	判断是否过期
	//	===============
	public static bool HasExpired(string key, int version)
	{
		if (IsFileExist(key)) {
			if (GetFileVersion(key) >= version)
				return false;
		}

		return true;
	}

	public static int GetFileVersion(string key)
	{
		return PlayerPrefs.GetInt(KeyPrefix + key);
	}

}

