using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class EditorFindReferences
{
	const string _menuName = "Assets/Find References In Project %e";

	[MenuItem(_menuName, false, 25)]
	static private void Find()
	{
		EditorSettings.serializationMode = SerializationMode.ForceText;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (!string.IsNullOrEmpty(path)) {
			string guid = AssetDatabase.AssetPathToGUID(path);
			Debug.Log("Current guid: " + guid);
			var withoutExtensions = new List<string>(){ ".prefab", ".unity", ".mat", ".asset" };
			string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
				.Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
			int startIndex = 0;

			EditorApplication.update = delegate() {
				string file = files[startIndex];

				bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

				if (Regex.IsMatch(File.ReadAllText(file), guid)) {
					Debug.Log(GetRelativeAssetsPath(file), AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
				}

				startIndex++;
				if (isCancel || startIndex >= files.Length) {
					EditorUtility.ClearProgressBar();
					EditorApplication.update = null;
					startIndex = 0;
				}
			};
		}
	}

	[MenuItem(_menuName, true)]
	static private bool VFind()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		return (!string.IsNullOrEmpty(path));
	}

	static private string GetRelativeAssetsPath(string path)
	{
		return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
	}
}