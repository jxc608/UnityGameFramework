using UnityEngine;
using System;
using Snaplingo.Tutorial;
using Snaplingo.UI;

public class BtnClickTutorialEvent : TutorialEvent
{
    [JsonAttribute("node", "", "节点名(类名)")]
    public string _nodeName = "";
    [JsonAttribute("trgt", "", "对象路径(相对节点)")]
    public string _targetPath = "";

    TutorialClick _tutorialClick = null;

    public override void Init(TutorialManager manager, Action<TutorialEvent> shootEvent)
    {
        base.Init(manager, shootEvent);

        Type type = Type.GetType(_nodeName);
        if (PageManager.Instance.CurrentPage != null) {
            var nodeInstance = PageManager.Instance.CurrentPage.GetNode(type);
            if (nodeInstance != null) {
                var rectTrans = nodeInstance.transform.Find(_targetPath) as RectTransform;
                if (rectTrans) {
                    _tutorialClick = rectTrans.gameObject.AddComponent<TutorialClick>();
                    _tutorialClick.Event = this;
                } else
                    LogManager.LogWarning("Warning! No button found in node: " + _nodeName + ": " + _targetPath);
            }
        }
    }

    public override void Cancel()
    {
        if (_tutorialClick)
            UnityEngine.Object.Destroy(_tutorialClick);
    }

    #if UNITY_EDITOR
    #pragma warning disable 0414
    [JsonEditorAttribute(JsonEditorAttribute.SpecialType.NodeIndex, "node")]
    int _nodeIndex = 0;
    #pragma warning restore
    #endif

}
