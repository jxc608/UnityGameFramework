using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Snaplingo.UI;

public class LogManager : GeneralUIManager
{
	static bool _showLog = false;

	public static void Log(object info)
	{
		CheckDisplayUI();
		if (DebugConfigController.Instance._Debug)
			Debug.Log(info);
	}

	public static void LogWarning(object info)
	{
		CheckDisplayUI();
		if (DebugConfigController.Instance._Debug)
			Debug.LogWarning(info);
	}

	public static void LogError(object info)
	{
		CheckDisplayUI();
		if (DebugConfigController.Instance._Debug)
			Debug.LogError(info);
	}

	public static void CheckDisplayUI()
	{
		if (Application.isPlaying && DebugConfigController.Instance._Debug && !_showLog) {
			Instance.DoNothing();
			_showLog = true;
		}
	}

	#region instance

	static LogManager _instance;

	static LogManager Instance {
		get {
			if (_instance == null) {
				GameObject go = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/Log/LogManager"));
				_instance = go.GetComponent<LogManager>();
				go.name = _instance.GetType().ToString();
				_instance.AddToUIManager();
				_instance.Init();
			}
			return _instance;
		}
	}

	void OnDestory()
	{
		_instance = null;
	}

	void Init()
	{
		SetPriority(Priority.Log);
		transform.Find("ShowHideBtn").gameObject.SetActive(false);
		Application.logMessageReceived -= addLogCallCallBack;
		Application.logMessageReceived += addLogCallCallBack;
		EnableParentCanvasRaycaster(transform);
		UIUtils.RegisterButton(_showHideBtn.name, delegate {
			SetLogShow(!_showUI);
		}, transform);
		UIUtils.RegisterButton("HidePanel", delegate {
			SetLogShow(false);
		}, transform);
		SetLogShow(false);
		StartCoroutine(DelayDisplayUI());
	}

	IEnumerator DelayDisplayUI()
	{
		yield return null;
		_instance.transform.Find("ShowHideBtn").gameObject.SetActive(true);
	}

	void DoNothing()
	{
	}

	#endregion

	class LogClass
	{
		public int index;
		public LogType logType;
		public string condition;
		public string stacktrace;

		public LogClass(int i, LogType type, string con, string stt)
		{
			index = i;
			logType = type;
			condition = con;
			stacktrace = stt;
		}
	}

	static int _curIndex = 0;
	static Dictionary<string, LogClass> _logDic = new Dictionary<string, LogClass>();

	public ScrollRect _logScrollView;
	public GameObject _logItem;
	public ScrollRect _showTextScroll;
	public GameObject _showHideBtn;
	public Text _title;
    
	bool _showUI = false;

	void SetLogShow(bool show)
	{
		_showUI = show;
		transform.Find("HidePanel").gameObject.SetActive(_showUI);
		transform.Find("Panel").gameObject.SetActive(_showUI);
		var color = _showHideBtn.GetComponent<Image>().color;
		color.a = _showUI ? 1f : .5f;
		_showHideBtn.GetComponent<Image>().color = color;
	}

	void AddToLogScroll(LogClass log)
	{
		GameObject newLogItem = Instantiate(_logItem) as GameObject;
		newLogItem.name = "logItem" + log.index;
		newLogItem.GetComponentInChildren<Text>().text = log.logType.ToString() + "_" + log.condition;
		newLogItem.transform.SetParent(_logScrollView.content);
		newLogItem.SetActive(true);
		newLogItem.transform.localScale = Vector3.one;
		switch (log.logType) {
			case LogType.Error:
				newLogItem.GetComponentInChildren<Text>().color = Color.red;
				break;
			case LogType.Exception:
				newLogItem.GetComponentInChildren<Text>().color = Color.red;
				break;
			case LogType.Warning:
				newLogItem.GetComponentInChildren<Text>().color = new Color(1, 0.75f, 0, 1);
				break;
		}
		UIUtils.RegisterButton(newLogItem.name, delegate {
			ShowCurLogText(newLogItem);
		}, _logScrollView.content);
		_logDic.Add(newLogItem.name, log);
	}

	GameObject _lastClkGo = null;

	void ShowCurLogText(GameObject clkGo)
	{
		if (_lastClkGo != null) {
			_lastClkGo.GetComponent<Image>().color = Color.white;
		}
		if (clkGo == _lastClkGo) {
			_showTextScroll.gameObject.SetActive(!_showTextScroll.gameObject.activeSelf);
		} else {
			_showTextScroll.gameObject.SetActive(true);
			_showTextScroll.content.GetComponent<Text>().text = _logDic[clkGo.name].condition + "\n\n" + _logDic[clkGo.name].stacktrace;
		}

		clkGo.GetComponent<Image>().color = _showTextScroll.gameObject.activeSelf ? new Color(0.8f, 0.8f, 0.8f, 1) : Color.white;
		_lastClkGo = clkGo;
	}

	void addLogCallCallBack(string condition, string stacktrace, LogType type)
	{
		LogClass log = new LogClass(_curIndex++, type, condition, stacktrace);
		AddToLogScroll(log);
	}
}
