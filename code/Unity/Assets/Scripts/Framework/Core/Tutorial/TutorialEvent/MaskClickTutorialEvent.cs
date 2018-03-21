using UnityEngine;
using System;
using Snaplingo.Tutorial;

public class MaskClickTutorialEvent : TutorialEvent
{
	[JsonAttribute("drag", "", "拖动可跳过")]
	public bool _enableDrag = false;
	[JsonAttribute("trpt", "", "透明")]
	public bool _isTransparent = false;
	[JsonAttribute("maAl", "", "遮罩透明度")]
	public float _maskAlpha = 0.4f;

	public override void Init(TutorialManager manager, Action<TutorialEvent> shootEvent)
	{
		base.Init(manager, shootEvent);

		TutorialUIManager.Instance.ShowInteractableFullScreenMask(this, _enableDrag, _isTransparent, _maskAlpha);
	}

	public override void Cancel()
	{
		TutorialUIManager.Instance.HideInteractableFullScreenMask();
	}
}