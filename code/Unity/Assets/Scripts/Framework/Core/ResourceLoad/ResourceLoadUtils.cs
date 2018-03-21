using System;
using UnityEngine;
using System.Collections.Generic;

public interface IResourceLoad
{
	string GetResourceFolder();

	T Load<T>(string path) where T : UnityEngine.Object;
}

[AttributeUsage(AttributeTargets.Class)]
public class ResourceLoadAttribute : Attribute
{
	public int _priority { get; private set; }

	public ResourceLoadAttribute(int priority)
	{
		_priority = priority;
	}
}

[ResourceLoadAttribute(1)]
public class ResourceLoad : IResourceLoad
{
	public string GetResourceFolder()
	{
		return Application.dataPath.Replace("\\", "/") + "/Resources/";
	}

	public T Load<T>(string path) where T : UnityEngine.Object
	{
		return Resources.Load<T>(path);
	}
}

[ResourceLoadAttribute(2)]
public class AssetDatabaseLoad : IResourceLoad
{
	public string GetResourceFolder()
	{
		return Application.dataPath.Replace("\\", "/") + "/AssetBundle/";
	}

	public T Load<T>(string path) where T : UnityEngine.Object
	{
		#if UNITY_EDITOR
		if (!DebugConfigController.Instance.TestBundleInEditor || !Application.isPlaying) {
			path = "Assets/AssetBundle/" + path;
			List<string> suffices = new List<string>();
			if (typeof(T) == typeof(Sprite)) {
				suffices.Add(".png");
				suffices.Add(".jpg");
			} else if (typeof(T) == typeof(Texture2D)) {
				suffices.Add(".png");
				suffices.Add(".jpg");
			} else if (typeof(T) == typeof(GameObject)) {
				suffices.Add(".prefab");
			} else if (typeof(T) == typeof(AudioClip)) {
				suffices.Add(".mp3");
				suffices.Add(".wav");
			} else if (typeof(T) == typeof(Material)) {
				suffices.Add(".mat");
			} else if (typeof(T) == typeof(TextAsset)) {
				suffices.Add(".txt");
				suffices.Add(".json");
				suffices.Add(".xml");        
			} else if (MiscUtils.IsSubclassOf(typeof(T), typeof(ScriptableObject))) {
				suffices.Add(".asset");
			}
			for (int i = 0; i < suffices.Count; ++i) {
				var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path + suffices[i]);
				if (obj != null)
					return obj;
			}
		}
		#endif
		return null;
	}
}

public class ResourceLoadUtils
{
	static List<IResourceLoad> _instances = new List<IResourceLoad>();
	static List<string> _resourceFolders = new List<string>();

	static bool _hasInit = false;

	static void CheckInit()
	{
		if (_hasInit)
			return;

		_hasInit = true;
		var types = MiscUtils.GetTypesHaveInterface(typeof(IResourceLoad));
		foreach (var type in types) {
			if (MiscUtils.IsSubclassOf(type, typeof(Manager))) {
				if (Application.isPlaying)
					AddInstance(Manager.GetManager(type) as IResourceLoad);
			} else {
				AddInstance(Activator.CreateInstance(type) as IResourceLoad);
			}
		}

		_instances.Sort(delegate(IResourceLoad x, IResourceLoad y) {
			var attrX = Attribute.GetCustomAttribute(x.GetType(), typeof(ResourceLoadAttribute), true) as ResourceLoadAttribute;
			var attrY = Attribute.GetCustomAttribute(y.GetType(), typeof(ResourceLoadAttribute), true) as ResourceLoadAttribute;
			return attrX._priority.CompareTo(attrY._priority);
		});
	}

	static void AddInstance(IResourceLoad inst)
	{
		_instances.Add(inst);
		if (!(inst is Manager))
			_resourceFolders.Add(inst.GetResourceFolder());
	}

	static string[] _subfolders = new string[] {
		"",
		"Framework/Core",
		"Framework/Debug",
	};

	public static string[] GetResourceFolders()
	{
		return _resourceFolders.ToArray();
	}

	public static string[] GetSubfolders()
	{
		return _subfolders;
	}

	public static string[] GetSubfolderFullPaths()
	{
		CheckInit();
		
		string[] resFolders = new string[_resourceFolders.Count * _subfolders.Length];
		for (int i = 0; i < _resourceFolders.Count; ++i) {
			for (int j = 0; j < _subfolders.Length; ++j) {
				resFolders[j + i * _subfolders.Length] = System.IO.Path.Combine(_resourceFolders[i], _subfolders[j]);
			}
		}
		return resFolders;
	}

	#region load functions

	static T LoadInternal<T>(string path) where T : UnityEngine.Object
	{
		return LoadInternal<T, IResourceLoad>(path);
	}

	public static TRes LoadInternal<TRes, TResLoad>(string path) where TRes : UnityEngine.Object where TResLoad : IResourceLoad
	{
		CheckInit();

		for (int i = 0; i < _instances.Count; ++i) {
			if (_instances[i] is TResLoad) {
				var obj = _instances[i].Load<TRes>(path);
				if (obj != null)
					return obj;
			}
		}

		return null;
	}

	public static T Load<T>(string path, bool checkAllFolders = false) where T : UnityEngine.Object
	{
		return Load<T, IResourceLoad>(path, checkAllFolders);
	}

	public static TRes Load<TRes, TResLoad>(string path, bool checkAllSubfolders = false) where TRes : UnityEngine.Object where TResLoad : IResourceLoad
	{
		string[] folders = null;
		if (checkAllSubfolders) {
			folders = _subfolders;
		} else
			folders = new string[] { "" };
		for (int i = 0; i < folders.Length; ++i) {
			TRes inst = LoadInternal<TRes, TResLoad>(System.IO.Path.Combine(folders[i], path));
			if (inst != null) {
				return inst;
			}
		}
		return null;
	}

	public static List<T> LoadAll<T>(string path) where T : UnityEngine.Object
	{
		List<T> assets = new List<T>();
		for (int i = 0; i < _subfolders.Length; ++i) {
			T asset = LoadInternal<T>(System.IO.Path.Combine(_subfolders[i], path));
			if (asset != null) {
				assets.Add(asset);
			}
		}
		return assets;
	}

	#endregion

}
