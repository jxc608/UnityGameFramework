using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CheckTip : MonoBehaviour
{
	public Text desc;
	private PromptManager.OnReceiveMessageBoxResult messageBoxCallback = null;

	public void Init(string str, PromptManager.OnReceiveMessageBoxResult callback = null)
	{
		desc.text = str;
		messageBoxCallback = callback;
	}

	public void OnConfirm()
	{
		if (messageBoxCallback != null) {
			messageBoxCallback(PromptManager.Result.OK);
			messageBoxCallback = null;
		}
		Destroy(gameObject);
	}

	public void OnCancel()
	{
		if (messageBoxCallback != null) {
			messageBoxCallback(PromptManager.Result.Cancel);
			messageBoxCallback = null;
		}
		Destroy(gameObject);
	}
}
