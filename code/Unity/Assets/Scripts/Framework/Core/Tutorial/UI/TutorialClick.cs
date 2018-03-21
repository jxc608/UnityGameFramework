using UnityEngine;
using UnityEngine.EventSystems;

namespace Snaplingo.Tutorial
{
	public class TutorialClick : MonoBehaviour, IPointerClickHandler
	{
		TutorialEvent _event = null;

		public TutorialEvent Event {
			get { return _event; }
			set { _event = value; }
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (Event != null) {
				Event.Shoot();
			}
		}
	}
}
