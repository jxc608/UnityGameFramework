using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public class EditorAssetBundle : EditorWindow
{
	static EditorAssetBundle _currentWindow = null;

	static readonly float _initWidth = 300f;
	static readonly float _initHeight = 350f;

	[MenuItem("自定义/Asset Bundle %l", false, 4)]
	static void OpenAssetBundle()
	{
		_currentWindow = EditorWindow.GetWindow<EditorAssetBundle>(false, "Asset Bundle");
		_currentWindow.minSize = new Vector2(_initWidth, _initHeight);
		_currentWindow.Show();
	}

	public static string m_BundleDirectory = "";
	public static string m_AssetBundlePath = "";

	static string GetTargetPath(BuildTarget target)
	{
		return m_AssetBundlePath + target.ToString() + "/";
	}

	void Awake()
	{
		m_BundleDirectory = Application.dataPath + "/AssetBundle/";
		m_AssetBundlePath = Application.streamingAssetsPath + "/AssetBundle/";
	}

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0, 0, _currentWindow.position.width, _currentWindow.position.height), 
		                     GUI.skin.GetStyle("RL Background"));

		var target = EditorUserBuildSettings.activeBuildTarget;

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if (GUILayout.Button("一键打包 (依次执行下面五步)", GUILayout.Width(_initWidth), GUILayout.ExpandWidth(true))) {
			BuildBundleAllInOne(target);
			_currentWindow.Close();
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if (GUILayout.Button("设置资源", GUILayout.Width(_initWidth), GUILayout.ExpandWidth(true))) {
			SetAllAssetName(target);
			_currentWindow.Close();
		}

		EditorGUILayout.Space();
		if (GUILayout.Button("打包资源", GUILayout.Width(_initWidth), GUILayout.ExpandWidth(true))) {
			BuildBundle(target);
			_currentWindow.Close();
		}

		EditorGUILayout.Space();
		if (GUILayout.Button("打包场景", GUILayout.Width(_initWidth), GUILayout.ExpandWidth(true))) {
			BuildScenes(target);
			_currentWindow.Close();
		}

		EditorGUILayout.Space();
		if (GUILayout.Button("合成Upk", GUILayout.Width(_initWidth), GUILayout.ExpandWidth(true))) {
			UpkPackGeneral(target);
			_currentWindow.Close();
		}

		EditorGUILayout.Space();
		if (GUILayout.Button("拷贝至GOPATH", GUILayout.Width(_initWidth), GUILayout.ExpandWidth(true))) {
			ExecuteCopyFile(target);
			_currentWindow.Close();
		}


		GUILayout.EndArea();
	}

	#region build bundles

	static void BuildBundleAllInOne(BuildTarget target)
	{
		SetAllAssetName(target);
		if (BuildBundle(target)) {
			BuildScenes(target);
			UpkPackGeneral(target);
			ExecuteCopyFile(target);
		}
	}

	static void SetAllAssetName(BuildTarget target)
	{
		if (!ConfigurationController.Instance.BuildAssetBundle)
			return;

		try {
			m_BundleNameList.Clear();
			m_BundleNameList.Add(target.ToString());

			string bundleDirPath = GetTargetPath(target);
			if (!Directory.Exists(bundleDirPath))
				Directory.CreateDirectory(bundleDirPath);
			ClearDirectoryAssetName(Application.dataPath + "/Resources/");
			ClearDirectoryAssetName(Application.dataPath + "/Prefabs/");
			ClearDirectoryAssetName(Application.dataPath + "/Materials/");
			Dictionary<string, List<string>> bundleAndPaths = new Dictionary<string, List<string>>();
			EditorUtility.DisplayProgressBar("Bundle Name", "设置中...", .1f);
			SetDirectoryAssetName(Application.dataPath + "/Artworks/", ref bundleAndPaths);
			var allPrefabBundleNames = SetDirectoryAssetName(m_BundleDirectory, ref bundleAndPaths);
			SetDirectoryAssetName(Application.dataPath + "/Fonts/", ref bundleAndPaths);
			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(bundleDirPath, BuildAssetBundleOptions.DryRunBuild, target);
			ClearDirectoryAssetName(Application.dataPath + "/Artworks/");
			ClearDirectoryAssetName(Application.dataPath + "/Fonts/");
			foreach (var bundleName in allPrefabBundleNames) {
				SetBundleNameRecursively(bundleName, manifest, bundleAndPaths, ref m_BundleNameList);
			}

			#region create file BundleNameList.txt for streamingAssets use
			StringBuilder sb = new StringBuilder();
			foreach (string s in m_BundleNameList) {
				sb.Append(s + "|");
			}
			File.WriteAllText(bundleDirPath + "../BundleNameList.txt", sb.ToString());
			#endregion
		} finally {
			EditorUtility.ClearProgressBar();
		}
	}

	private static List<string> m_BundleNameList = new List<string>();
	private static Dictionary<string, string> m_BundleMap = new Dictionary<string, string>();

	public static bool BuildBundle(BuildTarget target)
	{
		string bundleDirPath = GetTargetPath(target);
		if (Directory.Exists(bundleDirPath))
			Directory.Delete(bundleDirPath, true);

		if (!ConfigurationController.Instance.BuildAssetBundle)
			return true;

		try {
			Caching.CleanCache();
			m_BundleMap.Clear();

			Directory.CreateDirectory(bundleDirPath);
			BuildPipeline.BuildAssetBundles(bundleDirPath, BuildAssetBundleOptions.None, target);

			string[] manifests = Directory.GetFiles(bundleDirPath, "*.manifest", SearchOption.AllDirectories);
			foreach (string s in manifests) {
				if (s.Replace(bundleDirPath, "") == MiscUtils.GetCurrentPlatform() + ".manifest")
					continue;
				File.Delete(s);
			}
		} catch (Exception e) {
			Debug.Log(e.Message);
			return false;
		} finally {
			AssetDatabase.Refresh();
		}

		return true;
	}

	static string FullPath2BundleName(string fullPath)
	{
		string extension = Path.GetExtension(fullPath);
		if (string.IsNullOrEmpty(extension))
			return fullPath.Replace("\\", "/").Replace(Application.dataPath + "/", "").Replace(" ", "_");
		else
			return fullPath.Replace("\\", "/").Replace(Application.dataPath + "/", "").Replace(extension, "").Replace(" ", "_");
	}

	static string FullPath2RelativePath(string fullPath)
	{
		var relativeFilePath = fullPath.Replace("\\", "/").Replace(Application.dataPath, "Assets");
		return relativeFilePath;
	}

	private static List<string> SetDirectoryAssetName(string dirPath, ref Dictionary<string, List<string>> bundleAndPath)
	{
		List<string> bundleNames = new List<string>();
		string[] filePaths = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories);
		foreach (string file in filePaths) {
			string extension = Path.GetExtension(file);
			if (extension != ".meta" && extension != ".DS_Store") {
				var relativeFilePath = FullPath2RelativePath(file);
				AssetImporter ai = AssetImporter.GetAtPath(relativeFilePath);
				if (ai != null) {
					if (ai is TextureImporter && !string.IsNullOrEmpty(((TextureImporter)ai).spritePackingTag)) {
						ai.assetBundleName = "atlas/" + ((TextureImporter)ai).spritePackingTag;
					} else {
						ai.assetBundleName = FullPath2BundleName(file);
					}

					if (!bundleNames.Contains(ai.assetBundleName))
						bundleNames.Add(ai.assetBundleName);
					if (!bundleAndPath.ContainsKey(ai.assetBundleName))
						bundleAndPath.Add(ai.assetBundleName, new List<string>() { relativeFilePath });
					else
						bundleAndPath[ai.assetBundleName].Add(relativeFilePath);
				} else {
					Debug.LogError("the null assetImporter is:" + file);
				}
			}
		}
		return bundleNames;
	}

	private static void ClearDirectoryAssetName(string dirPath)
	{
		if (Directory.Exists(dirPath)) {
			string[] filePaths = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories);
			foreach (string file in filePaths) {
				string extension = Path.GetExtension(file);
				if (extension != ".meta" && extension != ".DS_Store") {
					AssetImporter ai = AssetImporter.GetAtPath(file.Replace("\\", "/").Replace(Application.dataPath, "Assets"));
					if (ai != null) {
						ai.assetBundleName = "";
					} else {
						Debug.LogError("the null assetImporter is:" + file);
					}
				}
			}
		}
	}

	static void SetBundleNameRecursively(string bundleName, AssetBundleManifest manifest, Dictionary<string, List<string>> bundleAndPaths, ref List<string> bundleNameList)
	{
		if (bundleAndPaths.ContainsKey(bundleName)) {
			foreach (var path in bundleAndPaths [bundleName]) {
				AssetImporter ai = AssetImporter.GetAtPath(path);
				if (ai != null) {
					if (ai is TextureImporter && !string.IsNullOrEmpty(((TextureImporter)ai).spritePackingTag)) {
						ai.assetBundleName = "atlas/" + ((TextureImporter)ai).spritePackingTag;
					} else {
						ai.assetBundleName = bundleName;
					}
					if (!bundleNameList.Contains(ai.assetBundleName))
						bundleNameList.Add(ai.assetBundleName);
				} else {
					throw new Exception("Error! AssetImporter.GetAtPath find no asset: " + path);
				}
			}

			string[] dependencies = manifest.GetAllDependencies(bundleName);
			foreach (string dependencyBundleName in dependencies) {
				SetBundleNameRecursively(dependencyBundleName, manifest, bundleAndPaths, ref bundleNameList);
			}
		} else {
			throw new Exception("Error! bundleAndPaths does not contains a key: " + bundleName);
		}
	}

	#endregion

	#region build scenes

	static void BuildScenes(BuildTarget target)
	{
		if (!ConfigurationController.Instance.BuildSceneAsBundle)
			return;
		
		var folder = Application.dataPath + "/StreamingAssets/AssetBundle/" + target.ToString() + "/scenes/";
		if (Directory.Exists(folder))
			Directory.Delete(folder, true);
		Directory.CreateDirectory(folder);
		string[] scenes = EditorUtils.GetSceneArrayByEditorScenes();
		foreach (var scene in scenes) {
			if (ConfigurationController.Instance.SceneIsBundle(scene)) {
				var fileName = Path.GetFileNameWithoutExtension(scene) + ".unity3d";
				BuildPipeline.BuildPlayer(new string[] { scene }, folder + fileName, 
				                           target, BuildOptions.BuildAdditionalStreamedScenes);
				
				File.AppendAllText(GetTargetPath(target) + "../BundleNameList.txt", "scenes/" + fileName + "|");
			}
		}
		AssetDatabase.Refresh();
	}

	#endregion

	#region combine .zips to .upk

	static void UpkPackGeneral(BuildTarget target)
	{
		if (!ConfigurationController.Instance.BuildAssetBundle && !ConfigurationController.Instance.BuildSceneAsBundle)
			return;
		
		var folderPath = GetTargetPath(target);
		if (Directory.Exists(folderPath)) {
			UpkPack.PackFolder(folderPath, folderPath + "../" + target.ToString() + ".upk", true);

			string tmpFile = ExecutePrepareEarlyBundlePath(target);
			var paths = File.ReadAllLines(tmpFile);
			if (paths != null && paths.Length > 0) {
				foreach (var path in paths) {
					var oldFolderName = Path.GetFileName(path);
					var oldVersion = oldFolderName.Substring(oldFolderName.LastIndexOf("_") + 1);

					var comparePath = Path.Combine(path, target.ToString());
					bool hasComparePath = false;
					if (!string.IsNullOrEmpty(comparePath) && Directory.Exists(comparePath)) {
						if (!comparePath.EndsWith("/"))
							comparePath += "/";
						hasComparePath = true;
					}
					UpkPack.PackFolder(folderPath, folderPath + "../" + target.ToString() + "_" + oldVersion + ".upk", true, 
					                    delegate (FileInfo fileInfo) {
						if (hasComparePath) {
							var compareFile = fileInfo.FullName.Replace(folderPath, comparePath);
							if (File.Exists(compareFile)) {
								if (MiscUtils.GetMD5HashFromFile(compareFile) == MiscUtils.GetMD5HashFromFile(fileInfo.FullName)) {
									return true;
								}
							}
						}
						return false;
					});
				}
			}
			AssetDatabase.Refresh();
			File.Delete(tmpFile);
		}
	}

	static string ExecutePrepareEarlyBundlePath(BuildTarget target)
	{
		string scriptPath = Application.dataPath + "/Tools/Framework/BuildBundles";
		string scriptName = scriptPath + "/prepare_early_bundle_path";
		string tmpFile = scriptName + ".tmp";
		EditorUtils.ProcessCommand(scriptName, new string[] {
			PlayerSettings.bundleVersion,
			tmpFile,
			ConfigurationController.Instance.ServerProjectName,
		});

		return tmpFile;
	}

	#endregion

	#region copy to GOPATH

	static void ExecuteCopyFile(BuildTarget target)
	{
		if (!ConfigurationController.Instance.BuildAssetBundle && !ConfigurationController.Instance.BuildSceneAsBundle) {
			return;
		}
		
		string scriptPath = Application.dataPath + "/Tools/Framework/BuildBundles";
		string tmpPath = GetTargetPath(target);
		if (Directory.Exists(tmpPath)) {
			EditorUtils.ProcessCommand(scriptPath + "/copy_bundles", new string[] {
				tmpPath,
				target.ToString(),
				PlayerSettings.bundleVersion,
				Application.streamingAssetsPath + "/AssetBundle/",
				ConfigurationController.Instance.ServerProjectName,
			});
		} else {
			Debug.LogError("Error! Directory not exist: " + tmpPath);
		}
	}

	#endregion
}
