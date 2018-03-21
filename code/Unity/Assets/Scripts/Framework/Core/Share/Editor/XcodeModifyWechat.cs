using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class XcodeModifyWechat
{
	[PostProcessBuild(3)]
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
			proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
			proj.AddFrameworkToProject(target, "libz.tbd", false);
			proj.AddFrameworkToProject(target, "libsqlite3.0.tbd", false);
			proj.AddFrameworkToProject(target, "libc++.tbd", false);
			proj.AddFrameworkToProject(target, "Security.framework", false);
			proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
			proj.AddFrameworkToProject(target, "CFNetwork.framework", false);

			proj.UpdateBuildProperty(target, "OTHER_LDFLAGS", new List<string>(){ "-Objc" }, new List<string>(){ "-Objc" });
			proj.SetBuildProperty(target, "IPHONEOS_DEPLOYMENT_TARGET", "8.0");

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
			elementDict.SetString("CFBundleURLName", "weixin");
			elementDict.CreateArray("CFBundleURLSchemes").AddString(ShareManager.WechatAppId);

			URLTypes.values.Add(elementDict);

			//添加LSApplicationQueriesSchemes
			PlistElementArray schemes;
			if (rootDict.values.ContainsKey("LSApplicationQueriesSchemes"))
				schemes = rootDict["LSApplicationQueriesSchemes"].AsArray();
			else
				schemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
			schemes.AddString("weixin");

			plist.WriteToFile(plistPath);
			#endif
		}
	}

}

