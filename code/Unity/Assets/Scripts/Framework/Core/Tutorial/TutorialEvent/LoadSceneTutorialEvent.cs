using UnityEngine;
using Snaplingo.Tutorial;

public class LoadSceneTutorialEvent : TutorialEvent
{
	public override void Init(TutorialManager manager, System.Action<TutorialEvent> shootEvent)
	{
		base.Init(manager, shootEvent);

		LoadSceneManager.Instance.AddNextSceneHandler(Shoot);
	}

	public override void Cancel()
	{
		LoadSceneManager.Instance.RemoveNextSceneHandler(Shoot);
	}

}
