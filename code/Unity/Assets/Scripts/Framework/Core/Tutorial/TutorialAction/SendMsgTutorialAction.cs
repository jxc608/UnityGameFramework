using UnityEngine;
using Snaplingo.Tutorial;

public class SendMsgTutorialAction : TutorialAction
{
	public enum ParaType
	{
		None,
		String,
		Int,
		Bool,
	}

	[JsonAttribute("object", "", "对象名")]
	public string _object = "";
	[JsonAttribute("name", "", "方法名")]
	public string _func = "";
	[JsonAttribute("hasPara", "", "参数")]
	public bool _hasPara = false;
	[JsonAttribute("type", "hasPara", "参数类型")]
	public ParaType _type;
	[JsonAttribute("para", "hasPara", "参数")]
	public string _para = "";
	[JsonAttribute("end", "hasPara", "结束发送消息")]
	public bool _endSend = false;
	[JsonAttribute("endPara", "end", "结束参数")]
	public string _endPara = "";

	GameObject _gameObject = null;

	protected override void Init()
	{
		_gameObject = GameObject.Find(_object);
		if (_gameObject == null) {
			LogManager.Log("PlaySendMsgAction: Gameobject is not exist.");
			return;
		}
		object parameter = null;
		switch (_type) {
			case ParaType.None:
				break;
			case ParaType.String:
				parameter = _para;
				break;
			case ParaType.Int:
				parameter = int.Parse(_para);
				break;
			case ParaType.Bool:
				parameter = bool.Parse(_para);
				break;

		}
		_gameObject.SendMessage(_func, parameter, SendMessageOptions.DontRequireReceiver);
	}

	protected override void Cancel()
	{
		if (_gameObject == null)
			return;
		if (_endSend) {
			object parameter = null;
			switch (_type) {
				case ParaType.None:
					break;
				case ParaType.String:
					parameter = _endPara;
					break;
				case ParaType.Int:
					parameter = int.Parse(_endPara);
					break;
				case ParaType.Bool:
					parameter = bool.Parse(_endPara);
					break;
			}
			_gameObject.SendMessage(_func, parameter, SendMessageOptions.DontRequireReceiver);
		}
		if (_gameObject)
			_gameObject = null;
	}
}
