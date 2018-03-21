using UnityEngine;
using System;
using System.Collections;
using Snaplingo.Tutorial;

public class WaitingTutorialEvent : TutorialEvent
{
	[JsonAttribute("inte", "", "间隔")]
	public float _interval = 1f;

	Coroutine _waitingCoroutine = null;

	public override void Init(TutorialManager manager, Action<TutorialEvent> shootEvent)
	{
		base.Init(manager, shootEvent);

		_waitingCoroutine = manager.StartCoroutine(WaitingForInterval());
	}

	IEnumerator WaitingForInterval()
	{
		yield return new WaitForSeconds(_interval);

		Shoot();
	}

	public override void Cancel()
	{
		if (_waitingCoroutine != null) {
			_tutorialManager.StopCoroutine(_waitingCoroutine);
			_waitingCoroutine = null;
		}
	}
}