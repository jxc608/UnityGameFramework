using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public abstract class AutoAction
{
    protected Action m_Finish;

    public abstract void Run();

    public virtual void SetFinishCallback(Action finish)
    {
        m_Finish += finish;
    }
}


public class AutoWaitScene : AutoAction
{
    private string m_SceneName;
    public string SceneName
    {
        get { return m_SceneName; }
    }

    public AutoWaitScene(string sceneName)
    {
        m_SceneName = sceneName;
    }

    public override void Run()
    {
        if (string.Equals(SceneManager.GetActiveScene().name, m_SceneName))
        {
            m_Finish.Invoke();
        }
    }
}

public class AutoWaitObjectAppear : AutoAction
{
    private string m_ObjName;
    public string ObjName
    {
        get { return m_ObjName; }
    }

    public AutoWaitObjectAppear(string objName)
    {
        m_ObjName = objName;
    }

    public override void Run()
    {
        GameObject obj = GameObject.Find(m_ObjName);
        if (obj != null && obj.activeInHierarchy)
        {
            m_Finish.Invoke();
        }
    }
}

public class AutoWaitObjectDisappear : AutoAction
{
    private string m_ObjName;
    public string ObjName
    {
        get { return m_ObjName; }
    }
    public AutoWaitObjectDisappear(string objName)
    {
        m_ObjName = objName;
    }

    public override void Run()
    {
        GameObject obj = GameObject.Find(m_ObjName);
        if (obj == null || !obj.activeInHierarchy)
        {
            m_Finish.Invoke();
        }
    }
}


public class AutoWaitComponentAppear : AutoAction
{
    private Type m_Type;
    public string Type
    {
        get { return m_Type.Name; }
    }

    public AutoWaitComponentAppear(Type comType)
    {
        m_Type = comType;
    }

    public override void Run()
    {
        GameObject obj = GameObject.FindObjectOfType(m_Type) as GameObject;
        if (obj != null)
        {
            m_Finish.Invoke();
        }
    }
}

public class AutoWaitComponentDisappear : AutoAction
{
    private Type m_Type;
    public string Type
    {
        get { return m_Type.Name; }
    }
    public AutoWaitComponentDisappear(Type comType)
    {
        m_Type = comType;
    }

    public override void Run()
    {
        GameObject obj = GameObject.FindObjectOfType(m_Type) as GameObject;
        if (obj == null)
        {
            m_Finish.Invoke();
        }
    }
}


public class AutoLabelTextAppear : AutoAction
{
    private string m_LabelName;
    public string LabelName
    {
        get { return m_LabelName; }
    }
    private string m_LabelContent;
    public string LableContent
    {
        get { return m_LabelContent; }
    }

    public AutoLabelTextAppear(string labelName, string labelContent)
    {
        m_LabelName = labelName;
        m_LabelContent = labelContent;
    }

    public override void Run()
    {
        GameObject obj = GameObject.Find(m_LabelName) as GameObject;

        if (obj != null)
        {
            Text text = obj.GetComponent<Text>();
            if (text != null && string.Equals(text.text, m_LabelContent))
            {
                m_Finish.Invoke();
            }
        }
    }
}

public class AutoTimer : AutoAction
{
    private float m_Length;
    public float Length
    {
        get { return m_Length; }
    }
    private float m_Timer;
    public AutoTimer(float length)
    {
        m_Length = length;
        m_Timer = 0;
    }

    public override void Run()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer > m_Length)
        {
            m_Finish.Invoke();
        }
    }
}

public class AutoWaitButtonAccessiable : AutoAction
{
    private string m_ButtonName;
    public string ButtonName
    {
        get { return m_ButtonName; }
    }

    public AutoWaitButtonAccessiable(string buttonName)
    {
        m_ButtonName = buttonName;
    }

    public override void Run()
    {
        GameObject obj = GameObject.Find(m_ButtonName);
        if (obj != null)
        {
            Button button = obj.GetComponent<Button>();
            if (button != null && button.isActiveAndEnabled)
            {
                m_Finish.Invoke();
            }
        }
    }
}

public class AutoClick : AutoAction
{
    private Vector3 m_Position;
    public Vector3 Position
    {
        get { return m_Position; }
    }
    public AutoClick(Vector3 pos)
    {
        m_Position = pos;
    }

    public override void Run()
    {
        InputUtils.SetFakeOnPress(m_Position);
        m_Finish.Invoke();
    }
}

public class AutoPressUI : AutoAction
{
    private string m_UIName;
    public string UIName
    {
        get { return m_UIName; }
    }
    public AutoPressUI(string uiName)
    {
        m_UIName = uiName;    
    }

    public override void Run()
    {
        GameObject obj = GameObject.Find(m_UIName);
        if(obj != null)
        {
            ExecuteEvents.Execute(obj, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            m_Finish.Invoke();
        }
    }
}

public class AutoLoadScene: AutoAction
{
    private string m_SceneName;
    public string SceneName
    {
        get { return m_SceneName; }
    }
    public AutoLoadScene(string sceneName)
    {
        m_SceneName = sceneName;
    }

    public override void Run()
    {
        LoadSceneManager.Instance.LoadSceneAsync(m_SceneName, false);
        m_Finish.Invoke();
    }
}


public class AutoActionContainer : AutoAction
{
    private List<AutoAction> m_ActionList = new List<AutoAction>();
    public List<AutoAction> ActionList
    {
        get { return m_ActionList; }
    }
    private int m_CurrentActionIndex;
    private int m_LoopTime;
    private int m_CurrentLoopTime;
    public AutoActionContainer (int loopTime = 1)
    {
        m_CurrentActionIndex = 0;
        if (loopTime < 1)
        {
            m_LoopTime = 1;
        }
        else
        {
            m_LoopTime = loopTime;
        }
        m_CurrentLoopTime = 0;
    }

    public void AddAction(AutoAction action)
    {
        m_ActionList.Add(action);
        action.SetFinishCallback(GetNextAction);
    }

    public override void Run()
    {
        m_ActionList[m_CurrentActionIndex].Run();
    }

    public override void SetFinishCallback(Action action)
    {
        base.SetFinishCallback(action);
        foreach(AutoAction act in m_ActionList)
        {
            act.SetFinishCallback(action);
        }
    }

    private void GetNextAction()
    {
        m_CurrentActionIndex++;
        if(m_CurrentActionIndex >= m_ActionList.Count)
        {
            if(m_CurrentLoopTime < m_LoopTime)
            {
                m_CurrentActionIndex = 0;
                m_CurrentLoopTime++;
            }
            else
            {
                m_Finish.Invoke();
            }
        }
    }
}