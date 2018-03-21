using Snaplingo.Tutorial;
using UnityEngine;

public class MaskClickTutorialAction : TutorialAction
{
	protected override void Init()
	{
		TutorialUIManager.Instance.ShowNoninteractableFullScreenMask();
	}

	protected override void Cancel()
	{
		TutorialUIManager.Instance.HideNoninteractableFullScreenMask();
	}
}
