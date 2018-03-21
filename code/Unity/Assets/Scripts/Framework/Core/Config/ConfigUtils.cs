using UnityEngine;
using System.Collections;
using StructUtils;
using System;
using System.Collections.Generic;

namespace Snaplingo.Config
{
	public static class ConfigUtils
	{
		// 通过路径读取表格内容
		public static List<TextAsset> ReadContent(string path)
		{
			return ResourceLoadUtils.LoadAll<TextAsset>("Config/" + path);
		}

		// 表格对象、清除标签
		static List<Tuple<IConfig, bool>> _configs = new List<Tuple<IConfig, bool>>();

		// 从内存中获取表格
		public static T GetConfig<T>() where T : IConfig
		{
			if (!HasConfig<T>()) {
				LoadConfig<T>();
			}

			for (int i = 0; i < _configs.Count; ++i) {
				if (_configs[i]._item1 is T) {
					return (T)_configs[i]._item1;
				}
			}

			return default (T);
		}

		static bool HasConfig<T>() where T : IConfig
		{
			for (int i = 0; i < _configs.Count; ++i) {
				if (_configs[i]._item1 is T) {
					return true;
				}
			}

			return false;
		}

		// 载入一系列表格
		public static void LoadConfigs(Type[] types)
		{
			LoadConfigs(types, ConfigurationController.Instance._DestroyUnusedConfigs);
		}

		// 载入一系列表格，destroyOther决定是否删除无用的表格
		public static void LoadConfigs(Type[] types, bool destroyOther)
		{
			InitConfigDelTag();
			for (int i = 0; i < types.Length; ++i)
				LoadConfig(types[i]);
			DeleteConfigByTag(destroyOther);
		}

		// 载入一个表格
		public static void LoadConfig(Type type)
		{
			for (int i = 0; i < _configs.Count; ++i) {
				var config = _configs[i];
				if (type == config._item1.GetType()) {
					config._item2 = true;
					return;
				}
			}

			try {
				IConfig instance = Activator.CreateInstance(type) as IConfig;
				if (instance != null) {
					var typeName = instance.GetType().ToString();
					typeName = typeName.Substring(typeName.LastIndexOf(".") + 1);
					foreach (var asset in ConfigUtils.ReadContent (typeName))
						instance.Fill(asset.text);
					_configs.Add(new Tuple<IConfig, bool>(instance, true));
				} else
					Debug.LogError("Pass a non-IConfig type to register(Type)");
			} catch (Exception e) {
				Debug.LogError(e.Message + "\n" + e.StackTrace);
			}
		}

		// 载入一个表格
		public static void LoadConfig<T>() where T : IConfig
		{
			LoadConfig(typeof(T));
		}

		// 卸载一个表格
		public static void UnloadConfig(Type type)
		{
			List<Tuple<IConfig, bool>> _removeConfigs = new List<Tuple<IConfig, bool>>();
			for (int i = 0; i < _configs.Count; ++i) {
				var config = _configs[i];
				if (type == config._item1.GetType()) {
					_removeConfigs.Add(config);
				}
			}

			for (int i = 0; i < _removeConfigs.Count; ++i) {
				_configs.Remove(_removeConfigs[i]);
			}
		}

		// 卸载一个表格
		public static void UnloadConfig<T>() where T : IConfig
		{
			UnloadConfig(typeof(T));
		}

		// 初始化表格清除标签
		static void InitConfigDelTag()
		{
			for (int i = 0; i < _configs.Count; ++i) {
				_configs[i]._item2 = false;
			}
		}

		// 根据表格清除标签清除无用的表格
		static void DeleteConfigByTag(bool destroyOther)
		{
			if (destroyOther) {
				List<Tuple<IConfig, bool>> _invalidConfigs = new List<Tuple<IConfig, bool>>();
				for (int i = 0; i < _configs.Count; ++i) {
					var config = _configs[i];
					if (!config._item2) {
						_invalidConfigs.Add(config);
					}
				}

				for (int i = 0; i < _invalidConfigs.Count; ++i) {
					_configs.Remove(_invalidConfigs[i]);
				}
			}
		}
	}
}
