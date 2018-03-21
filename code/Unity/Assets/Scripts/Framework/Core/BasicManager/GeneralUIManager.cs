using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GeneralUIManager : MonoBehaviour
{
	static Canvas _generalCanvas = null;

	public static void DisableTouch()
	{
		if (_generalCanvas != null) {
			_generalCanvas.GetComponent<GraphicRaycaster>().enabled = false;
		}
	}

	public static void EnableTouch()
	{
		if (_generalCanvas != null) {
			_generalCanvas.GetComponent<GraphicRaycaster>().enabled = true;
		}
	}

	public static void RefreshSortingOrder()
	{
		if (_generalCanvas != null) {
			_generalCanvas.sortingOrder = 10;
		}
	}

	protected void AddToUIManager()
	{
		if (_generalCanvas == null) {
			var go = new GameObject();
			go.name = typeof(GeneralUIManager).ToString();

			_generalCanvas = go.AddComponent<Canvas>();
			_generalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			_generalCanvas.sortingOrder = 10;

			var canvasScaler = go.AddComponent<CanvasScaler>();
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.referenceResolution = ConfigurationController.Instance._CanvasResolution;
			canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

			var raycaster = go.AddComponent<GraphicRaycaster>();
			raycaster.enabled = false;

			DontDestroyOnLoad(go);
		}

		transform.SetParent(_generalCanvas.transform, false);
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		transform.SetAsFirstSibling();

		StartCoroutine(ReviseRectSize());
	}

	IEnumerator ReviseRectSize()
	{
		yield return null;

		var rectTrans = gameObject.GetComponent<RectTransform>();
		if (rectTrans == null)
			rectTrans = gameObject.AddComponent<RectTransform>();
		rectTrans.sizeDelta = (transform.parent as RectTransform).sizeDelta;
	}

	public static Canvas GetCanvas()
	{
		return _generalCanvas;
	}

	#region 显示顺序

	protected enum Priority
	{
		Lowest,
		Common,
		Tutorial,
		Log,
	}

	Priority _priority;

	protected void SetPriority(Priority priority)
	{
		_priority = priority;
		RefreshChildren();
	}

	void RefreshChildren()
	{
		int uiCount = transform.parent.childCount;
		for (int i = 0; i < uiCount - 1; i++) {
			for (int j = 0; j < uiCount - 1 - i; j++) {
				Transform cur = transform.parent.GetChild(j);
				Transform next = transform.parent.GetChild(j + 1);
				if (cur.GetComponent<GeneralUIManager>()._priority
				                > next.GetComponent<GeneralUIManager>()._priority) {
					cur.SetSiblingIndex(j + 1);
				}
			}
		}
	}

	#endregion

	#region 如无操作者，关闭GraphicRaycaster以优化表现

	static List<Transform> handlers = new List<Transform>();

	public static void EnableParentCanvasRaycaster(Transform trans)
	{
		trans.root.GetComponent<GraphicRaycaster>().enabled = true;
		if (!handlers.Contains(trans))
			handlers.Add(trans);
	}

	public static void DisableParentCanvasRaycaster(Transform trans)
	{
		handlers.RemoveAll(delegate(Transform t) {
			return t == null;
		});
		if (handlers.Contains(trans))
			handlers.Remove(trans);
		if (handlers.Count == 0)
			trans.root.GetComponent<GraphicRaycaster>().enabled = false;
	}

	#endregion
}
