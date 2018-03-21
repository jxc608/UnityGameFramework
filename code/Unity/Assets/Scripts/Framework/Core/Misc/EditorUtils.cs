#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Text;

public class EditorUtils
{
	public static string GetRelativePathByAbsolutePath(string absolutePath)
	{
		return absolutePath.Replace(Path.GetDirectoryName(Application.dataPath) + "/", "");
	}

	public static void CreateAsset<T>(string path) where T : ScriptableObject
	{
		var folder = Path.GetDirectoryName(Application.dataPath + "/Resources/" + path);
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
		T ac = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(ac, "Assets/Resources/" + path + ".asset");
	}

	public static void ProcessCommand(string scriptName, string[] parameters, bool supportWin = false)
	{
		StringBuilder sb = new StringBuilder();
		foreach (var param in parameters) {
			if (sb.Length > 0)
				sb.Append(" ");
			sb.Append(param);
		}
		#if UNITY_EDITOR_OSX
		var command = scriptName + ".sh " + sb.ToString();
		UnityEngine.Debug.Log("Execute command: " + command);
		EditorUtils.ProcessCommand("/bin/sh", command);
		#elif UNITY_EDITOR_WIN
		if (supportWin)
			EditorUtils.ProcessCommand (scriptName + ".bat", sb.ToString ());
		#endif
	}

	public static void ProcessCommand(string command, string argument)
	{
		ProcessStartInfo start = new ProcessStartInfo(command);
		start.Arguments = argument;
		start.CreateNoWindow = false;
		start.ErrorDialog = true;
		start.UseShellExecute = true;
		if (start.UseShellExecute) {
			start.RedirectStandardOutput = false;
			start.RedirectStandardError = false;
			start.RedirectStandardInput = false;
		} else {
			start.RedirectStandardOutput = true;
			start.RedirectStandardError = true;
			start.RedirectStandardInput = true;
			start.StandardOutputEncoding = UTF8Encoding.UTF8;
			start.StandardErrorEncoding = UTF8Encoding.UTF8;
		}
		Process p = Process.Start(start);
		if (!start.UseShellExecute) {
			UnityEngine.Debug.Log(p.StandardOutput);
			UnityEngine.Debug.Log(p.StandardError);
		}
		p.WaitForExit();
		p.Close();
	}

	public static void ShowListInEditor<T>(ref List<T> list, string label)
	{
		int newCount = Mathf.Max(0, EditorGUILayout.IntField(label, list.Count));
		while (newCount < list.Count)
			list.RemoveAt(list.Count - 1);
		while (newCount > list.Count)
			list.Add(default(T));


		for (int i = 0; i < list.Count; i++) {
			T ele = list[i];
			object newEle = null;
			string indexLabel = "  " + i.ToString() + "=>";
			if (ele is int) {
				newEle = EditorGUILayout.IntField(indexLabel, Convert.ToInt32(ele), 
				                                   GUILayout.Width(100), GUILayout.ExpandWidth(true));
			} else if (ele is float) {
					newEle = EditorGUILayout.FloatField(indexLabel, (float)Convert.ToDouble(ele), 
					                                    GUILayout.Width(100), GUILayout.ExpandWidth(true));
				} else if (typeof(T) == typeof(GameObject)) {
						UnityEngine.Object obj = null;
						if (ele != null)
							obj = (GameObject)Convert.ChangeType(ele, typeof(GameObject));
						newEle = EditorGUILayout.ObjectField(indexLabel, obj, 
						                                    typeof(T), false, GUILayout.Width(100), GUILayout.ExpandWidth(true));
					}
			list[i] = (T)Convert.ChangeType(newEle, typeof(T));
		}
	}

	public static string[] GetSceneArrayByEditorScenes()
	{
		List<string> scenes = new List<string>();
		foreach (var scene in EditorBuildSettings.scenes) {
			if (scene.enabled) {
				scenes.Add(scene.path);
			}
		}
		return scenes.ToArray();
	}
}

#endif

