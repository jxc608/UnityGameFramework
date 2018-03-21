using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using Snaplingo.UI;
using System.Collections.Generic;

namespace Snaplingo.Tutorial
{
	public class TutorialUIManager : GeneralUIManager
	{
		#region instance

		[RuntimeInitializeOnLoadMethod]
		static void InitializeOnLoad()
		{
			_instance = Instance;
		}

		private static TutorialUIManager _instance = null;

		public static bool HasInstance()
		{
			return _instance != null;
		}

		public static TutorialUIManager Instance {
			get {
				if (_instance == null) {
					GameObject go = new GameObject();
					_instance = go.AddComponent<TutorialUIManager>();
					go.name = _instance.GetType().ToString();
					_instance.AddToUIManager();
					_instance.SetPriority(Priority.Tutorial);
					GameObject uiSwitch = new GameObject("UISwitch");
					uiSwitch.transform.SetParent(go.transform);
					uiSwitch.transform.localPosition = Vector3.zero;
					uiSwitch.transform.localScale = Vector3.one;
					uiSwitch.AddComponent<RectTransform>();
					_switchTransform = uiSwitch.transform;
				}

				_instance.StartCoroutine(_instance.DelaySetSizeData());
				return _instance;
			}
		}

		IEnumerator DelaySetSizeData()
		{
			yield return null;
			transform.Find("UISwitch").GetComponent<RectTransform>().sizeDelta = transform.parent.GetComponent<RectTransform>().sizeDelta;
		}

		public void OnDestory()
		{
			_instance = null;
		}

		/// <summary>
		/// UI 开关的transform,当外部建立物体时可以找到此父亲节点
		/// </summary>
		static Transform _switchTransform = null;

		public Transform SwitchTransform {
			get { return _switchTransform; }
			private set { _switchTransform = value; }
		}

		#endregion

		#region 遮罩

		RectTransform[] _focusMasks = new RectTransform[4];

		public void ShowFocusMasks(RectTransform focusRectTf, Canvas canvas, float _maskAlpha = .4f)
		{
			Rect focusRect = focusRectTf.rect;
			for (int i = 0; i < _focusMasks.Length; ++i) {
				if (_focusMasks[i] == null) {
					var go = new GameObject();
					go.name = "FocusMask" + (i + 1);
					var image = go.AddComponent<Image>();
					image.color = new Color(0, 0, 0, _maskAlpha);
					_focusMasks[i] = go.transform as RectTransform;
					_focusMasks[i].SetParent(SwitchTransform);
					_focusMasks[i].localScale = Vector3.one;
					_focusMasks[i].gameObject.SetActive(false);
				} else
					_focusMasks[i].GetComponent<Image>().color = new Color(0, 0, 0, _maskAlpha);
			}

			float width = 6000f, height = 6000f;
			var pageCanvasScale = canvas.transform.localScale.x;
			var tutorialCanvasScale = transform.parent.localScale.x;
			float pt = pageCanvasScale / tutorialCanvasScale;
			for (int i = 0; i < _focusMasks.Length; ++i) {
				Vector2 newCenter = UIUtils.GetCenterPosInWorldFromRectTf(focusRectTf, pageCanvasScale);
				switch (i) {
					case 0:
						_focusMasks[i].sizeDelta = new Vector2(width, height);
                        newCenter += Vector2.left * (width * tutorialCanvasScale + focusRect.width * focusRectTf.localScale.x * pageCanvasScale) * .5f;
						break;
					case 1:
                        _focusMasks[i].sizeDelta = new Vector2(focusRect.width * focusRectTf.localScale.x * pt, height);
                        newCenter += Vector2.up * (height * tutorialCanvasScale + focusRect.height * focusRectTf.localScale.y * pageCanvasScale) * .5f;
						break;
					case 2:
						_focusMasks[i].sizeDelta = new Vector2(width, height);
                        newCenter += Vector2.right * (width * tutorialCanvasScale + focusRect.width  * focusRectTf.localScale.x * pageCanvasScale) * .5f;
						break;
					case 3:
					default:
                        _focusMasks[i].sizeDelta = new Vector2(focusRect.width * focusRectTf.localScale.x * pt, height);
                        newCenter += Vector2.down * (height * tutorialCanvasScale + focusRect.height* focusRectTf.localScale.y * pageCanvasScale) * .5f;
						break;
				}
				_focusMasks[i].position = newCenter;
			}

			for (int i = 0; i < _focusMasks.Length; ++i) {
				_focusMasks[i].gameObject.SetActive(true);
				_focusMasks[i].SetAsFirstSibling();
				EnableParentCanvasRaycaster(_focusMasks[i]);
			}
		}

		public void HideFocusMasks()
		{
			for (int i = 0; i < _focusMasks.Length; ++i) {
				if (_focusMasks[i] != null) {
					_focusMasks[i].gameObject.SetActive(false);
					DisableParentCanvasRaycaster(_focusMasks[i]);
				}
			}
		}

		public bool HasFocusMasks()
		{
			for (int i = 0; i < _focusMasks.Length; ++i) {
				if (_focusMasks[i] != null && _focusMasks[i].gameObject.activeInHierarchy) {
					return true;
				}
			}

			return false;
		}

		#endregion

		#region 焦点框

		public RectTransform _focusFrame = null;

		public void ShowFocusFrame(RectTransform rectTrans, Canvas canvas)
		{
			if (_focusFrame == null) {
				var go = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/Focus/FocusFrame")) as GameObject;
				_focusFrame = go.transform as RectTransform;
				_focusFrame.gameObject.SetActive(false);
				_focusFrame.SetParent(SwitchTransform);
                _focusFrame.localScale = Vector3.one;
            }
            _focusFrame.localScale = rectTrans.localScale;
			var pageCanvasScale = canvas.transform.localScale.x;
			var tutorialCanvasScale = transform.parent.localScale.x;
			_focusFrame.position = UIUtils.GetCenterPosInWorldFromRectTf(rectTrans, pageCanvasScale);
			float pt = pageCanvasScale / tutorialCanvasScale;
			_focusFrame.sizeDelta = new Vector2(rectTrans.rect.size.x * pt + 45, rectTrans.rect.size.y * pt + 45);

			_focusFrame.gameObject.SetActive(true);

			EnableParentCanvasRaycaster(_focusFrame);
		}

		public void HideFocusFrame()
		{
			if (_focusFrame) {
				_focusFrame.gameObject.SetActive(false);
				DisableParentCanvasRaycaster(_focusFrame);
			}
		}

		public bool HasFocusFrame()
		{
			return _focusFrame != null && _focusFrame.gameObject.activeInHierarchy;
		}

		#endregion

		#region 指示箭头

		RectTransform _focusPointer = null;

		public void ShowFocusPointer(RectTransform focusRectTf, Canvas canvas, float angle, bool _isMove, float _moveRange, bool isPingPong = true)
		{
			StartCoroutine(ShowFocusPointerSub(focusRectTf, canvas, angle, _isMove, _moveRange, isPingPong));
		}

		IEnumerator ShowFocusPointerSub(RectTransform focusRectTf, Canvas canvas, float angle, bool _isMove, float _moveRange, bool isPingPong = true)
		{
			Vector2 moveDirection = default(Vector2);
			if (_isMove)
				moveDirection = UIUtils.GetNormalizedFromAngle(angle);
			if (_focusPointer == null) {
				var go = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/Focus/FocusPointer")) as GameObject;
				_focusPointer = go.transform as RectTransform;
				_focusPointer.gameObject.SetActive(false);
				_focusPointer.SetParent(SwitchTransform);
				_focusPointer.localScale = Vector3.one;
			}

			var pageCanvasScale = canvas.transform.localScale.x;
			Vector2 pivotPos = UIUtils.GetOctagonalPosOfRectByAngleInWorld(focusRectTf, angle, pageCanvasScale);
			_focusPointer.position = pivotPos;

			angle = UIUtils.NormalizedAngle(angle);
			_focusPointer.localScale = new Vector3(1, angle > 90 && angle < 270 ? 1 : -1, 1);
			_focusPointer.eulerAngles = new Vector3(0, 0, angle);

			yield return null;
			_focusPointer.gameObject.SetActive(true);

			if (_isMove) {
				Vector2 targetPosition = (Vector2)_focusPointer.position - moveDirection * _moveRange * pageCanvasScale;
				GameObjectMove(_focusPointer.gameObject, _focusPointer.position, targetPosition, 1, isPingPong);
			}
			EnableParentCanvasRaycaster(_focusPointer);
		}

		//指示箭头基于两点移动
		public void ShowFocusPointerMoveToPoint(RectTransform focusRectTf, Canvas canvas, float angle, RectTransform targetRtf, bool _isMove, bool isPingPong = true)
		{
			StartCoroutine(ShowFocusPointerByPointSub(focusRectTf, canvas, angle, targetRtf, _isMove, isPingPong));
		}

		IEnumerator ShowFocusPointerByPointSub(RectTransform focusRectTf, Canvas canvas, float angle, RectTransform targetRtf, bool _isMove, bool isPingPong = true)
		{
			if (_focusPointer == null) {
				var go = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/Focus/FocusPointer")) as GameObject;
				_focusPointer = go.transform as RectTransform;
				_focusPointer.gameObject.SetActive(false);
				_focusPointer.SetParent(SwitchTransform);
				_focusPointer.localScale = Vector3.one;
			}

			var pageCanvasScale = canvas.transform.localScale.x;
			Vector2 pivotPos = UIUtils.GetOctagonalPosOfRectByAngleInWorld(focusRectTf, angle, pageCanvasScale);
			_focusPointer.position = pivotPos;

			Vector2 targetPos = UIUtils.GetOctagonalPosOfRectByAngleInWorld(targetRtf, angle, pageCanvasScale);
			angle = UIUtils.NormalizedAngle(angle);
			_focusPointer.localScale = new Vector3(1, angle > 90 && angle < 270 ? 1 : -1, 1);
			_focusPointer.eulerAngles = new Vector3(0, 0, angle);

			yield return null;
			_focusPointer.gameObject.SetActive(true);

			if (_isMove) {
				GameObjectMove(_focusPointer.gameObject, _focusPointer.position, targetPos, 1, isPingPong);
			}
			EnableParentCanvasRaycaster(_focusPointer);
		}

		public void HideFocusPointer()
		{
			if (_focusPointer) {
				_focusPointer.gameObject.SetActive(false);
				DisableParentCanvasRaycaster(_focusPointer);
			}
		}

		public bool HasFocusPointer()
		{
			return _focusPointer != null && _focusPointer.gameObject.activeInHierarchy;
		}

		#endregion

		#region 可交互的全屏遮罩

		RectTransform _maskFullScreen = null;

		public void ShowInteractableFullScreenMask(TutorialEvent evt, bool enableDrag = false, bool isTransparent = false, float _maskAlpha = 0.4f)
		{
			Image image;
			if (_maskFullScreen == null) {
				var go = new GameObject();
				go.name = "FullScreenMask";
				image = go.AddComponent<Image>();
				_maskFullScreen = go.transform as RectTransform;
				_maskFullScreen.SetParent(SwitchTransform, false);
				go.SetActive(false);
				float width = 6000f, height = 6000f;
				_maskFullScreen.sizeDelta = new Vector2(width, height);
			} else {
				image = _maskFullScreen.gameObject.GetComponent<Image>();
			}
			if (isTransparent)
				image.color = new Color(0, 0, 0, 0);
			else
				image.color = new Color(0, 0, 0, _maskAlpha);
			if (evt != null) {
				var tc = _maskFullScreen.gameObject.AddComponent<TutorialScreenClick>();
				tc.Event = evt;
				tc.EnableDrag = enableDrag;
				image.raycastTarget = true;
			} else {
				image.raycastTarget = false;
			}

			_maskFullScreen.gameObject.SetActive(true);

			_maskFullScreen.SetAsFirstSibling();
			EnableParentCanvasRaycaster(_maskFullScreen);
		}

		public void HideInteractableFullScreenMask()
		{
			if (_maskFullScreen) {
				_maskFullScreen.gameObject.SetActive(false);
				DisableParentCanvasRaycaster(_focusPointer);
			}
		}

		public bool HasInteractableFullScreenMask()
		{
			return _maskFullScreen != null && _maskFullScreen.gameObject.activeInHierarchy;
		}

		#endregion

		#region 不可交互的全屏遮罩

		RectTransform _maskNoninteractableFullScreen = null;

		public void ShowNoninteractableFullScreenMask()
		{
			StartCoroutine(ShowNoninteractableFullScreenMaskSub());
		}

		IEnumerator ShowNoninteractableFullScreenMaskSub()
		{
			Image image;
			if (_maskNoninteractableFullScreen == null) {
				var go = new GameObject();
				go.name = "FullScreenMaskNoninteractable";
				image = go.AddComponent<Image>();
				_maskNoninteractableFullScreen = go.transform as RectTransform;
				_maskNoninteractableFullScreen.SetParent(SwitchTransform, false);
				go.SetActive(false);
				float width = 6000f, height = 6000f;
				_maskNoninteractableFullScreen.sizeDelta = new Vector2(width, height);
			} else {
				image = _maskFullScreen.gameObject.GetComponent<Image>();
			}
			image.color = new Color(0, 0, 0, 0);
			image.raycastTarget = true;

			yield return null;

			_maskNoninteractableFullScreen.gameObject.SetActive(true);
			_maskNoninteractableFullScreen.SetAsFirstSibling();//TODO 先设置全局遮罩，后设置局部遮罩 导致全局遮罩挡住了局部遮罩
			EnableParentCanvasRaycaster(_maskNoninteractableFullScreen);
		}

		public void HideNoninteractableFullScreenMask()
		{
			if (_maskNoninteractableFullScreen) {
				_maskNoninteractableFullScreen.gameObject.SetActive(false);
				DisableParentCanvasRaycaster(_focusPointer);
			}
		}

		public bool HasNoninteractableFullScreenMask()
		{
			return _maskNoninteractableFullScreen != null && _maskNoninteractableFullScreen.gameObject.activeInHierarchy;
		}

		#endregion

		#region 展示图片

		RectTransform _picture = null;

		public void ShowPicture(string path, bool assignPosition = false, Vector2 position = default(Vector2), float scale = 1f, bool autoMatch = true)
		{
			StartCoroutine(ShowPictureSub(path, assignPosition, position, scale, autoMatch));
		}

		IEnumerator ShowPictureSub(string path, bool assignPosition, Vector2 position, float scale, bool autoMatch)
		{
			Image image;
			if (_picture == null) {
				var go = new GameObject();
				go.name = "Picture";
				image = go.AddComponent<Image>();
				_picture = go.transform as RectTransform;
				_picture.SetParent(SwitchTransform);
				_picture.localScale = Vector3.one;
			} else {
				image = _picture.GetComponent<Image>();
			}


			if (assignPosition) {
				_picture.gameObject.SetActive(false);
				yield return null;

				Sprite sprite = ResourceLoadUtils.Load<Sprite>(Path.Combine("Framework/Core/Tutorial/Display", path));
				image.sprite = sprite;
				Vector2 spriteSize = new Vector2(sprite.texture.width * scale, sprite.texture.height * scale);
				if (autoMatch) {
					var oldSize = spriteSize;
					spriteSize = UIUtils.CalculateAutoMatchedSize(spriteSize);
					var anchorX = Mathf.Clamp01((position.x + spriteSize.x * .5f) / spriteSize.x);
					var adjust = UIUtils.CalculateAutoMatchedDeltaX() - (spriteSize.x - oldSize.x) * .5f;
					position.x -= (1f - anchorX * 2f) * adjust;
				}
				UIUtils.SetScreenPosition(ref _picture, GetCanvas(), position);
				_picture.sizeDelta = spriteSize;
			}

			_picture.gameObject.SetActive(true);
			EnableParentCanvasRaycaster(_picture);
		}

		public void HidePicture()
		{
			if (_picture != null) {
				DisableParentCanvasRaycaster(_picture);
				_picture.GetComponent<Image>().sprite = null;
				Resources.UnloadUnusedAssets();
				_picture.gameObject.SetActive(false);
			}
		}

		public bool HasPicture()
		{
			return _picture != null && _picture.gameObject.activeInHierarchy;
		}

		#endregion

		#region 显示对话框

		RectTransform _dialog = null;

		public enum CharacterType
		{
			TipBall1,
			TipBall2,
			TipBall3,
			TipBall4,

			Count,
		}

		GameObject[] _tipBalls = new GameObject[(int)CharacterType.Count];

		public enum DialogType
		{
			Bottom,
			Anywhere,
		}

		public enum CharacterPos
		{
			Left,
			Right,
			BottomLeft,
			BottomRight,

			Count,
		}

		public void ShowDialog(DialogType type, CharacterPos cPos, CharacterType chType, string text, bool isDisplayContinue, Rect rectPanel, int hatIndex, bool hideTipball = false)
		{
			if (_dialog == null) {
				var go = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/Dialog/Dialog")) as GameObject;
				_dialog = go.transform as RectTransform;
				_dialog.SetParent(SwitchTransform, false);
				_dialog.localScale = Vector3.one;
			}
			_dialog.gameObject.SetActive(false);

			RectTransform root = _dialog.Find(type.ToString()) as RectTransform;
			Text contentText = null;
			for (int i = 0; i < _dialog.childCount; ++i) {
				var go = _dialog.GetChild(i).gameObject;
				go.SetActive(type.ToString() == go.name);
				if (type.ToString() == go.name) {
					contentText = go.transform.Find("Background/Text").GetComponent<Text>();
				}
			}
			if (contentText)
				contentText.text = text;
			Transform rtrans = null;

			switch (type) {
				case DialogType.Anywhere:
					root.anchorMin = new Vector2(rectPanel.xMin, rectPanel.yMin);
					root.anchorMax = new Vector2(rectPanel.xMax, rectPanel.yMax);

					for (CharacterPos i = 0; i < CharacterPos.Count; ++i) {
						var parentTrans = root.Find("Background/" + i.ToString() + "/Tipball");
						parentTrans.parent.gameObject.SetActive(i == cPos);
						if (i == cPos) {
							rtrans = parentTrans;
							var anchorPos = (rtrans.parent as RectTransform).anchoredPosition;
							Rect rootRect = UIUtils.GetRectInCanvas(GetCanvas(), root.Find("Background") as RectTransform);
							switch (cPos) {
								case CharacterPos.BottomLeft:
									(rtrans.parent as RectTransform).anchoredPosition = new Vector2(rootRect.width * -.3f, anchorPos.y);
									break;
								case CharacterPos.BottomRight:
									(rtrans.parent as RectTransform).anchoredPosition = new Vector2(rootRect.width * .3f, anchorPos.y);
									break;
							}
						}
					}
					root.Find("Background/ClickContinuePic").gameObject.SetActive(isDisplayContinue);
					break;
				case DialogType.Bottom:
					rtrans = root.Find("Background/Tipball");
					root.Find("Background/ClickContinuePic").gameObject.SetActive(isDisplayContinue);
					break;
			}
			for (int i = 0; i < _tipBalls.Length; ++i) {
				if ((int)chType == i && !hideTipball) {
					if (_tipBalls[i] == null) {
						_tipBalls[i] = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/Dialog/" + chType.ToString()), rtrans, false);
					} else {
						_tipBalls[i].transform.SetParent(rtrans, false);
						_tipBalls[i].SetActive(true);
					}
					for (int j = 0; j < _tipBalls[i].transform.childCount; j++)
						_tipBalls[i].transform.GetChild(j).gameObject.SetActive(hatIndex == j);
				} else {
					if (_tipBalls[i])
						_tipBalls[i].SetActive(false);
				}
			}

			_dialog.gameObject.SetActive(true);
			EnableParentCanvasRaycaster(_dialog);
		}

		public void HideDialog()
		{
			if (_dialog != null) {
				_dialog.gameObject.SetActive(false);
				DisableParentCanvasRaycaster(_dialog);
			}
		}

		public bool HasDialog()
		{
			return _dialog != null && _dialog.gameObject.activeInHierarchy;
		}

		#endregion

		#region 图片动画，基于两点

		public void GameObjectMove(GameObject go, Vector2 startPoint, Vector2 targetPoint, float speed = 1f, bool isPingPong = false)
		{
			StartCoroutine(GameObjectMoveSub(go, startPoint, targetPoint, speed, isPingPong));
		}

		IEnumerator GameObjectMoveSub(GameObject go, Vector2 startPoint, Vector2 targetPoint, float speed, bool isPingPong = false)
		{
			float progress = 0;
			bool isToTarget = true;
			while (go && go.activeSelf) {
				if (isPingPong)
					if (isToTarget)
						progress += speed * Time.deltaTime;
					else
						progress -= speed * Time.deltaTime * 2;//收回速度加倍
                else
					progress += speed * Time.deltaTime;

				var target = go.GetComponent<RectTransform>();
				target.transform.position = Vector2.Lerp(startPoint, targetPoint, progress);
				//UIUtils.SetScreenPosition (ref target, GetCanvas (), Vector2.Lerp (startPoint, targetPoint, progress));
				if (isPingPong) {
					if (progress >= 1)
						isToTarget = false;
					else if (progress <= 0)
							isToTarget = true;
				} else {
					if (progress >= 1) {
						progress = 0;
						yield return new WaitForSeconds(0.4f);
					}
				}
				yield return null;
			}
		}

		#endregion

		#region 手指点击动画

		GameObject _fingerClickGo = null;

		public void ShowFingerClick(Vector2 position)
		{
			StartCoroutine(ShowFingerClickSub(position));
		}

		IEnumerator ShowFingerClickSub(Vector2 position)
		{
			if (!_fingerClickGo)
				_fingerClickGo = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/FingerClick/FingerClick")) as GameObject;
			_fingerClickGo.SetActive(false);
			_fingerClickGo.transform.SetParent(SwitchTransform);
			_fingerClickGo.transform.localScale = Vector3.one;
			_fingerClickGo.transform.position = position;
			_fingerClickGo.SetActive(true);
			yield return null;
			_fingerClickGo.GetComponent<Animation>().Play();
		}

		public void HideFingerClick()
		{
			if (_fingerClickGo) {
				_fingerClickGo.GetComponent<Animation>().Stop();
				_fingerClickGo.SetActive(false);
			}
		}

		#endregion

		#region 箭头往复运动动画(支持多个)

		public void ArrowAnimation(Transform target, Canvas canvas, float moveRange, float angle, List<GameObject> lgo)
		{
			StartCoroutine(ArrowAnimationSub(target, canvas, moveRange, angle, lgo));
		}

		IEnumerator ArrowAnimationSub(Transform target, Canvas canvas, float moveRange, float angle, List<GameObject> lgo)
		{
			GameObject arrow = GameObject.Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/ImageMove/Arrow"));
			arrow.SetActive(false);
			lgo.Add(arrow);
			arrow.transform.SetParent(SwitchTransform);
			arrow.transform.localScale = Vector3.one;

			Vector2 startPosition = default(Vector2);
			angle = UIUtils.NormalizedAngle(angle);

			if (target.GetComponent<RectTransform>()) {
				var pageCanvasScale = canvas.transform.localScale.x;
				startPosition = UIUtils.GetOctagonalPosOfRectByAngleInWorld(target.GetComponent<RectTransform>(), angle, pageCanvasScale);
			} else
				startPosition = Camera.main.WorldToScreenPoint(target.position);
			arrow.transform.localEulerAngles = Vector3.forward * angle;
			Vector2 targetPosition = startPosition + moveRange * UIUtils.GetNormalizedFromAngle(-angle);
			arrow.transform.position = targetPosition;
			arrow.SetActive(true);
			yield return null;
			GameObjectMove(arrow, startPosition, targetPosition, 1, true);
		}

		#endregion

		#region 隐藏/显示 TutorialUIManager实例

		public void Hide()
		{
			SwitchTransform.gameObject.SetActive(false);
		}

		public void Show()
		{
			SwitchTransform.gameObject.SetActive(true);
		}

		/// <summary>
		/// 新手是否正在执行
		/// </summary>
		public bool IsActiving()
		{
			foreach (Transform t in SwitchTransform) {
				if (t.gameObject.activeSelf)
					return true;
			}
			return false;
		}

		#endregion

		#region 聊天气泡

		RectTransform _chatBubble = null;

		public enum ChatBubbleArrowPos
		{
			Left,
			Middle,
			Right,

			Count,
		}

		public void ShowChatBubble(ChatBubbleArrowPos cPos, string text, Rect rectPanel, Vector4 deltaSize, bool adaptation = true)
		{
			if (_chatBubble == null) {
				var go = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Tutorial/ChatBubble/ChatBubble")) as GameObject;
				_chatBubble = go.transform as RectTransform;
				_chatBubble.SetParent(SwitchTransform, false);
				_chatBubble.localScale = Vector3.one;
			}
			_chatBubble.gameObject.SetActive(false);
			RectTransform root = _chatBubble.Find("Bubble") as RectTransform;
			if (adaptation) {
				root.anchorMin = new Vector2(rectPanel.xMin, rectPanel.yMin);
				root.anchorMax = new Vector2(rectPanel.xMax, rectPanel.yMax);
			} else {
				root.anchorMin = Vector2.one * 0.5f;
				root.anchorMax = Vector2.one * 0.5f;
				root.localPosition = new Vector2(deltaSize.x, deltaSize.y);
				root.sizeDelta = new Vector2(deltaSize.z, deltaSize.w);
			}

			Text contentText = null;
			contentText = root.Find("Text").GetComponent<Text>();
			if (contentText)
				contentText.text = text;
            
			for (ChatBubbleArrowPos i = 0; i < ChatBubbleArrowPos.Count; ++i) {
				root.Find("Background/" + i.ToString()).gameObject.SetActive(i.Equals(cPos));
			}

			_chatBubble.gameObject.SetActive(true);
			EnableParentCanvasRaycaster(_chatBubble);
		}

		public void HideChatBubble()
		{
			if (_chatBubble != null) {
				_chatBubble.gameObject.SetActive(false);
				DisableParentCanvasRaycaster(_chatBubble);
			}
		}

		public bool HasChatBubble()
		{
			return _chatBubble != null && _chatBubble.gameObject.activeInHierarchy;
		}

		#endregion
	}
}
