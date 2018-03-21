using System;
using System.Collections.Generic;
using UnityEngine;
using Snaplingo.Config;
using LitJson;

public class LevelConfig : IConfig 
{
	public static LevelConfig Instance{ get { return ConfigUtils.GetConfig<LevelConfig>(); } }

	public Dictionary<int, Dictionary<string, string>> m_items = new Dictionary<int, Dictionary<string, string>>();

	public List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();

	public void Fill (string jsonstr)
	{
		var json = JsonMapper.ToObject (jsonstr);
        List<string> keys = new List<string>(json.Keys);
        for (int i = 0; i < keys.Count; i++)
		{
            try
            {
                var value = json[keys[i]];
                int id = int.Parse(value.TryGetString("levelID"));
                Dictionary<string, string> items = new Dictionary<string, string>();
                items["LevelDifficulty"] = value.TryGetString("LevelDifficulty");
                items["LevelIcon"] = value.TryGetString("LevelIcon");
                items["levelCityName"] = value.TryGetString("levelCityName");
                items["levelType"] = value.TryGetString("levelType");
                items["nextLevelID"] = value.TryGetString("nextLevelID");
                items["songID"] = value.TryGetString("songID");
                items["stageID"] = value.TryGetString("stageID");
                items["levelID"] = value.TryGetString("levelID");
				items["energy"] = value.TryGetString("energy");
				items["exp"] = value.TryGetString("exp");
				m_items[id] = items;
                dataList.Add(items);
            }
            catch(Exception e)
            {
                Debug.LogError("Level Config row " + i + " parse Error! " + e);
            }
		}
    }

	public int GetsongIDBylevelID (int id)
	{
		int songID = int.Parse(m_items[id]["songID"]);
		return songID;
	}

	public string GetLevelDifficultyByLevelID (int id)
	{
		string levelDifficulty = m_items[id]["LevelDifficulty"];
		return levelDifficulty;
	}

	public int GetLevelEnergyByLevelID(int id)
	{
		int energy = int.Parse(m_items[id]["energy"]);
		return energy;
	}

	public int GetLevelExpBaseByLevelID(int id)
	{
		int exp = int.Parse(m_items[id]["exp"]);
		return exp;
	}

	public List<Dictionary<string, string>> GetListLevelDataByDifficultyAndStage(string difficulty)
	{
		List<Dictionary<string, string>> returnData = new List<Dictionary<string, string>>();

		foreach (var item in dataList) {
			if (item["LevelDifficulty"] == difficulty) {
				returnData.Add(item);
			}
		}
		return returnData;

	}
}
