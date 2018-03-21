using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class EditorFrameworkExport
{
	[MenuItem("Framework/导出/gz")]
	static void ExportAsGzipWithoutEx()
	{
		ExportAsGzip(false);
	}

	[MenuItem("Framework/导出/gz(带Ex)")]
	static void ExportAsGzipWithEx()
	{
		ExportAsGzip(true);
	}

	public static string ExportAsGzip(bool withEx)
	{
		string root = Path.GetFullPath(Application.dataPath + "/../../../");

		// 获取本工程中git忽略的文件列表
		List<string> gitIgnores = GetGitIgnores(root);
		if (gitIgnores == null)
			return "";

		try {
			// 解析Framework配置文件
			var configurations = ParseFrameworkConfiguration(root + "framework/framework.conf");
			string version = configurations["version"][0];
			var foldersCopyAll = configurations["folders_copy_all"];
			var ignores = new List<string>();
			var ignoresEx = new List<string>();
			if (configurations.ContainsKey("ignore_public")) {
				ignores.AddRange(configurations["ignore_public"]);
				ignoresEx.AddRange(configurations["ignore_public"]);
			}
			if (configurations.ContainsKey("ignore_framework"))
				ignores.AddRange(configurations["ignore_framework"]);
			if (configurations.ContainsKey("ignore_framework_ex"))
				ignoresEx.AddRange(configurations["ignore_framework_ex"]);

			string fileMap = "";
			bool hasRecordFileMap = false;
			UpkPack.Ignored ignoreFunc = (FileInfo fileInfo) => {
				bool ex = withEx && hasRecordFileMap;
				var relativePath = fileInfo.FullName.Replace(root, "").Replace('\\', '/');
				// 过滤所有不在Framework文件夹中的文件
				if (!IsInFrameworkFolder(relativePath, foldersCopyAll, ex))
					return true;
				// 过滤.gitignore中忽略的文件
				if (IsIgnored(relativePath, gitIgnores))
					return true;
				// 过滤.frameworkignore中忽略的文件
				if (IsIgnored(relativePath, ex ? ignoresEx : ignores))
					return true;
				// 过滤非本版本的文件地图文件
				if (IsInvalidFileMap(relativePath, version))
					return true;

				if (!hasRecordFileMap)
					fileMap += relativePath + "\n";

				return false;
			};
			// 记录文件地图
			UpkPack.TraverseFolder(root, false, ignoreFunc);
			var fileMapPath = Path.Combine(root, "framework/filemap/" + GetFileMapName(version));
			CheckCreateDirectory(fileMapPath);
			File.WriteAllText(fileMapPath, fileMap);
			hasRecordFileMap = true;
			// 合并为upk
			var upkPath = Path.Combine(root, "framework/output/v" + version + (withEx ? "_ex" : "") + ".upk");
			CheckCreateDirectory(upkPath);
			UpkPack.PackFolder(root, upkPath, false, ignoreFunc);
			// 压缩为gzip
			var gzipPath = Path.Combine(root, "framework/output/v" + version + (withEx ? "_ex" : "") + ".upk.gz");
			CheckCreateDirectory(gzipPath);
			File.WriteAllBytes(gzipPath, GZipUtil.Zip(File.ReadAllBytes(upkPath)));
			File.Delete(upkPath);
			return gzipPath;
		} catch (System.Exception e) {
			Debug.LogError(e.Message + "\n" + e.StackTrace);
			return "";
		}
	}

	static List<string> GetGitIgnores(string root)
	{
		var scriptPath = Application.dataPath + "/Tools/Framework/Framework/get_git_ignore";
		var logPath = "framework/output/ignore.log";
		var logFullPath = Path.Combine(root, logPath);
		CheckCreateDirectory(logFullPath);
		EditorUtils.ProcessCommand(scriptPath, new string[] {
			root,
			logPath,
		}, true);

		List<string> ignores = null;
		if (File.Exists(logFullPath)) {
			ignores = new List<string>();
			var lines = File.ReadAllLines(logFullPath);
			foreach (var line in lines) {
				ignores.Add(line.Replace("Would remove ", ""));
			}
		}
		File.Delete(logFullPath);
		return ignores;
	}

	static Dictionary<string, List<string>> ParseFrameworkConfiguration(string path)
	{
		var configurations = new Dictionary<string, List<string>>();
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
		return configurations;
	}

	static bool IsInFrameworkFolder(string relativePath, List<string> foldersCopyAll, bool withEx)
	{
		bool isIn = false;
		if (relativePath.StartsWith("framework/") || relativePath.Contains("/framework/")
		    || relativePath.StartsWith("Framework/") || relativePath.Contains("/Framework/"))
			isIn = true;

		if (!isIn) {
			if (withEx && (relativePath.StartsWith("frameworkEx/") || relativePath.Contains("/frameworkEx/")
			    || relativePath.StartsWith("FrameworkEx/") || relativePath.Contains("/FrameworkEx/")))
				isIn = true;
		}

		if (!isIn) {
			for (int i = 0; i < foldersCopyAll.Count; ++i) {
				if (foldersCopyAll[i].StartsWith("/")) {
					if (relativePath.Equals(foldersCopyAll[i].Substring(1, foldersCopyAll[i].Length - 1))) {
						isIn = true;
						break;
					}
				} else {
					if (relativePath.Contains(foldersCopyAll[i])) {
						isIn = true;
						break;
					}
				}
			}
		}

		if (isIn) {
			if (!withEx) {
				if (relativePath.StartsWith("debug/") || relativePath.Contains("/debug/")
				    || relativePath.StartsWith("Debug/") || relativePath.Contains("/Debug/")
				    || relativePath.EndsWith("debug.meta") || relativePath.EndsWith("Debug.meta"))
					isIn = false;
			}
		}

		return isIn;
	}

	static bool IsIgnored(string relativePath, List<string> gitIgnores)
	{
		foreach (var ignore in gitIgnores) {
			if (relativePath.StartsWith(ignore))
				return true;
		}
		return false;
	}

	static string GetFileMapName(string version)
	{
		return "v" + version + ".txt";
	}

	static bool IsInvalidFileMap(string relativePath, string version)
	{
		if (Path.GetDirectoryName(relativePath).Equals("framework/filemap")) {
			if (!Path.GetFileName(relativePath).Equals(GetFileMapName(version))) {
				return true;
			}
		}
		return false;
	}

	static void CheckCreateDirectory(string filePath)
	{
		if (!Directory.Exists(Path.GetDirectoryName(filePath)))
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
	}

}
