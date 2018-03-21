using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using UnityEngine.UI;

public class AutoTestUnitTest : MonoBehaviour {

    private void Awake()
    {
        DontDestroyOnLoad(this);    
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 50, 50), "test"))
        {
            AutoActionContainer container = new AutoActionContainer();

            AutoLoadScene loadScene = new AutoLoadScene("test");

            AutoClick click = new AutoClick(new Vector3(10, 20, 30));

            AutoTimer timer = new AutoTimer(5);

            AutoPressUI pressUI = new AutoPressUI("bg");

            container.AddAction(loadScene);

            container.AddAction(click);

            container.AddAction(timer);

            container.AddAction(pressUI);

            AutoActionContainer container2 = new AutoActionContainer();

            AutoWaitObjectAppear objectAppear = new AutoWaitObjectAppear("page");

            AutoWaitObjectAppear objectDisappear = new AutoWaitObjectAppear("pageDis");

            AutoWaitComponentAppear componentAppear = new AutoWaitComponentAppear(typeof(Image));

            AutoWaitComponentDisappear componentDisappear = new AutoWaitComponentDisappear(typeof(Button));

            AutoLabelTextAppear labelText = new AutoLabelTextAppear("label", "hello world");

            container2.AddAction(container);

            container2.AddAction(objectAppear);

            container2.AddAction(labelText);

            container2.AddAction(objectDisappear);

            container2.AddAction(componentAppear);

            container2.AddAction(componentDisappear);

            string json = AutoActionJsonWriter.Write(container2);
            Debug.Log("json is:" + json);

            JsonData data = JsonMapper.ToObject(json);
    
            AutoActionContainer newContainer = AutoActionJsonParser.Parse(data) as AutoActionContainer;

            foreach(AutoAction action in newContainer.ActionList)
            {
                Debug.Log(action.GetType().Name);
                if(action.GetType().Name == "AutoActionContainer")
                {
                    foreach (AutoAction childAction in (action as AutoActionContainer).ActionList)
                    {
                        Debug.Log(childAction.GetType().Name);
                    }
                }
            }
        }
    }
}
