using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

public class XcodeModifyGeneral
{
	[PostProcessBuild(1)]
	public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget == BuildTarget.iOS) {
#if UNITY_IPHONE

			// 修改xcode工程
			string projPath = PBXProject.GetPBXProjectPath(path);
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");

			proj.RemoveFrameworkFromProject(target, "MetalKit.framework");
			proj.AddFrameworkToProject(target, "MetalKit.framework", true);

			if (File.Exists(Application.dataPath.Replace("Assets", "iOS/UnityAppController.mm"))) {
				File.Delete(Path.Combine(path, "Classes/UnityAppController.h"));
				File.Delete(Path.Combine(path, "Classes/UnityAppController.mm"));
				File.Copy(Application.dataPath.Replace("Assets", "iOS/UnityAppController.h"), Path.Combine(path, "Classes/UnityAppController.h"));
				File.Copy(Application.dataPath.Replace("Assets", "iOS/UnityAppController.mm"), Path.Combine(path, "Classes/UnityAppController.mm"));
			}

			proj.SetBuildProperty(target, "GCC_GENERATE_DEBUGGING_SYMBOLS", "No");
			proj.SetBuildProperty(target, "IPHONEOS_DEPLOYMENT_TARGET", "8.0");
			proj.SetBuildProperty(target, "TARGETED_DEVICE_FAMILY", "1,2");

			proj.WriteToFile(projPath);

			//获取info.plist
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));
			PlistElementDict rootDict = plist.root;

			rootDict.SetString("NSCameraUsageDescription", "MusicGame需要访问您的摄像机");
            rootDict.SetString("NSSpeechRecognitionUsageDescription", "MusicGame需要访问您的麦克风");
            rootDict.SetString("NSMicrophoneUsageDescription", "MusicGame需要访问您的麦克风");

             

			plist.WriteToFile(plistPath);
#endif
        }
    }

	#if UNITY_IPHONE
	public static void CopyAndReplaceDirectory(string srcPath, string dstPath)
	{
		if (Directory.Exists(dstPath))
			Directory.Delete(dstPath);
		if (File.Exists(dstPath))
			File.Delete(dstPath);

		Directory.CreateDirectory(dstPath);

		foreach (var file in Directory.GetFiles(srcPath))
			File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));

		foreach (var dir in Directory.GetDirectories(srcPath))
			CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
	}

	public static void AddFilesToBuild(ref List<string> filepaths, string projPath, string pathToProj)
	{
		var path = Path.Combine(projPath, pathToProj);
		string[] directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
		for (int i = 0; i < directories.Length; i++) {
			string directory = directories[i];
			var eachPathToProj = directory.Replace(projPath, "").Substring(1);
			if (!directory.EndsWith("framework") && !directory.EndsWith("bundle")) {
				AddFilesToBuild(ref filepaths, projPath, eachPathToProj);
			} else {
				if (!filepaths.Contains(eachPathToProj))
					filepaths.Add(eachPathToProj);
			}
		}

		string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
		for (int i = 0; i < files.Length; i++) {
			string file = files[i];
			string extension = Path.GetExtension(file);
			if (extension != ".DS_Store" && extension != ".meta") {
				var eachPathToProj = file.Replace(projPath, "").Substring(1);
				if (!filepaths.Contains(eachPathToProj))
					filepaths.Add(eachPathToProj);
			}
		}
	}

	public static void AddFrameworkToEmbed(string path, string frameworkName)
	{
		var filePath = Path.Combine(path, "Unity-iPhone.xcodeproj/project.pbxproj");
		int tokenLength = 24;

		var lines = new List<string>(File.ReadAllLines(filePath));

		// 加入embed framework的line
		var index = GetLineIndexThatContains(lines, frameworkName + " in Frameworks");
		var line = lines[index];
		var frameworkToken = line.Substring(0, line.IndexOf(@"/*")).Trim();
		var fileRef = line.Substring(line.IndexOf("fileRef = ") + "fileRef = ".Length, tokenLength);
		var embedFrameworkLineToken = GetNextToken(frameworkToken);
		var embedFrameworkLine = "\t\t" + embedFrameworkLineToken + " /* " + frameworkName + " in Embed Frameworks */ = {isa = PBXBuildFile; fileRef = " + fileRef + " /* " + frameworkName + " */; settings = {ATTRIBUTES = (CodeSignOnCopy, RemoveHeadersOnCopy, ); }; };";
		lines.Insert(index + 1, embedFrameworkLine);

		// 加入embed framework的section
		index = GetLineIndexThatContains(lines, "/* Begin PBXCopyFilesBuildPhase section */");
		var embedFrameworkSectionLineIndex = GetLineIndexThatContains(lines, "/* Embed Frameworks */ = {", index);
		if (embedFrameworkSectionLineIndex > -1) {
			index = GetLineIndexThatContains(lines, "files = (", embedFrameworkSectionLineIndex);
			lines.Insert(++index, "\t\t\t\t" + embedFrameworkLineToken + " /* " + frameworkName + " in Embed Frameworks */,");
		} else {
			var embedFrameworkSectionToken = GetNextToken(embedFrameworkLineToken);
			lines.Insert(++index, "\t\t" + embedFrameworkSectionToken + " /* Embed Frameworks */ = {");
			lines.Insert(++index, "\t\t\tisa = PBXCopyFilesBuildPhase;");
			lines.Insert(++index, "\t\t\tbuildActionMask = 2147483647;");
			lines.Insert(++index, "\t\t\tdstPath = \"\";");
			lines.Insert(++index, "\t\t\tdstSubfolderSpec = 10;");
			lines.Insert(++index, "\t\t\tfiles = (");
			lines.Insert(++index, "\t\t\t\t" + embedFrameworkLineToken + " /* " + frameworkName + " in Embed Frameworks */,");
			lines.Insert(++index, "\t\t\t);");
			lines.Insert(++index, "\t\t\tname = \"Embed Frameworks\";");
			lines.Insert(++index, "\t\t\trunOnlyForDeploymentPostprocessing = 0;");
			lines.Insert(++index, "\t\t};");

			// 加入上述section的引用
			index = GetLineIndexThatContains(lines, "/* Begin PBXNativeTarget section */");
			var indexBuildPhase = GetLineIndexThatContains(lines, "buildPhases = (", index);
			var indexBuildPhaseEnd = GetLineIndexThatContains(lines, ");", indexBuildPhase);
			lines.Insert(indexBuildPhaseEnd, "\t\t\t\t" + embedFrameworkSectionToken + " /* Embed Frameworks */,");
		}

		File.WriteAllLines(filePath, lines.ToArray());

		// 移到主函数中以规避Unity一个bug导致的错误
//		string projPath = PBXProject.GetPBXProjectPath (path);
//		PBXProject proj = new PBXProject ();
//		proj.ReadFromString (File.ReadAllText (projPath));
//		string target = proj.TargetGuidByName ("Unity-iPhone");
//		proj.AddBuildProperty (target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
//		proj.WriteToFile (projPath);
	}

	static int GetLineIndexThatContains(List<string> lines, string content, int start = 0)
	{
		for (int i = start; i < lines.Count; ++i) {
			if (lines[i].Contains(content)) {
				return i;
			}
		}

		return -1;
	}

	static string GetNextToken(string token)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < token.Length; ++i) {
			if (i == 7) {
				char c = token[i];
				if (c == '9') {
					c = 'A';
				} else if (c == 'Z' || c == 'F') {
					c = '0';
				} else {
					c = (char)(c + 1);
				}
				sb.Append(c);
			} else {
				sb.Append(token[i]);
			}
		}
		return sb.ToString();
	}
	#endif
}
