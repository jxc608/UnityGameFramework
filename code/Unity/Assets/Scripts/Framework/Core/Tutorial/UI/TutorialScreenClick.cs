using UnityEngine;
using System.Collections;
using Snaplingo.Tutorial;

public class TutorialScreenClick : MonoBehaviour
{
	TutorialEvent _event = null;

	public TutorialEvent Event {
		get { return _event; }
		set { _event = value; }
	}

	bool _enableDrag = false;

	public bool EnableDrag {
		get { return _enableDrag; }
		set { _enableDrag = value; }
	}

	Vector2 _touchStartPoint;

	void Update()
	{
		if (InputUtils.OnPressed()) {
			_touchStartPoint = InputUtils.GetTouchPosition();
		} else if (InputUtils.OnReleased()) {
			if (EnableDrag || (InputUtils.GetTouchPosition() - _touchStartPoint).sqrMagnitude < 225 && Event != null) {
				Event.Shoot();
			}
		}
	}
}
