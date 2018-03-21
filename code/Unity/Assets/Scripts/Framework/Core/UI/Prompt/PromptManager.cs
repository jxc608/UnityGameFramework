using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PromptManager : GeneralUIManager
{
	#region Instance

	private static PromptManager _instance;

	public static PromptManager Instance {
		get {
			if (_instance == null) {
				GameObject go = new GameObject();
				_instance = go.AddComponent<PromptManager>();
				go.name = _instance.GetType().ToString();
				_instance.AddToUIManager();
				_instance.SetPriority(Priority.Common);
				_instance.Init();
			}
			return _instance;
		}
	}

	public void OnDestory()
	{
		_instance = null;
	}

	void Init()
	{
		_checkTip = ResourceLoadUtils.Load<GameObject>("Framework/Core/UI/Prompt/CheckTip") as GameObject;
		_windowTip = ResourceLoadUtils.Load<GameObject>("Framework/Core/UI/Prompt/WindowTip") as GameObject;
		_floatingTip = ResourceLoadUtils.Load<GameObject>("Framework/Core/UI/Prompt/FloatingTip") as GameObject;
		_noticeTip = ResourceLoadUtils.Load<GameObject>("Framework/Core/UI/Prompt/NoticeTip") as GameObject;
	}

	#endregion

	public enum Type
	{
		CheckTip,
		WindowTip,
		FloatingTip,
		NoticeTip,
	}

	public enum Result
	{
		OK,
		Cancel,
	}

	GameObject _tip;
	GameObject _checkTip;
	GameObject _windowTip;
	GameObject _floatingTip;
	GameObject _noticeTip;

	public delegate void OnReceiveMessageBoxResult(Result result);

	private event OnReceiveMessageBoxResult onReceiveMessageBoxResult = null;

	class Tip
	{
		public Type _type;
		public string _desc;
		public OnReceiveMessageBoxResult _callback;
	}

	List<Tip> tips = new List<Tip>();

	/// <summary>
	/// 通用UI Type: Float浮动提示 Window窗口提示 Check确认窗口 Received数值获取窗口 ReceivedRes道具获取窗口(3个及以下) ReceivedResBig道具获取窗口(3个以上)
	/// </summary>
	/// <param name="type"></param>
	/// <param name="desc"></param>
	/// <param name="callback"></param>
	public void MessageBox(Type type, string desc, OnReceiveMessageBoxResult callback = null)
	{
		Tip tempTip = new Tip();
		tempTip._type = type;
		tempTip._desc = desc;
		onReceiveMessageBoxResult = null;
		if (type != Type.FloatingTip)
			onReceiveMessageBoxResult += DeleteAndShowTip;
		onReceiveMessageBoxResult += callback;
		tempTip._callback = onReceiveMessageBoxResult;
		if (type == Type.FloatingTip)
			ShowTip(tempTip);
		else
			AddAndShowTip(tempTip);
	}

	void AddAndShowTip(Tip tempTip)
	{
		tips.Add(tempTip);
		if (tips.Count == 1) {
			ShowTip(tips[0]);
		}
	}

	FloatingTip _activeFloatingTip;

	void ShowTip(Tip tempTip)
	{
		switch (tempTip._type) {
			case Type.FloatingTip:
				if (_activeFloatingTip != null)
					_activeFloatingTip.Close();
				_tip = Instantiate(_floatingTip);
				_activeFloatingTip = _tip.GetComponent <FloatingTip>();
				InitFloatingTip(_tip, tempTip._desc, tempTip._callback);
				AudioManager.Instance.PlaySfx("FloatTip");
				break;
			case Type.CheckTip:
				_tip = Instantiate(_checkTip);
				InitTip(_tip, tempTip._desc, tempTip._callback);
				break;
			case Type.WindowTip:
				_tip = Instantiate(_windowTip);
				InitTip(_tip, tempTip._desc, tempTip._callback);
				//AudioManager.Instance.PlaySfx("SecondWindow");
				break;
			case Type.NoticeTip:
				_tip = Instantiate(_noticeTip);
				InitTip(_tip, tempTip._desc, tempTip._callback);
				break;
		}

		EnableParentCanvasRaycaster(transform);
	}

	public void DeleteAndShowTip(Result result = Result.Cancel)
	{
		if (tips.Count > 0) {
			tips.RemoveAt(0);
		}

		if (tips.Count > 0) {
			ShowTip(tips[0]);
		} else {
			if (_activeFloatingTip == null)
				DisableParentCanvasRaycaster(transform);
		}
	}

	void InitFloatingTip(GameObject obj, string desc, OnReceiveMessageBoxResult callback = null)
	{
		var script = obj.GetComponent<FloatingTip>();
		InitTransform(obj);
		script.Init(desc, callback);
		//TODO 控制消失方式
	}

	void InitTip(GameObject obj, string desc, OnReceiveMessageBoxResult callback = null)
	{
		var script = obj.GetComponent<CheckTip>();
		InitTransform(obj);
		script.Init(desc, callback);
	}

	void InitTransform(GameObject obj)
	{
		var trans = obj.transform as RectTransform;
		if (trans) {
			trans.SetParent(transform);
			trans.localPosition = Vector3.zero;
			trans.localScale = Vector3.one;
			trans.sizeDelta = Vector2.zero;
			trans.localPosition = Vector2.zero;
		}
	}
}
