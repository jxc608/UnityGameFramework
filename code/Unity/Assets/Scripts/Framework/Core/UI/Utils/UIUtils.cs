using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Snaplingo.UI
{
	public class UIUtils
	{
		public static void AttachAndReset(GameObject go, Transform parent, GameObject prefab = null)
		{
			var rectTrans = go.transform as RectTransform;
			if (rectTrans) {
				rectTrans.SetParent(parent);
				rectTrans.localPosition = Vector3.zero;
				rectTrans.localScale = Vector3.one;
				if (prefab == null) {
					rectTrans.sizeDelta = Vector2.zero;
					rectTrans.localPosition = Vector2.zero;
					rectTrans.offsetMax = Vector2.zero;
					rectTrans.offsetMin = Vector2.zero;
				} else {
					var prefabRectTrans = prefab.transform as RectTransform;
					if (prefabRectTrans) {
						rectTrans.sizeDelta = prefabRectTrans.sizeDelta;
						rectTrans.localPosition = prefabRectTrans.localPosition;
						rectTrans.offsetMax = prefabRectTrans.offsetMax;
						rectTrans.offsetMin = prefabRectTrans.offsetMin;
					}
				}
			}
		}

		public static void RegisterButton(string buttonPath, UnityEngine.Events.UnityAction action, Transform transform)
		{
			var child = transform.Find(buttonPath);
			if (child) {
                child.GetComponent<Button> ().onClick.RemoveListener (action);
				child.GetComponent<Button>().onClick.AddListener(action);
			}
		}

		public static void RemoveOnClickListeners(string buttonPath, Transform transform)
		{
			var child = transform.Find(buttonPath);
			if (child) {
				child.GetComponent<Button>().onClick.RemoveAllListeners();
			}
		}

		public static void SetText(string textPath, string text, Transform transform)
		{
			var child = transform.Find(textPath);
			if (child) {
				child.GetComponentInChildren<Text>(true).text = text;
			}
		}

		public static string GetInputFieldText(string textPath, Transform transform)
		{
			var child = transform.Find(textPath);
			if (child) {
				return child.GetComponentInChildren<InputField>(true).text;
			}

			return "";
		}

		public static void SetInputField(string textPath, string text, Transform transform)
		{
			var child = transform.Find(textPath);
			if (child) {
				child.GetComponentInChildren<InputField>(true).text = text;
			}
		}

		public static Vector2 GetCenterPosInCanvas(Canvas canvas, RectTransform rectTrans)
		{
			Vector2 pos;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
			                                                            rectTrans.position, canvas.worldCamera, out pos)) {
				var delta = new Vector2((.5f - rectTrans.pivot.x) * rectTrans.rect.width, (.5f - rectTrans.pivot.y) * rectTrans.rect.height);
				return pos + delta;
			}

			throw new System.Exception("Error! Get RectTransform center position in canvas fail.");
		}

		public static Vector2 GetPositionForNewCanvas(Canvas srcCanvas, Canvas dstCanvas, Vector2 pos)
		{
			pos.x *= (dstCanvas.transform as RectTransform).sizeDelta.x / (srcCanvas.transform as RectTransform).sizeDelta.x;
			pos.y *= (dstCanvas.transform as RectTransform).sizeDelta.y / (srcCanvas.transform as RectTransform).sizeDelta.y;
			return pos;
		}

		public static void SetScreenPosition(ref RectTransform target, Canvas canvas, Vector2 dstCenter)
		{
			Vector2 center = UIUtils.GetCenterPosInCanvas(canvas, target);
			target.anchoredPosition += dstCenter - center;
		}

		public static Rect GetRectInCanvas(Canvas canvas, RectTransform rectTrans)
		{
			Vector2 pos;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
			                                                            rectTrans.position, canvas.worldCamera, out pos)) {
				var rect = new Rect(new Vector2(pos.x - rectTrans.pivot.x * rectTrans.rect.width, pos.y - rectTrans.pivot.y * rectTrans.rect.height), rectTrans.rect.size);
				return rect;
			}

			throw new System.Exception("Error! Get RectTransform rect in canvas fail.");
		}

		public static float CalculateAutoMatchedDeltaX()
		{
			return ((Screen.width * ConfigurationController.Instance._CanvasResolution.y / Screen.height) - ConfigurationController.Instance._CanvasResolution.x) * 0.5f;
		}

		public static Vector2 CalculateAutoMatchedSize(Vector2 standardSize)
		{
			float ratio = (Screen.width * ConfigurationController.Instance._CanvasResolution.y / Screen.height) / ConfigurationController.Instance._CanvasResolution.x;
			return standardSize * ratio;
		}

		public static Rect CalculateAutoMatchedRect(Rect rect, float anchorX, bool needScaleWidth)
		{
			rect.x -= (1f - anchorX * 2f) * CalculateAutoMatchedDeltaX();
			if (needScaleWidth) {
				var oldWidth = rect.width;
				rect.size = CalculateAutoMatchedSize(rect.size);
				rect.x -= (1f - anchorX * 2f) * (rect.width - oldWidth) * .5f;
			}

			return rect;
		}

		public static float NormalizedAngle(float angle)
		{
			angle = angle % 360;
			if (angle > 180f)
				angle -= 360f;
			else if (angle <= -180f)
				angle += 360f;
			return angle;
		}

		public static Vector2 GetOctagonalPosOfRectByAngle(Rect rect, float angle, float gap = 0)
		{
			angle = NormalizedAngle(angle);
			Vector2 pos;
			if (angle >= 157.5f || angle < -157.5f)
				pos = new Vector2(rect.xMin - gap, rect.center.y);
			else if (angle >= 112.5f)
				pos = new Vector2(rect.xMin - gap, rect.yMax + gap);
			else if (angle >= 67.5f)
				pos = new Vector2(rect.center.x, rect.yMax + gap);
			else if (angle >= 22.5f)
				pos = new Vector2(rect.xMax + gap, rect.yMax + gap);
			else if (angle >= -22.5f)
				pos = new Vector2(rect.xMax + gap, rect.center.y);
			else if (angle >= -67.5f)
				pos = new Vector2(rect.xMax + gap, rect.yMin - gap);
			else if (angle >= -112.5f)
				pos = new Vector2(rect.center.x, rect.yMin - gap);
			else // if (angle >= - 157.5f)
                pos = new Vector2(rect.xMin - gap, rect.yMin - gap);

			return pos;
		}

		/// <summary>
		/// 根据RectTransform、角度和UI比例值(Canvas的Scales值)获取元素对应位置的世界坐标--指示器用
		/// </summary>
		/// <param name="rtTarget"></param>
		/// <param name="angle"></param>
		/// <param name="uiProportion"></param>
		/// <returns></returns>
		public static Vector2 GetOctagonalPosOfRectByAngleInWorld(RectTransform rtTarget, float angle, float uiProportion)
		{
			angle = NormalizedAngle(angle);
			Vector2 startPosition;
			float width = rtTarget.rect.width * uiProportion;
			float height = rtTarget.rect.height * uiProportion;
			Vector2 belongToMin = (Vector2)rtTarget.position - new Vector2(width * rtTarget.pivot.x, height * rtTarget.pivot.y);
			if (angle == 0)
				startPosition = belongToMin + Vector2.up * height * .5f;
			else if (angle == 90)
				startPosition = belongToMin + Vector2.right * width * .5f;
			else if (angle == 180)
				startPosition = belongToMin + Vector2.right * width + Vector2.up * height * .5f;
			else if (angle == -90)
				startPosition = belongToMin + Vector2.right * width * .5f + Vector2.up * height;
			else if (angle > 0 && angle < 90)
				startPosition = belongToMin;
			else if (angle > 90 && angle < 180)
				startPosition = belongToMin + Vector2.right * width;
			else if (angle > -90 && angle < 0)
				startPosition = belongToMin + Vector2.up * height;
			else// if (angle > -180 && angle < -90)
                startPosition = belongToMin + Vector2.right * width + Vector2.up * height;
			return startPosition;
		}

		/// <summary>
		/// 根据角度获取对应单位向量
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static Vector2 GetNormalizedFromAngle(float angle)
		{
			return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
		}

		public static void DestroyChildren(Transform parent)
		{
			for (int i = parent.childCount - 1; i >= 0; i--) {
				GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
			}
		}

		/// <summary>
		/// 根据RectTransform和UI比例值(Canvas的Scales值)获取元素中心的世界坐标
		/// </summary>
		/// <param name="rtf"></param>
		/// <param name="uiProportion"></param>
		/// <returns></returns>
		public static Vector2 GetCenterPosInWorldFromRectTf(RectTransform rtf, float uiProportion)
		{
			Vector2 pivot0 = (Vector2)rtf.position - new Vector2(rtf.rect.width * rtf.pivot.x, rtf.rect.height * rtf.pivot.y) * uiProportion;
			return pivot0 + new Vector2(rtf.rect.width * .5f, rtf.rect.height * .5f) * uiProportion;
		}

		public static void SetAllChildrenActive(Transform trans, bool active)
		{
			for (int i = 0; i < trans.childCount; ++i)
				trans.GetChild(i).gameObject.SetActive(active);
		}

		public static bool IsClickUI()
		{
			bool res = false;
			if (EventSystem.current != null) {
				res = EventSystem.current.currentSelectedGameObject != null;
				if (!res) {
					PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
					eventDataCurrentPosition.position = InputUtils.GetTouchPosition();

					Canvas canvas = PageManager.Instance.GetComponent<Canvas>();
					GraphicRaycaster uiRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
					List<RaycastResult> results = new List<RaycastResult>();
					uiRaycaster.Raycast(eventDataCurrentPosition, results);
					res = results.Count > 0;
					if (!res) {
						canvas = GeneralUIManager.GetCanvas();
						uiRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
						results = new List<RaycastResult>();
						uiRaycaster.Raycast(eventDataCurrentPosition, results);
						res = results.Count > 0;
					}
				}
			}

			return res;
		}

        public static void Fade(Transform trans, float alpha, float duration)
        {
            foreach (Transform ts in trans)
            {
                if (ts.childCount > 0)
                {
                    Fade(ts, alpha, duration);
                }

                Image image = ts.GetComponent<Image>();
                if (image != null)
                {
                    image.DOFade(alpha, duration);
                }
                else
                {
                    Text text = ts.GetComponent<Text>();
                    if (text != null)
                    {
                        text.DOFade(alpha, duration);
                    }
                }
            }
        }

		public static void SetAlpha(Transform trans, float alpha)
        {
            foreach (Transform ts in trans)
            {
                if (ts.childCount > 0)
                {
					SetAlpha(ts, alpha);
                }

                Image image = ts.GetComponent<Image>();
                if (image != null)
                {
					image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
                }
                else
                {
                    Text text = ts.GetComponent<Text>();
                    if (text != null)
                    {
						text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                    }
                }
            }
        }

		public static void Kill(Transform trans)
        {
            foreach (Transform ts in trans)
            {
                if (ts.childCount > 0)
                {
					Kill(ts);
                }

                ts.DOKill();
				Image image = ts.GetComponent<Image>();
                if (image != null)
                	image.DOKill();
				Text text =ts.GetComponent<Text>();
                if (text != null)
                	text.DOKill();
                RectTransform tr = ts.GetComponent<RectTransform>();
                if( tr != null )
                	tr.DOKill();
                
            }
        }
    }
}