using LitJson;
using Snaplingo.SaveData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Snaplingo.Config;

public class PlayerInfo : ISaveData
{
	public static PlayerInfo Instance {
		get { return SaveDataUtils.GetSaveData<PlayerInfo>(); }
	}

	string _uuid = "";

	public string Uuid { get { return _uuid; } set { _uuid = value; } }

	string _nickname = "";

	public string Nickname {
		get {
			if (string.IsNullOrEmpty(_nickname)) {
				_nickname = "呤呤小伙伴";
			}
			return _nickname;
		}
		set { _nickname = value; }
	}

	string _server = "";

	public string Server { get { return _server; } set { _server = value; } }

	string _sessionId = "";

	public string SessionId { get { return _sessionId; } set { _sessionId = value; } }

	string _username = "";

	public string Username { get { return _username; } set { _username = value; } }

	public string SaveTag()
	{
		return "PlayerInfo";
	}

	public string SaveAsJson()
	{
		JsonData data = new JsonData();
		data["uuid"] = string.IsNullOrEmpty(Uuid) ? "" : Uuid;
		data["nickname"] = string.IsNullOrEmpty(Nickname) ? "" : Nickname;
	
		return data.ToJson();
	}

	public void LoadFromJson(string json)
	{
		JsonData data = JsonMapper.ToObject(json);
		Uuid = data.TryGetString("uuid");
		Nickname = data.TryGetString("nickname");
		
	}

	public void Refresh(string uuid, string username, string nickname, string server, int level=0, int energy=0, int experience=0)
	{
		bool save = false;
		if (!string.IsNullOrEmpty(uuid)) {
			Uuid = uuid;
			save = true;
		}
		if (!string.IsNullOrEmpty(nickname)) {
			Nickname = nickname;
			save = true;
		}

		if (save)
			SaveDataUtils.Save<PlayerInfo>();
	}

	public void save()
	{
		SaveDataUtils.Save<PlayerInfo>();
	}

}

