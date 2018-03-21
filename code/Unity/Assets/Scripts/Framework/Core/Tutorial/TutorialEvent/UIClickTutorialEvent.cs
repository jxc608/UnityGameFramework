using UnityEngine;
using System;
using Snaplingo.Tutorial;
using Snaplingo.UI;

public class UIClickTutorialEvent : TutorialEvent
{
	[JsonAttribute("node", "", "节点名(类名)")]
	public string _nodeName = "";
	[JsonAttribute("trgt", "", "对象路径(相对节点)")]
	public string _targetPath = "";
	[JsonAttribute("maAl", "", "遮罩透明度")]
	public float _maskAlpha = 0.4f;

	#pragma warning disable 0414
	[JsonAttribute("fcs", "", "显示焦点框")]
	bool _showFocus = false;
	#pragma warning restore

	[JsonAttribute("mask", "fcs", "显示遮罩")]
	public bool _enableMasks = true;
	[JsonAttribute("frm", "fcs", "显示外框")]
	public bool _enableFrame = true;
	[JsonAttribute("ptr", "fcs", "显示指针")]
	public bool _enablePointer = true;
	[JsonAttribute("agl", "ptr", "指针角度")]
	public float _pointerAngle = 180f;
	[JsonAttribute("move", "ptr", "指针移动")]
	public bool _isPointerMove = false;
	[JsonAttribute("range", "move", "移动幅度(单位:像素)")]
	public float _moveRange = 0;

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
					if (_enableMasks)
						TutorialUIManager.Instance.ShowFocusMasks(rectTrans, nodeInstance.RootCanvas, _maskAlpha);
					if (_enableFrame)
						TutorialUIManager.Instance.ShowFocusFrame(rectTrans, nodeInstance.RootCanvas);
					if (_enablePointer)
						TutorialUIManager.Instance.ShowFocusPointer(rectTrans, nodeInstance.RootCanvas, _pointerAngle, _isPointerMove, _moveRange);
				} else
					LogManager.LogWarning("Warning! No button found in node: " + _nodeName + ": " + _targetPath);
			}
		}
	}

	public override void Cancel()
	{
		if (_tutorialClick)
			UnityEngine.Object.Destroy(_tutorialClick);
		if (_enableMasks)
			TutorialUIManager.Instance.HideFocusMasks();
		if (_enableFrame)
			TutorialUIManager.Instance.HideFocusFrame();
		if (_enablePointer)
			TutorialUIManager.Instance.HideFocusPointer();
	}

	#if UNITY_EDITOR
	#pragma warning disable 0414
	[JsonEditorAttribute(JsonEditorAttribute.SpecialType.NodeIndex, "node")]
	int _nodeIndex = 0;
	#pragma warning restore
	#endif

}
