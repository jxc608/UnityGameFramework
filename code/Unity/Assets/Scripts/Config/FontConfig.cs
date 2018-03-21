using System;
using System.Collections.Generic;
using Snaplingo.Config;
using LitJson;
using UnityEngine;

public class FontConfig : IConfig
{
	public static FontConfig Instance { get { return ConfigUtils.GetConfig<FontConfig>(); } }

	public Dictionary<string, Dictionary<string, float>> m_items = new Dictionary<string, Dictionary<string, float>>();

	public void Fill( string jsonstr )
	{
		var json = JsonMapper.ToObject(jsonstr);
		List<string> keys = new List<string>(json.Keys);
		for (int i = 0; i < keys.Count; i++)
		{
			try
			{
				Dictionary<string, float> items = new Dictionary<string, float>();
				items["next"] = float.Parse(json[keys[i]].TryGetString("next"));
				items["before"] = float.Parse(json[keys[i]].TryGetString("before"));
				m_items[keys[i]] = items;
			}
			catch (Exception e)
			{
				Debug.LogError("Font Config row " + keys[i] + " parse Error! " + e);
			}
		}
	}

	public float GetNextStation(string alph)
	{
		return m_items[alph]["next"];
	}
	public float GetBeforeStation(string alph)
	{
		return m_items[alph]["before"];
	}
}
