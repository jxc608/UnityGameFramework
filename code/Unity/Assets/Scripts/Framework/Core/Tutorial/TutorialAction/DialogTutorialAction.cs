using UnityEngine;
using Snaplingo.Tutorial;
using Snaplingo.UI;

public class DialogTutorialAction : TutorialAction
{
	[JsonAttribute("rect", "acty", "区域(屏幕百分比)")]
	public Rect _panelRect = new Rect();
	[JsonAttribute("char", "", "提宝")]
	public TutorialUIManager.CharacterType _character = TutorialUIManager.CharacterType.TipBall1;
	[JsonAttribute("snd", "", "语音")]
	public int _soundId = 0;
	[JsonAttribute("cont", "acty", "显示点击继续")]
	public bool _isClickContinue = true;
	[JsonAttribute("acty", "", "自定义面板")]
	public bool _activityPanel = true;
	[JsonAttribute("cpos", "acty", "提宝位置")]
	public TutorialUIManager.CharacterPos _charPos = TutorialUIManager.CharacterPos.Left;
	[JsonAttribute("tId", "", "文本")]
	public int _textId = 0;
	[JsonAttribute("hdtb", "acty", "隐藏提宝")]
	public bool _hideTipball = false;

	protected override void Init()
	{
		TutorialUIManager.Instance.ShowDialog(_activityPanel ? TutorialUIManager.DialogType.Anywhere : TutorialUIManager.DialogType.Bottom, 
		                                      _charPos, _character, LocalizationConfig.Instance.GetStringById(_textId).Replace("<name>", PlayerInfo.Instance.Nickname), _isClickContinue, 
		                                       _panelRect, 0, _hideTipball);
		if (_soundId != 0)
			AudioManager.Instance.PlaySfx(_soundId);
	}

	protected override void Cancel()
	{
		TutorialUIManager.Instance.HideDialog();
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
