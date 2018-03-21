using System;
using LitJson;

public class AutoActionJsonParser
{
    public static AutoAction Parse(JsonData content)
    {
        AutoActionContainer container = new AutoActionContainer();

        ParseDataToContainer(container, content);
       
        return container;
    }

    private static void ParseDataToContainer(AutoActionContainer container, JsonData content)
    {
        for (int i = 0; i < content.Count; i++)
        {
            string key = content[i].TryGetString("name");
            JsonData data = content[i]["data"];

            AutoAction action = null;
            switch (key)
            {
                case "Container":
                    action = Parse(data);
                    break;
                case "WaitScene":
                    string waitSceneName = data.TryGetString("scene_name");
                    action = new AutoWaitScene(waitSceneName);
                    break;
                case "WaitObjectAppear":
                    string objectAppear = data.TryGetString("object_name");
                    action = new AutoWaitObjectAppear(objectAppear);
                    break;
                case "WaitObjectDisappear":
                    string objectDisappear = data.TryGetString("object_name");
                    action = new AutoWaitObjectAppear(objectDisappear);
                    break;
                case "WaitComponentAppear":
                    string componentAppear = data.TryGetString("component_name");
                    action = new AutoWaitComponentAppear(Type.GetType(componentAppear));
                    break;
                case "WaitComponentDisappear":
                    string componentDisappear = data.TryGetString("component_name");
                    action = new AutoWaitComponentDisappear(Type.GetType(componentDisappear));
                    break;
                case "LabelTextAppear":
                    string labelObjectName = data.TryGetString("object_name");
                    string textContent = data.TryGetString("content");
                    action = new AutoLabelTextAppear(labelObjectName, textContent);
                    break;
                case "Timer":
                    string time = data.TryGetString("time_length");
                    action = new AutoTimer(float.Parse(time));
                    break;
                case "WaitButtonAccessible":
                    string buttonName = data.TryGetString("button_name");
                    action = new AutoWaitButtonAccessiable(buttonName);
                    break;
                case "Click":
                    float x = float.Parse(data.TryGetString("pos_x")) ;
                    float y = float.Parse(data.TryGetString("pos_y")) ;
                    float z = float.Parse(data.TryGetString("pos_z")) ;
                    action = new AutoClick(new UnityEngine.Vector3(x,y,z));
                    break;
                case "PressUI":
                    string uiName = data.TryGetString("ui_name");
                    action = new AutoPressUI(uiName);
                    break;
                case "LoadScene":
                    string loadSceneName = data.TryGetString("scene_name");
                    action = new AutoLoadScene(loadSceneName);
                    break;
            }

            if(action != null)
                container.AddAction(action);
        }
    }
}

