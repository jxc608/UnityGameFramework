using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using LitJson;
using StructUtils;

namespace Snaplingo.SaveData
{
	public static class SaveDataUtils
	{
		static string _folder = Application.persistentDataPath + "/savedata/";

		static string GetPath(string fileName)
		{
			return _folder + fileName;
		}

		#region 存档索引

		static Dictionary<string, string> _fileNames = new Dictionary<string, string>();

		static string _saveFile = "";

		static string GetUniqueId()
		{
			return Guid.NewGuid().ToString("N");
		}

		[RuntimeInitializeOnLoadMethod]
		static void Init()
		{
			if (!Directory.Exists(_folder))
				Directory.CreateDirectory(_folder);
			
			var saveKey = ConfigurationController.Instance._SaveDataIndexFileKey;
			if (string.IsNullOrEmpty(saveKey))
				throw new Exception("Error! Save data index file key is null or empty.");
			_saveFile = PlayerPrefs.GetString(saveKey, "");
			if (string.IsNullOrEmpty(_saveFile)) {
				_saveFile = GetUniqueId();
				PlayerPrefs.SetString(saveKey, _saveFile);
			}

			LoadFileNames();
		}

		// 存储索引到文件
		static void SaveFileNames()
		{
			var indexFilePath = GetPath(_saveFile);
			JsonData data = new JsonData();
			foreach (var pair in _fileNames) {
				data[pair.Key] = pair.Value;
			}
			var json = data.ToJson();
			File.WriteAllBytes(indexFilePath, AESUtils.AESEncrypt(json));
		}

		// 从文件读取索引
		static void LoadFileNames ()
		{
			var indexFilePath = GetPath (_saveFile);
			if (File.Exists (indexFilePath)) {
				byte[] bytes = File.ReadAllBytes (indexFilePath);
				if (bytes.Length > 0) {
					var json = AESUtils.AESDecrypt(bytes);
					var data = JsonMapper.ToObject(json);
					foreach (var key in data.Keys) {
						_fileNames.Add(key, data.TryGetString(key));
					}
				}
			}
		}

		static string AddOrChangeFileName(string saveTag)
		{
			if (_fileNames.ContainsKey(saveTag)) {
				if (File.Exists(GetPath(_fileNames[saveTag]))) {
					var fileName = Guid.NewGuid().ToString("N");
					File.Move(GetPath(_fileNames[saveTag]), GetPath(fileName));
					_fileNames[saveTag] = fileName;
				}
			} else {
				var fileName = Guid.NewGuid().ToString("N");
				_fileNames.Add(saveTag, fileName);
			}
			SaveFileNames();
			return _fileNames[saveTag];
		}

		public static void ClearAll()
		{
			var saveKey = ConfigurationController.Instance._SaveDataIndexFileKey;
			_saveFile = PlayerPrefs.GetString(saveKey, "");
			if (string.IsNullOrEmpty(_saveFile)) {
				ClearFilesByIndexFile (GetPath(_saveFile));
			}
		}

		static void ClearFilesByIndexFile(string path)
		{
			if (File.Exists(path)) {
				var json = AESUtils.AESDecrypt(File.ReadAllBytes(path));
				var data = JsonMapper.ToObject(json);
				foreach (var key in data.Keys) {
					File.Delete(GetPath(data.TryGetString(key)));
				}
				File.Delete(path);
			}
		}

		#endregion

		#region 存档

		static List<ISaveData> _saveDatas = new List<ISaveData>();

		// 获取内存中的存档信息，若不存在，则会从文件中读取
		public static T GetSaveData<T>() where T : ISaveData
		{
			return (T)GetSaveData(typeof(T));
		}

		public static ISaveData GetSaveData(Type type)
		{
			for (int i = 0; i < _saveDatas.Count; ++i) {
				if (_saveDatas[i].GetType() == type) {
					return _saveDatas[i];
				}
			}

			return LoadOrCreateSaveData(type);
		}

		// 获取对应类型的存档的序号
		static int IndexOfSaveData<T>()  where T : ISaveData
		{
			return IndexOfSaveData(typeof(T));
		}

		static int IndexOfSaveData(Type type)
		{
			for (int i = 0; i < _saveDatas.Count; ++i) {
				if (_saveDatas[i].GetType() == type) {
					return i;
				}
			}

			return -1;
		}

		// 从内存中删除某个存档
		static void Delete<T>() where T : ISaveData
		{
			Delete(typeof(T));
		}

		static void Delete(Type type)
		{
			for (int i = 0; i < _saveDatas.Count; ++i) {
				if (_saveDatas[i].GetType() == type) {
					_saveDatas.RemoveAt(i);
					break;
				}
			}
		}

		// 将当前内存中的存档数据（如果有的话）存储到文件
		public static void Save<T>() where T : ISaveData
		{
			Save(typeof(T));
		}

		public static void Save(Type type)
		{
			ISaveData instance = null;
			for (int i = 0; i < _saveDatas.Count; ++i) {
				if (_saveDatas[i].GetType() == type) {
					instance = _saveDatas[i];
					break;
				}
			}

			SaveFrom(instance);
		}

		public static void SaveFrom(ISaveData instance)
		{
			if (instance != null) {
				if (!Directory.Exists(_folder))
					Directory.CreateDirectory(_folder);
				var fileName = AddOrChangeFileName(instance.SaveTag());
				File.WriteAllBytes(GetPath(fileName), AESUtils.AESEncrypt(instance.SaveAsJson()));
			}
		}

		// 从文件读取存档到内存，将覆盖原有内存数据（如果有的话）
		// 一般情况下，使用GetSaveData就可以满足需求，然而有时候修改内存数据后需要重置到存档状态，此时使用Load
		public static void Load<T>() where T : ISaveData
		{
			Load(typeof(T));
		}

		public static void Load(Type type)
		{
			for (int i = 0; i < _saveDatas.Count; ++i) {
				if (_saveDatas[i].GetType() == type) {
					LoadTo(_saveDatas[i]);
					return;
				}
			}

			LoadOrCreateSaveData(type);
		}

		// 指定特定存档对象，将其替代原有对象（如果有的话），并从文件读取到这个对象
		public static void LoadTo(ISaveData instance)
		{
			var saveTag = instance.SaveTag();
			if (_fileNames.ContainsKey(saveTag)) {
				var path = GetPath(_fileNames[saveTag]);
				if (File.Exists(path))
					instance.LoadFromJson(AESUtils.AESDecrypt(File.ReadAllBytes(path)));
			}
		}

		// 从文件（如果存在）读取存档数据
		static T LoadOrCreateSaveData<T>() where T : ISaveData
		{
			return (T)LoadOrCreateSaveData(typeof(T));
		}

		static ISaveData LoadOrCreateSaveData(Type type)
		{
			var instance = Activator.CreateInstance(type) as ISaveData;
			var saveTag = instance.SaveTag();
			LoadTo(instance);
			if (!_fileNames.ContainsKey(saveTag)) {
				AddOrChangeFileName(saveTag);
			}
			if (IndexOfSaveData(type) < 0)
				_saveDatas.Add(instance);
			return instance;
		}

		#endregion

	}
}
