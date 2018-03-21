using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UGUIEventListener : MonoBehaviour,
                                IPointerClickHandler,
                                IPointerDownHandler,
                                IPointerEnterHandler,
                                IPointerExitHandler,
                                IPointerUpHandler,
                                IBeginDragHandler,
                                IDragHandler,
                                IEndDragHandler
{
	public delegate void VoidDelegate(GameObject go);

	public VoidDelegate onClick;
	public VoidDelegate onDown;
	public VoidDelegate onEnter;
	public VoidDelegate onExit;
	public VoidDelegate onUp;
	public VoidDelegate onDragStart;
	public VoidDelegate onDrag;
	public VoidDelegate onDragEnd;
	public VoidDelegate onLongPress;
	//与长按配合使用
	bool isUp = true;

	public object parameter;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (onClick != null) {
			onClick(gameObject);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (onDown != null)
			onDown(gameObject);
		isUp = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (onEnter != null)
			onEnter(gameObject);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (onExit != null)
			onExit(gameObject);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (onUp != null)
			onUp(gameObject);
		isUp = true;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (onDragStart != null)
			onDragStart(gameObject);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (onDrag != null)
			onDrag(gameObject);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (onDragEnd != null)
			onDragEnd(gameObject);
	}

	static public UGUIEventListener Get(GameObject go)
	{
		UGUIEventListener listener = go.GetComponent<UGUIEventListener>();
		if (listener == null)
			listener = go.AddComponent<UGUIEventListener>();
		return listener;
	}

	void Update()
	{
		if (!isUp && onLongPress != null) {
			onLongPress(gameObject);
		}
	}

}