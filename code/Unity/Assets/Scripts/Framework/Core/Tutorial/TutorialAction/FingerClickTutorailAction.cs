using UnityEngine;
using Snaplingo.Tutorial;
using Snaplingo.UI;
using System;

// 手指点击的事件（非指向）
public class FingerClickTutorailAction : TutorialAction
{
	[JsonAttribute("node", "", "节点名(类名)")]
	public string _nodeName = "";
	[JsonAttribute("trgt", "", "对象路径(相对节点)")]
	public string _targetPath = "";

	protected override void Init()
	{
		if (PageManager.Instance.CurrentPage != null) {
			var nodeInstance = PageManager.Instance.CurrentPage.GetNode(Type.GetType(_nodeName));
			Transform rectTrans = nodeInstance.transform.Find(_targetPath);
			TutorialUIManager.Instance.ShowFingerClick(rectTrans.position);
		}
	}

	protected override void Cancel()
	{
		TutorialUIManager.Instance.HideFingerClick();
	}

	#if UNITY_EDITOR
	#pragma warning disable 0414
	[JsonEditorAttribute(JsonEditorAttribute.SpecialType.NodeIndex, "node")]
	int _nodeIndex = 0;
	#pragma warning restore
	#endif
}
