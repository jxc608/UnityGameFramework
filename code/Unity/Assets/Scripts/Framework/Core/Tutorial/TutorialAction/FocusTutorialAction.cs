using UnityEngine;
using Snaplingo.Tutorial;
using Snaplingo.UI;
using System;

public class FocusTutorialAction : TutorialAction
{
	[JsonAttribute("node", "", "节点名(类名)")]
	public string _nodeName = "";
	[JsonAttribute("trgt", "", "对象路径(相对节点)")]
	public string _targetPath = "";
	[JsonAttribute("maAl", "", "遮罩透明度")]
	public float _maskAlpha = 1;

	protected override void Init()
	{
		Type type = Type.GetType(_nodeName);
		if (PageManager.Instance.CurrentPage != null) {
			var nodeInstance = PageManager.Instance.CurrentPage.GetNode(type);
			if (nodeInstance != null) {
				var rectTrans = nodeInstance.transform.Find(_targetPath) as RectTransform;
				if (rectTrans) {
					TutorialUIManager.Instance.ShowFocusMasks(rectTrans, nodeInstance.RootCanvas, _maskAlpha);
					TutorialUIManager.Instance.ShowFocusFrame(rectTrans, nodeInstance.RootCanvas);
				} else
					LogManager.LogWarning("Warning! No button found in node: " + _nodeName + ": " + _targetPath);
			}
		}
	}

	protected override void Cancel()
	{
		TutorialUIManager.Instance.HideFocusMasks();
		TutorialUIManager.Instance.HideFocusFrame();
	}

	#if UNITY_EDITOR
	#pragma warning disable 0414
	[JsonEditorAttribute(JsonEditorAttribute.SpecialType.NodeIndex, "node")]
	int _nodeIndex = 0;
	#pragma warning restore
	#endif
}
