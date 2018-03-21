using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Snaplingo.Config;
using LitJson;
using System;

public class ExpConfig : IConfig
{
	public static ExpConfig Instance { get { return ConfigUtils.GetConfig<ExpConfig>(); } }


	public List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();

	public void Fill(string jsonstr)
	{
		var json = JsonMapper.ToObject(jsonstr);
        List<string> keys = new List<string>(json.Keys);
        for (int i = 0; i < keys.Count; i++)
		{
            try
            {
                var value = json[keys[i]];
                Dictionary<string, string> items = new Dictionary<string, string>();
                items["level"] = value.TryGetString("level");
                items["exp"] = value.TryGetString("exp");
                items["energy"] = value.TryGetString("energy");
                dataList.Add(items);
            }
            catch (Exception e)
            {
                Debug.LogError("Exp Config row " + i + " parse Error! " + e);
            }
		}
		Comparison<Dictionary<string, string>> comparison = new Comparison<Dictionary<string, string>>
			((Dictionary<string, string> item1, Dictionary<string, string> item2) => {
				if (float.Parse(item1["level"]) < float.Parse(item2["level"]))
					return -1;
				else
					return 1;
			});
		dataList.Sort(comparison);

	}
	private void ArraySort(Dictionary<string, string> item1, Dictionary<string, string> item2)
	{

	}

	//获取对应等级的全部信息
	public Dictionary<string, string> GetItemlevelDataByLevel(float level)
	{
		foreach (var item in dataList)
		{
			if (float.Parse(item["level"]) == level)
			{
				return item;
			}
		}

		return null;
	}

	//根据经验值获取所在的等级和下一级所需经验值
	public List<float> GetCurrentLevelByExperience(float exp)
	{
		List<float> returnData = new List<float>();
		for (int i = 0; i < dataList.Count; i++)
		{
			//未达到最高级别
			if (exp < float.Parse(dataList[i]["exp"]))
			{
				//0级别
				if (i <= 0)
				{
					returnData.Add(float.Parse("0"));
					returnData.Add(float.Parse("0"));
					returnData.Add(float.Parse(dataList[i]["exp"]));
				}
				//最高级别
				else if (i >= dataList.Count - 1)
				{
					returnData.Add(float.Parse(dataList[i - 1]["level"]));
					returnData.Add(float.Parse(dataList[i - 1]["exp"]));
					returnData.Add(float.Parse(dataList[i]["exp"]));
				}
				//中间级别
				else
				{
					returnData.Add(float.Parse(dataList[i - 1]["level"]));
					returnData.Add(float.Parse(dataList[i - 1]["exp"]));
					returnData.Add(float.Parse(dataList[i]["exp"]));
				}
				return returnData;
			}
			//达到最高级别
			else if (exp >= float.Parse(dataList[dataList.Count - 1]["exp"]))
			{
				Debug.Log("超过最高级别~~" + float.Parse(dataList[dataList.Count - 1]["level"]));

				returnData.Add(float.Parse(dataList[dataList.Count - 1]["level"]));
				returnData.Add(float.Parse(dataList[dataList.Count - 1]["exp"]));
				returnData.Add(-1);
				return returnData;
			}
		}
		return null;
	}
}
