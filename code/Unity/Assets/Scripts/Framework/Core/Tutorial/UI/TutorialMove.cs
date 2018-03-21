using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Snaplingo.Tutorial
{
	public class TutorialMove : MonoBehaviour
	{
		TutorialEvent _event = null;

		public TutorialEvent Event {
			get { return _event; }
			set { _event = value; }
		}

		public bool canShoot = false;

		event UnityAction _loopCallBack = null;

		public UnityAction LoopCallBack {
			get { return _loopCallBack; }
			set { _loopCallBack = value; }
		}

		void Update()
		{
			if (Event != null && canShoot)
				Event.Shoot();

		}

		void LateUpdate()
		{
			if (LoopCallBack != null)
				LoopCallBack();
		}

		void OnDestroy()
		{
			if (LoopCallBack != null)
				LoopCallBack = null;
		}
	}
}
