using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class XcodeModifyQQ
{
	[PostProcessBuild(5)]
	public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget == BuildTarget.iOS) {
			#if UNITY_IPHONE

			// 修改xcode工程
			string projPath = PBXProject.GetPBXProjectPath(path);
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");

			//添加xcode默认framework引用
			proj.AddFrameworkToProject(target, "Security.framework", false);
			proj.AddFrameworkToProject(target, "libiconv.tbd", false);
			proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
			proj.AddFrameworkToProject(target, "CoreGraphics.framework", false);
			proj.AddFrameworkToProject(target, "libsqlite3.tbd", false);
			proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
			proj.AddFrameworkToProject(target, "libstdc++.tbd", false);
			proj.AddFrameworkToProject(target, "libz.tbd", false);

			string fileName = Application.dataPath.Replace("Assets", "iOS/QQ");
			XcodeModifyGeneral.CopyAndReplaceDirectory(fileName, Path.Combine(path, "QQ"));
			List<string> filePaths = new List<string>();
			XcodeModifyGeneral.AddFilesToBuild(ref filePaths, path, "QQ");
			foreach (var filepath in filePaths)
				proj.AddFileToBuild(target, proj.AddFile(filepath, filepath));
			
			proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/QQ");

			proj.WriteToFile(projPath);

			//获取info.plist
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));
			PlistElementDict rootDict = plist.root;

			//添加URLTypes
			PlistElementArray URLTypes;
			if (rootDict.values.ContainsKey("CFBundleURLTypes"))
				URLTypes = rootDict["CFBundleURLTypes"].AsArray();
			else
				URLTypes = rootDict.CreateArray("CFBundleURLTypes");

			PlistElementDict elementDict = new PlistElementDict();
			elementDict.SetString("CFBundleTypeRole", "Editor");
			elementDict.SetString("CFBundleURLName", "tencent");
			elementDict.CreateArray("CFBundleURLSchemes").AddString("tencent" + ShareManager.QQAppId_iOS);

			URLTypes.values.Add(elementDict);

			//添加LSApplicationQueriesSchemes
			PlistElementArray schemes;
			if (rootDict.values.ContainsKey("LSApplicationQueriesSchemes"))
				schemes = rootDict["LSApplicationQueriesSchemes"].AsArray();
			else
				schemes = rootDict.CreateArray("LSApplicationQueriesSchemes");

			schemes.AddString("mqqopensdkapiV4");
			schemes.AddString("mqq");
			schemes.AddString("mqzone");
			schemes.AddString("mqqwpa");
			schemes.AddString("mqqapi");
			schemes.AddString("wtloginmqq2");
			schemes.AddString("mqqopensdkapiV3");
			schemes.AddString("mqqopensdkapiV2");
			schemes.AddString("mqzoneopensdk");
			schemes.AddString("mqzoneopensdkapiV2");
			schemes.AddString("mqzoneopensdkapi19");
			schemes.AddString("mqzoneopensdkapi");
			schemes.AddString("mqqOpensdkSSoLogin");

			plist.WriteToFile(plistPath);
			#endif
		}
	}

}

