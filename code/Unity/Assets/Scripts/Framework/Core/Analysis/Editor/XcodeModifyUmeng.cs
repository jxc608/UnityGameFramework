using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif
using System.Collections.Generic;
using System.IO;

public class XcodeModifyUmeng
{
	[PostProcessBuild(4)]
	public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget == BuildTarget.iOS) {
			#if UNITY_IPHONE

			// 修改xcode工程
			string projPath = PBXProject.GetPBXProjectPath(path);
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");

			proj.AddFrameworkToProject(target, "libz.tbd", false);

			string fileName = Application.dataPath.Replace("Assets", "iOS/Umeng");
			XcodeModifyGeneral.CopyAndReplaceDirectory(fileName, Path.Combine(path, "Umeng"));
			List<string> filePaths = new List<string>();
			XcodeModifyGeneral.AddFilesToBuild(ref filePaths, path, "Umeng");
			foreach (var filepath in filePaths)
				proj.AddFileToBuild(target, proj.AddFile(filepath, filepath));

			proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Umeng");

			proj.WriteToFile(projPath);

			//获取info.plist
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));
			PlistElementDict rootDict = plist.root;

			rootDict.SetString("NSLocationWhenInUseUsageDescription", "MusicGame需要访问您的位置信息");

			plist.WriteToFile(plistPath);
			#endif
		}
	}

}

