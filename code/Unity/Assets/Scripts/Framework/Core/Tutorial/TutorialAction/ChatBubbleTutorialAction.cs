using UnityEngine;
using Snaplingo.Tutorial;

public class ChatBubbleTutorialAction : TutorialAction
{
	[JsonAttribute("rect", "ada", "区域(屏幕百分比)")]
	public Rect _panelRect = new Rect();
	[JsonAttribute("snd", "", "语音")]
	public int _soundId = 0;
	[JsonAttribute("cpos", "", "箭头位置")]
	public TutorialUIManager.ChatBubbleArrowPos _charPos = TutorialUIManager.ChatBubbleArrowPos.Left;
	[JsonAttribute("tId", "", "文本")]
	public int _textId = 0;
	[JsonAttribute("ada", "", "是否适配")]
	public bool _adaptation = true;
	[JsonAttribute("delSize", "!ada", "中心为起点的位置和长宽")]
	public Vector4 _deltaSize = new Vector4();

	protected override void Init()
	{
		TutorialUIManager.Instance.ShowChatBubble(_charPos, LocalizationConfig.Instance.GetStringById(_textId).Replace("<name>", PlayerInfo.Instance.Nickname), _panelRect, _deltaSize, _adaptation);
		if (_soundId != 0)
			AudioManager.Instance.PlaySfx(_soundId);
	}

	protected override void Cancel()
	{
		TutorialUIManager.Instance.HideChatBubble();
		if (_soundId != 0)
			AudioManager.Instance.StopSfx();
	}

	#if UNITY_EDITOR
	#pragma warning disable 0414
	[JsonEditorAttribute(JsonEditorAttribute.SpecialType.SoundIndex, "snd")]
	int _soundIndex = 0;
	#pragma warning restore
	#endif
}
