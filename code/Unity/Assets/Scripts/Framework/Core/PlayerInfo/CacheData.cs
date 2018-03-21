using LitJson;
using Snaplingo.SaveData;
using System.Collections.Generic;

public class CacheData : ISaveData
{
	public static CacheData Instance { get { return SaveDataUtils.GetSaveData<CacheData>(); } }

	Dictionary<int, int> m_LevelInfo = new Dictionary<int, int>();
	public Dictionary<int, int> LevelInfo { get { return m_LevelInfo; } set { m_LevelInfo = value; } }

	public void LoadFromJson(string json)
	{
		JsonData data = JsonMapper.ToObject(json);
		List<string> keys = new List<string>(data.Keys);
		foreach (string key in keys)
		{
			JsonData levelinfo = data[key];
			int level = int.Parse(key);
			int info = int.Parse(levelinfo.ToString());
			LevelInfo[level] = info;
		}
	}

	public string SaveAsJson()
	{
		JsonData data = new JsonData();
		List<int> keys = new List<int>(LevelInfo.Keys);
		foreach (int key in keys)
		{
			JsonData temp = new JsonData();
			temp = LevelInfo[key].ToString();
			data[key.ToString()] = temp;
		}
		return data.ToJson();
	}

	public string SaveTag()
	{
		return "CacheData";
	}


	public void AddData(int level)
	{
		if( !LevelInfo.ContainsKey(level))
			LevelInfo[level] = 1;
	}

	public void DeleteData( int level )
	{
		if (LevelInfo.ContainsKey(level))
			LevelInfo.Remove(level);
	}

	public List<int> GetCacheDataLevelID()
	{
		if (LevelInfo.Count == 0)
			return null;
		else
			return new List<int>(LevelInfo.Keys);

	}

	public bool ExitCacheData(int levelId)
	{
		if (LevelInfo.ContainsKey(levelId))
			return true;
		else
			return false;
	}
}
