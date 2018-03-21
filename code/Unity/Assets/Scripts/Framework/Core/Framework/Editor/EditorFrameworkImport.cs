using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

public class EditorFrameworkImport
{
	[MenuItem("Framework/新建工程")]
	static void NewProject()
	{
		string root = EditorUtility.OpenFolderPanel("选择工程目录", Path.GetFullPath(Application.dataPath + "/../../../../"), "framework_new");
		if (!string.IsNullOrEmpty(root)) {
			ImportFromGzip(root, true, true);
		}
	}

	[MenuItem("Framework/升级其他工程")]
	static void UpgradeOtherProject()
	{
		string root = EditorUtility.OpenFolderPanel("选择工程目录", Path.GetFullPath(Application.dataPath + "/../../../../"), "framework_new");
		if (!string.IsNullOrEmpty(root)) {
			ImportFromGzip(root, false, true);
		}
	}


	[MenuItem("Framework/升级本工程")]
	static void UpgradeProject()
	{
		string root = Path.GetFullPath(Application.dataPath + "/../../../");
		ImportFromGzip(root, false, false);
	}

	static void ImportFromGzip(string root, bool withEx, bool generateGz)
	{
		string[] oldFileMap = null;
		var configurations = ParseFrameworkConfiguration(Path.Combine(root, "framework/framework.conf"));
		if (configurations.Count == 0)
			configurations = ParseFrameworkConfiguration(Path.Combine(root, "framework/FRAMEWORK.md"));
		string oldVersion = "";
		if (configurations.ContainsKey("version")) {
			oldVersion = configurations["version"][0];
			var fileMapPath = Path.Combine(root, "framework/filemap/v" + oldVersion + ".txt");
			if (File.Exists(fileMapPath)) {
				oldFileMap = File.ReadAllLines(fileMapPath);
			}
		}

		bool import = false;
		if (oldFileMap == null) {
			if (EditorUtility.DisplayDialog("⚠️ 警告!", "目标工程并非基于某一个玩创Lab框架版本, 是否确定要导入?", "确认", "取消")) {
				import = true;
			}
		} else
			import = true;

		if (import) {
			List<string> pathList = null;
			bool success = false;
			var filePath = "";
			if (!generateGz) {
				filePath = EditorUtility.OpenFilePanel("选择gz文件", Path.Combine(root, "../"), "gz");
			} else {
				filePath = EditorFrameworkExport.ExportAsGzip(withEx);
			}
			if (!string.IsNullOrEmpty(filePath)) {
				var upkPath = Path.Combine(root, "framework/output/" + Path.GetFileNameWithoutExtension(filePath));
				CheckCreateDirectory(upkPath);
				File.WriteAllBytes(upkPath, GZipUtil.UnZip(File.ReadAllBytes(filePath)));
				success = UPKExtra.ExtraUPK(upkPath, root, ref pathList);
				if (File.Exists(upkPath))
					File.Delete(upkPath);
			}

			if (success) {
				if (oldFileMap != null && oldFileMap.Length > 0) {
					foreach (var oldFile in oldFileMap) {
						if (!pathList.Contains(oldFile)) {
							Debug.Log("Delete old version file: " + oldFile);
							if (File.Exists(Path.Combine(root, oldFile)))
								File.Delete(Path.Combine(root, oldFile));
						}
					}
				}
			}

			AssetDatabase.Refresh();
		}
	}

	static void CheckCreateDirectory(string filePath)
	{
		if (!Directory.Exists(Path.GetDirectoryName(filePath)))
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
	}


	static Dictionary<string, List<string>> ParseFrameworkConfiguration(string path)
	{
		var configurations = new Dictionary<string, List<string>>();
		if (File.Exists(path)) {
			string currentKey = "";
			var lines = File.ReadAllLines(path);
			foreach (var line in lines) {
				if (line.EndsWith(":")) {
					currentKey = line.Substring(0, line.Length - 1).Trim();
					configurations.Add(currentKey, new List<string>());
				} else {
					var ele = line.Trim();
					if (!string.IsNullOrEmpty(ele) && !ele.StartsWith("#"))
						configurations[currentKey].Add(ele);
				}
			}
		}
		return configurations;
	}

}
