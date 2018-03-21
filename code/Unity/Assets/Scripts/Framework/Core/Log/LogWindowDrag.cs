using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LogWindowDrag : EventTrigger
{
	Transform _logWindow;

	void Start()
	{
		_logWindow = transform.parent;
	}

	public override void OnDrag(PointerEventData eventData)
	{
		_logWindow.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
	}
}
