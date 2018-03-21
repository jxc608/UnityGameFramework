using UnityEngine;
using Snaplingo.Tutorial;
using Snaplingo.UI;
using System;

public class OnDragTutorialAction : TutorialAction
{
	[JsonAttribute("node", "", "节点名(类名)")]
	public string _nodeName = "";
	[JsonAttribute("trgtS", "", "对象路径(开始点)")]
	public string _targetPathS = "";
	[JsonAttribute("trgtE", "", "对象路径(结束点)")]
	public string _targetPathE = "";
	[JsonAttribute("res", "", "资源路径")]
	public string _resource = "Framework/Core/Tutorial/ImageMove/Finger";
	[JsonAttribute("sca", "", "缩放")]
	public Vector3 _scale = Vector3.one;
	[JsonAttribute("ang", "", "角度")]
	public Vector3 _angle = Vector3.zero;
	[JsonAttribute("speed", "", "移动速度")]
	public int _speed = 1;
	[JsonAttribute("ipp", "", "是否往复")]
	public bool _isPingPong = false;

	GameObject _finger;

	protected override void Init()
	{
		Type type = Type.GetType(_nodeName);
		if (PageManager.Instance.CurrentPage != null) {
			var nodeInstance = PageManager.Instance.CurrentPage.GetNode(type);
			if (nodeInstance != null) {
				var startPoint = nodeInstance.transform.Find(_targetPathS);
				var endPoint = nodeInstance.transform.Find(_targetPathE);
				if (startPoint && endPoint) {
					_finger = GameObject.Instantiate(ResourceLoadUtils.Load<GameObject>(_resource));
					_finger.SetActive(false);
					_finger.transform.SetParent(TutorialUIManager.Instance.SwitchTransform);
					_finger.transform.localScale = _scale;
					_finger.transform.rotation = Quaternion.Euler(_angle);
					_finger.transform.position = startPoint.position;
					_finger.SetActive(true);
					TutorialUIManager.Instance.GameObjectMove(_finger, startPoint.position, endPoint.position, _speed, _isPingPong);
				} else
					LogManager.LogWarning("Warning! No button found in node: " + _nodeName + ": " + _targetPathS + ": " + _targetPathE);
			}
		}
	}

	protected override void Cancel()
	{
		TutorialUIManager.Destroy(_finger);
	}

	#if UNITY_EDITOR
	#pragma warning disable 0414
	[JsonEditorAttribute(JsonEditorAttribute.SpecialType.NodeIndex, "node")]
	int _nodeIndex = 0;
	#pragma warning restore
	#endif
}
