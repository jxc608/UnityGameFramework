using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class FloatingTip : MonoBehaviour
{
	Tweener tweener;
	public Text Desc;
	public Image BG;
	private PromptManager.OnReceiveMessageBoxResult messageBoxCallback = null;

	public void Init(string str, PromptManager.OnReceiveMessageBoxResult callBack)
	{
		messageBoxCallback = callBack;
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		Desc.text = str;
		startTime = true;
	}

	public void Close()
	{
		if (messageBoxCallback != null) {
			messageBoxCallback(PromptManager.Result.OK);
			messageBoxCallback = null;
		}
		Destroy(gameObject);
	}

	bool startTime;
	bool isAnim;
	float speed = 0.05f;
	float timer;

	void Update()
	{
		if (startTime) {
			timer += Time.unscaledDeltaTime;

			if (timer > 1.5f) {
				tweener = BG.DOColor(new Color(0f, 0f, 0f, 0f), 1f);
				tweener.OnComplete<Tweener>(delegate () {
					Close();
				});
				isAnim = true;
				timer = 0;
				startTime = false;
			}
		}
		if (isAnim) {
			Desc.color -= new Color(0, 0, 0, speed);
			if (Desc.color.a < 0)
				isAnim = false;
		}
	}
}
