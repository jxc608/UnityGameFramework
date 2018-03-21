using System;
using System.Collections.Generic;
using UnityEngine;
using Snaplingo.Config;
using LitJson;

public class StageConfig : IConfig 
{
	public static StageConfig Instance{ get { return ConfigUtils.GetConfig<StageConfig>(); } }

	public Dictionary<int, Dictionary<string, string>> m_items = new Dictionary<int, Dictionary<string, string>>();

    public void Fill (string jsonstr)
	{
		var json = JsonMapper.ToObject (jsonstr);
        List<string> keys = new List<string>(json.Keys);
        for (int i = 0; i < keys.Count; i++)
		{
            try
            {
                var value = json[keys[i]];
                int id = int.Parse(value.TryGetString("stageID"));
                Dictionary<string, string> items = new Dictionary<string, string>();
                items["nextStageID"] = value.TryGetString("nextStageID");
                items["stageResource"] = value.TryGetString("stageResource");
                items["stageTargetPosition"] = value.TryGetString("stageTargetPosition");
                m_items[id] = items;
            }
            catch (Exception e)
            {
                Debug.LogError("Stage Config row " + i + " parse Error! " + e);
            }
		}
    }
}