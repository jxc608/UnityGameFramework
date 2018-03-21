using System;
using System.Text;
using LitJson;
using UnityEngine;

public class AutoActionJsonWriter
{
    public static string Write(AutoActionContainer container)
    {
        if (container.ActionList.Count == 0)
            return "";
        
        StringBuilder sb = new StringBuilder();

        sb.Append("[");
        for (int i = 0; i < container.ActionList.Count; i++)
        {
            AutoAction action = container.ActionList[i];
            JsonData data = new JsonData();
            switch (action.GetType().Name)
            {
                case "AutoActionContainer":
                    data["name"] = "Container";
                    data["data"] = JsonMapper.ToObject(Write((AutoActionContainer)action));
                    break;
                case "AutoTimer":
                    data["name"] = "Timer";
                    JsonData timerData = new JsonData();
                    timerData["time_length"] = (action as AutoTimer).Length.ToString();
                    data["data"] = timerData;
                    break;
                case "AutoClick":
                    data["name"] = "Click";
                    JsonData clickData = new JsonData();
                    clickData["pos_x"] = (action as AutoClick).Position.x.ToString();
                    clickData["pos_y"] = (action as AutoClick).Position.y.ToString();
                    clickData["pos_z"] = (action as AutoClick).Position.z.ToString();
                    data["data"] = clickData;
                    break;
                case "AutoPressUI":
                    data["name"] = "PressUI";
                    JsonData pressUIData = new JsonData();
                    pressUIData["ui_name"] = (action as AutoPressUI).UIName;
                    data["data"] = pressUIData;
                    break;
                case "AutoLoadScene":
                    data["name"] = "LoadScene";
                    JsonData loadSceneData = new JsonData();
                    loadSceneData["scene_name"] = (action as AutoLoadScene).SceneName;
                    data["data"] = loadSceneData;
                    break;
                case "AutoWaitButtonAccessiable":
                    data["name"] = "WaitButtonAccessible";
                    JsonData buttonAccessibleData = new JsonData();
                    buttonAccessibleData["button_name"] = (action as AutoWaitButtonAccessiable).ButtonName;
                    data["data"] = buttonAccessibleData;
                    break;
                case "AutoLabelTextAppear":
                    data["name"] = "LabelTextAppear";
                    JsonData labelTextData = new JsonData();
                    labelTextData["object_name"] = (action as AutoLabelTextAppear).LabelName;
                    labelTextData["content"] = (action as AutoLabelTextAppear).LableContent;
                    data["data"] = labelTextData;
                    break;
                case "AutoWaitComponentDisappear":
                    data["name"] = "WaitComponentDisappear";
                    JsonData componentDisappearData = new JsonData();
                    componentDisappearData["component_name"] = (action as AutoWaitComponentDisappear).Type;
                    data["data"] = componentDisappearData;
                    break;
                case "AutoWaitComponentAppear":
                    data["name"] = "WaitComponentAppear";
                    JsonData componentAppearData = new JsonData();
                    componentAppearData["component_name"] = (action as AutoWaitComponentAppear).Type;
                    data["data"] = componentAppearData;
                    break;
                case "AutoWaitObjectAppear":
                    data["name"] = "WaitObjectAppear";
                    JsonData objectAppearData = new JsonData();
                    objectAppearData["object_name"] = (action as AutoWaitObjectAppear).ObjName;
                    data["data"] = objectAppearData;
                    break;
                case "AutoWaitObjectDisappear":
                    data["name"] = "WaitObjectDisappear";
                    JsonData objectDisappearData = new JsonData();
                    objectDisappearData["object_name"] = (action as AutoWaitObjectDisappear).ObjName;
                    data["data"] = objectDisappearData;
                    break;
                case "AutoWaitScene":
                    data["name"] = "WaitScene";
                    JsonData waitSceneLoad = new JsonData();
                    waitSceneLoad["scene_name"] = (action as AutoWaitScene).SceneName;
                    data["data"] = waitSceneLoad;
                    break;
            }

            sb.Append(data.ToJson());

            if(i < container.ActionList.Count - 1)
                sb.Append(",");
        }
       
        sb.Append("]");

        return sb.ToString();

    }
}

