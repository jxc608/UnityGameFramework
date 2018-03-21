using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;

public class AutoTestManager : MonoBehaviour 
{
    private float m_Timer;
    private bool m_Start;
    private AutoAction m_AutoAction;
    private string m_ScriptName;

    void Awake()
    {
        DontDestroyOnLoad(this);
        m_Start = false;
    }

    public void StartAutoTestScript(string scriptName)
    {
        m_ScriptName = scriptName;
        string json = ResourceLoadUtils.Load<TextAsset>("AutoText/" + scriptName).text;
        JsonData data = JsonMapper.ToObject(json);
        m_AutoAction = AutoActionJsonParser.Parse(data);
        if(m_AutoAction != null)
        {
            m_AutoAction.SetFinishCallback(TestSuccess);
            Application.logMessageReceived += GetLog;
            m_Start = true;
        }
        else
        {
            TestFailed("script parse error!! " + scriptName);
        }
    }


    private const float OverTime = 10f;
    private void Update()
    {
        if(m_Start)
        {
            m_Timer += Time.deltaTime;
            if (m_Timer > OverTime)
            {
                m_Start = false;
            }

            m_AutoAction.Run();
        }
    }

    private void GetLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            string url = "api/game_app/v1/test_logs";

            LogManager.LogError("Auto test got error!!");
            LocationInfo li = new LocationInfo();
            JsonData json = new JsonData();
            json["uuid"] = PlayerInfo.Instance.Uuid;
            json["level"] = type.ToString();
            json["missionID"] = m_ScriptName;
            json["log"] = logString + " \n" + stackTrace;
            json["timestamp"] = li.timestamp;

            byte[] Data = System.Text.Encoding.UTF8.GetBytes(json.ToJson());
            StaticMonoBehaviour.Instance.StartCoroutine(HttpRequest.Instance.WebRequest(HttpRequest.HttpReqType.POST, url, Data, null, null));
        }
    }

    private void TestFailed(string reason)
    {
        m_Start = false;
        Application.logMessageReceived -= GetLog;
        LogManager.LogError(reason);
    }

    private void TestSuccess()
    {
        m_Start = false;
        Application.logMessageReceived -= GetLog;
        LogManager.Log("Test succeed!!");
    }

    private void ActionCompleteCallback()
    {
        m_Timer = 0;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= GetLog;
    }
}
