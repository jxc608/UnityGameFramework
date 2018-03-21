using UnityEngine;
using Snaplingo.UI;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(Page), true)]
public class EditorPage : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		Page page = (Page)target;
		if (page.transform.parent) {
			var canvasScaler = page.transform.parent.GetComponent<CanvasScaler>();
			if (canvasScaler) {
				var rectTrans = page.transform as RectTransform;
				rectTrans.anchorMin = new Vector2(.5f, .5f);
				rectTrans.anchorMax = new Vector2(.5f, .5f);
				rectTrans.pivot = new Vector2(.5f, .5f);
				rectTrans.sizeDelta = (canvasScaler.transform as RectTransform).sizeDelta;
				rectTrans.anchoredPosition = Vector2.zero;
				rectTrans.localRotation = Quaternion.identity;
				rectTrans.localScale = Vector3.one;
			}
		}

	}
}
