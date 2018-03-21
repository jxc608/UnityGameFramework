using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Snaplingo.Config;
using LitJson;
using System;

public class SongConfig : IConfig 
{
	public static SongConfig Instance{ get { return ConfigUtils.GetConfig<SongConfig>(); } }

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
                int id = int.Parse(value.TryGetString("songID"));
                Dictionary<string, string> items = new Dictionary<string, string>();
                items["songAuthorIconUrl"] = value.TryGetString("songAuthorIconUrl");
                items["songAuthorName"] = value.TryGetString("songAuthorName");
                items["songDifficultyStar"] = value.TryGetString("songDifficultyStar");
                items["songName"] = value.TryGetString("songName");
                items["songScript"] = value.TryGetString("songScript");
                items["offset"] = value.TryGetString("offset");
                items["teachSceneBG"] = value.TryGetString("teachSceneBG");
				items["educationText"] = value.TryGetString("educationText");
                m_items[id] = items;
            }
            catch (Exception e)
            {
                Debug.LogError("Song Config row " + i + " parse Error! " + e);
            }
        }
    }

	public Dictionary<string, string> GetsongInfoBysongID (int id)
	{
		Dictionary<string, string> songInfo = new Dictionary<string, string> ();
		songInfo["songAuthorIconUrl"] = m_items[id]["songAuthorIconUrl"];
		songInfo["songAuthorName"] = m_items[id]["songAuthorName"];
		songInfo["songDifficultyStar"] = m_items[id]["songDifficultyStar"];
		songInfo["songName"] = m_items[id]["songName"];
		songInfo["songScript"] = m_items[id]["songScript"];
        songInfo["offset"] = m_items[id]["offset"];
        songInfo["teachSceneBG"] = m_items[id]["teachSceneBG"];
		songInfo["educationText"] = m_items[id]["educationText"];
		return songInfo;
	}

	public string GetsongScriptBySongIDAndLevelDiffculty (int songID, int LevelDifficulty)
	{
		string songScriptAll = m_items[songID]["songScript"];
		string[] script = songScriptAll.Split('|');
		return script[LevelDifficulty-1];
	}

    public int GetSongOffsetBySongIDAndLevelDiffculty(int songID, int LevelDifficulty)
    {
        string offset = m_items[songID]["offset"];
        string[] script = offset.Split('|');
        if (script.Length != 2)
        {
            Debug.LogError("songConfig offset error! songID:" + songID);
            return 0;
        }
        else
        {
            int result = 0;
            try
            {
                result = int.Parse(script[LevelDifficulty - 1]);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n songConfig offset parse Error! songID" + songID);
                return 0;
            }
            return result;
        }
    }
}