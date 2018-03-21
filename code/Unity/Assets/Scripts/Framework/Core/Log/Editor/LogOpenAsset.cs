using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Reflection;
using System.Text.RegularExpressions;

public class LogOpenAsset
{
	[OnOpenAssetAttribute(0)]
	public static bool OnOpenAsset(int instanceID, int line)
	{
		string stackTrace = GetConsoleActiveText();
		if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains("LogManager:Log")) {
			Match matches = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
			string pathLine = "";
			while (matches.Success) {
				pathLine = matches.Groups[1].Value;
				if (!pathLine.Contains("LogManager.cs")) {
					int splitIndex = pathLine.LastIndexOf(":");
					string path = pathLine.Substring(0, splitIndex);
					line = int.Parse(pathLine.Substring(splitIndex + 1));
					string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + path;
					UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath, line);
					break;
				}
				matches = matches.NextMatch();
			}
			return true;
		}
		return false;
	}

	static string GetConsoleActiveText()
	{
		var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
		var consoleWindwoInstance = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
		if (consoleWindwoInstance != null) {
			if ((object)EditorWindow.focusedWindow == consoleWindwoInstance) {
				return consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(consoleWindwoInstance).ToString();
			}
		}

		return "";
	}

}