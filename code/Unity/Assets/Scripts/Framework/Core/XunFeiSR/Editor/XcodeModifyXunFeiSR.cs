using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif
using System.Collections.Generic;
using System.IO;

public class XcodeModifyXunFeiSR
{
    [PostProcessBuild(6)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
#if UNITY_IPHONE

			// 修改xcode工程
			string projPath = PBXProject.GetPBXProjectPath(path);
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");

			proj.AddFrameworkToProject(target, "libz.tbd", false);
            proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);

			string fileName = Application.dataPath.Replace("Assets", "iOS/XunFeiSR");
			XcodeModifyGeneral.CopyAndReplaceDirectory(fileName, Path.Combine(path, "XunFeiSR"));
			List<string> filePaths = new List<string>();
			XcodeModifyGeneral.AddFilesToBuild(ref filePaths, path, "XunFeiSR");
			foreach (var filepath in filePaths)
				proj.AddFileToBuild(target, proj.AddFile(filepath, filepath));

			proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/XunFeiSR");
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");

			proj.WriteToFile(projPath);
#endif
        }
    }

}

