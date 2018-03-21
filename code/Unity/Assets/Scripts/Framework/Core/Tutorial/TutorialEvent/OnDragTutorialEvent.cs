using UnityEngine;
using Snaplingo.Tutorial;
using Snaplingo.UI;
using System;

public class OnDragTutorialEvent : TutorialEvent
{
	[JsonAttribute("node", "", "节点名(类名)")]
	public string _nodeName = "";
	[JsonAttribute("trgt", "", "对象路径(相对节点)")]
	public string _targetPath = "";

	GameObject _operateObject;

	public override void Init(TutorialManager manager, System.Action<TutorialEvent> shootEvent)
	{
		base.Init(manager, shootEvent);
		Type type = Type.GetType(_nodeName);
		if (PageManager.Instance.CurrentPage != null) {
			var nodeInstance = PageManager.Instance.CurrentPage.GetNode(type);
			if (nodeInstance != null) {
				var rectTrans = nodeInstance.transform.Find(_targetPath);
				if (rectTrans) {
					_operateObject = rectTrans.gameObject;
					UGUIEventListener.Get(_operateObject).onDragStart = delegate {
						Shoot();
					};
				} else
					LogManager.LogWarning("Warning! No button found in node: " + _nodeName + ": " + _targetPath);
			}
		}
	}

	public override void Cancel()
	{
		var com = _operateObject.GetComponent<UGUIEventListener>();
		if (com)
			TutorialUIManager.Destroy(com);
	}

	#if UNITY_EDITOR
	#pragma warning disable 0414
	[JsonEditorAttribute(JsonEditorAttribute.SpecialType.NodeIndex, "node")]
	int _nodeIndex = 0;
	#pragma warning restore
	#endif
}

