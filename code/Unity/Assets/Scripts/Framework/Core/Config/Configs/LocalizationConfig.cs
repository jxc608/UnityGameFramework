using System.Collections.Generic;
using LitJson;
using Snaplingo.Config;

public enum Language
{
	English,
	Chinese,

	LanguageCount,
}

public class LocalizationConfig : IConfig
{
	public static LocalizationConfig Instance { get { return ConfigUtils.GetConfig<LocalizationConfig>(); } }

	public struct LanguageItem
	{
		public string[] _texts;

		public LanguageItem(params string[] texts)
		{
			_texts = texts;
		}
	}

	public Dictionary<int, LanguageItem> _items = new Dictionary<int, LanguageItem>();

	public void Fill(string jsonstr)
	{
		var json = JsonMapper.ToObject(jsonstr);
		foreach (var k in json.Keys) {
			var value = json[k];

			int id = int.Parse(value.TryGetString("id"));
			string[] texts = new string[(int)Language.LanguageCount];
			for (Language lang = 0; lang < Language.LanguageCount; ++lang) {
				texts[(int)lang] = value.TryGetString(lang.ToString()).Replace("\\n", "\n");
			}
			LanguageItem item = new LanguageItem(texts);
			if (_items.ContainsKey(id)) {
				LogManager.LogWarning("Duplicated localization id: " + id);
			}
			_items[id] = item;
		}
	}

	Language _currentLanguage = ConfigurationController.Instance._DefaultLanguage;

	public Language CurrentLanguage {
		get {
			return _currentLanguage;
		}
		set {
			_currentLanguage = value;
		}
	}

	public string GetStringById(int id, params object[] args)
	{
		if (_items.Count == 0)
			return "";
		if (!_items.ContainsKey(id)) {
			UnityEngine.Debug.LogError("Localization id not exist: " + id);
			return "";
		}
		if (_items[id]._texts.Length <= (int)_currentLanguage) {
			UnityEngine.Debug.LogError("Unsupported language: " + _currentLanguage.ToString());
			return "";
		}

		if (args.Length > 0)
			return string.Format(_items[id]._texts[(int)_currentLanguage], args);
		else
			return _items[id]._texts[(int)_currentLanguage];
	}
}
