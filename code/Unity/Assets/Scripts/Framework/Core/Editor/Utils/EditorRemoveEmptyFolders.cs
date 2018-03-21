using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EditorRemoveEmptyFolders
{
	[MenuItem("自定义/工具/删除空文件夹")]
	static void FindAndRemove()
	{
		var root = Application.dataPath;
		string[] dirs = Directory.GetDirectories(root, "*", SearchOption.AllDirectories);
		List<DirectoryInfo> emptyDirs = new List<DirectoryInfo>();
		foreach (var dir in dirs) {
			DirectoryInfo di = new DirectoryInfo(dir);
			if (IsDirectoryEmpty(di))
				emptyDirs.Add(di);
		}
		foreach (var emptyDir in emptyDirs) {
			if (Directory.Exists(emptyDir.FullName)) {
				Directory.Delete(emptyDir.FullName, true);
				Debug.Log("Recursively delete folder: " + emptyDir.FullName);
			}
		}
		AssetDatabase.Refresh();
	}

	static bool IsDirectoryEmpty(DirectoryInfo dir)
	{
		if (HasNoFile(dir)) {
			var subDirs = dir.GetDirectories();
			bool allEmpty = true;
			foreach (var subDir in subDirs) {
				if (!IsDirectoryEmpty(subDir)) {
					allEmpty = false;
					break;
				}
			}
			return allEmpty;
		}
		return false;
	}

	static bool HasNoFile(DirectoryInfo dir)
	{
		bool noFile = true;
		foreach (var file in dir.GetFiles ()) {
			if (file.Name == ".DS_Store")
				continue;

			if (file.Name.EndsWith(".meta") && Directory.Exists(
				    Path.Combine(dir.FullName, file.Name.Substring(0, file.Name.IndexOf(".meta")))))
				continue;

			noFile = false;
			break;
		}
		return noFile;
	}

}
