using UnityEngine;
using Snaplingo.Tutorial;
using Snaplingo.UI;
using System;
using UnityEngine.UI;

public class PictureTutorialAction : TutorialAction
{
	[JsonAttribute("node", "", "节点名(类名)")]
	public string _nodeName = "";
	[JsonAttribute("trgtS", "", "位置路径")]
	public string _targetPathS = "";
	[JsonAttribute("path", "", "资源名")]
	public string _path = "Tutorial/TutorialArrow";
	[JsonAttribute("ang", "", "角度")]
	public Vector3 _angle = Vector3.zero;
	[JsonAttribute("scl", "", "缩放")]
	public float _scale = 1f;
    [JsonAttribute("a", "", "透明度")]
    public float _alpha = 1f;

	GameObject _arrow;

	protected override void Init()
	{
		_arrow = GameObject.Instantiate(ResourceLoadUtils.Load<GameObject>(_path));
		_arrow.SetActive(false);
		_arrow.transform.SetParent(TutorialUIManager.Instance.SwitchTransform);
		_arrow.transform.localScale = new Vector3(_scale, _scale, _scale);
		_arrow.transform.rotation = Quaternion.Euler(_angle);
		Type type = Type.GetType(_nodeName);
		if (PageManager.Instance.CurrentPage != null) {
			var nodeInstance = PageManager.Instance.CurrentPage.GetNode(type);
			if (nodeInstance != null) {
				var trans = nodeInstance.transform.Find(_targetPathS);
				if (trans != null) {
					_arrow.transform.position = trans.position;
				} else
					LogManager.LogWarning("Warning! No button found in node: " + _nodeName + ": " + _targetPathS);
			}
		}
        var oldColor = _arrow.GetComponent<Image> ().color;
        _arrow.GetComponent<Image> ().color = new Color (oldColor.r, oldColor.g, oldColor.b, _alpha);
		_arrow.SetActive(true);
	}

	protected override void Cancel()
	{
		TutorialUIManager.Destroy(_arrow);
	}

	#if UNITY_EDITOR
	#pragma warning disable 0414
	[JsonEditorAttribute(JsonEditorAttribute.SpecialType.NodeIndex, "node")]
	int _nodeIndex = 0;
	#pragma warning restore
	#endif
}
