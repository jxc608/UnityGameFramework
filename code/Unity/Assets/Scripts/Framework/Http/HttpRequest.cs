using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class HttpRequest : Manager
{
    public enum HttpReqType {PUT, POST, GET}
    public static HttpRequest Instance { get { return GetManager<HttpRequest>(); } }
	private string HostName = ConfigurationController.Instance.HttpHostList[DebugConfigController.Instance.HttpHostIndex];
	private string Token = ConfigurationController.Instance.HttpToken;
	private string Https;

	protected override void Init()
	{
		base.Init();
		Https = "https://" + HostName + "/";
	}

    public IEnumerator WebRequest(HttpReqType type, string url, byte[] data = null, Action<string> successCallback = null, Action failCallback = null)
    {
        UnityWebRequest www = null;
        switch(type)
        {
            case HttpReqType.GET:
                www = UnityWebRequest.Get(Https + url);
                break;
            case HttpReqType.PUT:
                www = UnityWebRequest.Put(Https + url, data);
                break;
            case HttpReqType.POST:
                if(data != null)
                {
                    www = UnityWebRequest.Put(Https + url, data);
                }
                else
                {
                    JsonData json = new JsonData();
                    json["AppName"] = "MusicGame";
                    byte[] Data = System.Text.Encoding.UTF8.GetBytes(json.ToJson());
                    www = UnityWebRequest.Put(Https + url, Data);
                }
                www.method = UnityWebRequest.kHttpVerbPOST;
                break;
        }

        www.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        www.SetRequestHeader("Token", Token);
        yield return www.Send();

        bool exceptionOccur = false;
        try
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                LogManager.Log(www.error);
            }
            else
            {
                if (successCallback != null)
                {
                    successCallback.Invoke(www.downloadHandler.text);
                }
            }
        }
        catch (Exception e)
        {
            LogManager.LogError(e.StackTrace);
            LogManager.LogError(e.Message + "\n webRequest faild: " + url);
            exceptionOccur = true;
        }
        finally
        {
            if (failCallback != null && exceptionOccur)
                failCallback.Invoke();
        }
    }
}