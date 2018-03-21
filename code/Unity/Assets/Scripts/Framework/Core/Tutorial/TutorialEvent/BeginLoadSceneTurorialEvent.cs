using UnityEngine;
using Snaplingo.Tutorial;

public class BeginLoadSceneTurorialEvent : TutorialEvent
{
	public override void Init(TutorialManager manager, System.Action<TutorialEvent> shootEvent)
	{
		base.Init(manager, shootEvent);
		LoadSceneManager.Instance.AddCurrentSceneEndCallback(Shoot);
	}


	public override void Cancel()
	{
		LoadSceneManager.Instance.RemoveCurrentSceneEndCallback(Shoot);
		TutorialManager.Instance.StopTutorial();
	}
}
