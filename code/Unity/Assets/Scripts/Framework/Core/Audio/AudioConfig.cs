using System.Collections.Generic;
using Snaplingo.Config;
using UnityEngine;
using LitJson;
using StructUtils;

public class AudioConfig : IConfig
{
	public static AudioConfig Instance { get { return ConfigUtils.GetConfig<AudioConfig>(); } }

	public OrderedDictionary<int, Tuple<string, string>> _items = new OrderedDictionary<int, Tuple<string, string>>();

	public void Fill(string jsonstr)
	{
		var json = JsonMapper.ToObject(jsonstr);
		foreach (var k in json.Keys) {
			var value = json[k];

			int id = int.Parse(value.TryGetString("id"));
			string key = value.TryGetString("enum");
			string name = value.TryGetString("name");
			var item = new Tuple<string, string>(key, name);
			if (_items.ContainsKey(id)) {
				LogManager.LogWarning("Duplicated audio id: " + id);
			}
			_items[id] = item;
		}

		_items.Sort(delegate(Tuple<int, Tuple<string, string>> x, Tuple<int, Tuple<string, string>> y) {
			return x._item1.CompareTo(y._item1);
		});
	}

	public string GetNameById(int id)
	{
		if (_items.ContainsKey(id))
			return _items[id]._item2;
		else
			throw new System.Exception("Wrong id in audio conifg: " + id);
	}

	public string GetNameByKey(string key)
	{
		for (int i = 0; i < _items.Count; ++i) {
			var p = _items.PairAt(i);
			if (p._item2._item1.Equals(key))
				return p._item2._item2;
		}

		throw new System.Exception("Wrong key in audio conifg: " + key);
	}

	public int GetIdByKey(string key)
	{
		for (int i = 0; i < _items.Count; ++i) {
			var p = _items.PairAt(i);
			if (p._item2._item1.Equals(key)) {
				if (!string.IsNullOrEmpty(p._item2._item2))
					return p._item1;
				else
					return 0;
			}
		}

		throw new System.Exception("Wrong key in audio conifg: " + key);
	}

	public string GetKeyById(int id)
	{
		if (id > 0) {
			for (int i = 0; i < _items.Count; ++i) {
				var p = _items.PairAt(i);
				if (p._item1 == id)
					return p._item2._item1;
			}
		}

		throw new System.Exception("Wrong id in audio conifg: " + id);
	}

	public string[] GetAllKeys()
	{
		List<string> keys = new List<string>();
		foreach (var v in _items.Values) {
			keys.Add(v._item1);
		}
		return keys.ToArray();
	}

	public int GetIndexById(int id)
	{
		var keys = GetAllKeys();
		var key = GetKeyById(id);
		if (!string.IsNullOrEmpty(key)) {
			for (int i = 0; i < keys.Length; ++i) {
				if (keys[i].Equals(key)) {
					return i;
				}
			}
			return 0;
		} else
			throw new System.Exception("Wrong id in audio conifg: " + id);
	}

}
