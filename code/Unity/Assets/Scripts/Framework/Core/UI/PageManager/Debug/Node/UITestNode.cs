using UnityEngine;
using System.Collections;
using Snaplingo.UI;
using Snaplingo.Tutorial;
using LitJson;
using System;

public class UITestNode : Node
{
	public override void Init(params object[] args)
	{
		base.Init(args);

		UIUtils.RegisterButton("GeneralUITest", delegate {
			GeneralUITest();
		}, transform);

		UIUtils.RegisterButton("ShowHideTitle", delegate {
			ShowHideTitle();
		}, transform);

		var sound = transform.Find("SwitchSound").GetComponent<UnityEngine.UI.Toggle>();
		sound.isOn = AudioManager.Instance.Music;
		sound.onValueChanged.AddListener(delegate(bool status) {
			AudioManager.Instance.SetSound(status);
		});

		UIUtils.RegisterButton("UIRectTest", delegate {
			UIRectTest();
		}, transform);

		UIUtils.RegisterButton("DisplayTest", delegate {
			DisplayTest();
		}, transform);

		UIUtils.RegisterButton("DialogTest", delegate {
			DialogTest();
		}, transform);

	}

	void OnDestroy()
	{
		if (TutorialUIManager.HasInstance() && TutorialUIManager.Instance.HasDialog())
			TutorialUIManager.Instance.HideDialog();
	}

	void GeneralUITest()
	{
		PromptManager.Instance.MessageBox(PromptManager.Type.CheckTip, "Check Tip");
		PromptManager.Instance.MessageBox(PromptManager.Type.FloatingTip, "Floating Tip");
	}

	void ShowHideTitle()
	{
		var title = ParentPage.GetNode<TitleTestNode>();
		if (title != null) {
			if (title.IsShown())
				title.Close();
			else
				title.Open();
		}
	}

	void UIRectTest()
	{
		if (TutorialUIManager.Instance.HasFocusMasks())
			TutorialUIManager.Instance.HideFocusMasks();
		else {
			var rectTransButton = transform.Find("UIRectTest").transform as RectTransform;
			TutorialUIManager.Instance.ShowFocusMasks(rectTransButton, RootCanvas);
		}
	}

	void DisplayTest()
	{
		if (TutorialUIManager.Instance.HasPicture())
			TutorialUIManager.Instance.HidePicture();
		else
			TutorialUIManager.Instance.ShowPicture("TestPic", true, Vector2.zero, 1.3f);
	}

	void DialogTest()
	{
		if (TutorialUIManager.Instance.HasDialog())
			TutorialUIManager.Instance.HideDialog();
		else {
			var pos = (TutorialUIManager.CharacterPos)UnityEngine.Random.Range(0, (int)TutorialUIManager.CharacterPos.Count);
			TutorialUIManager.Instance.ShowDialog(TutorialUIManager.DialogType.Anywhere, pos, TutorialUIManager.CharacterType.TipBall2,
			                                       "我是一个兵我是一个兵我是一个兵我是我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个我是我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个我是我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个我是我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个我是我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个我是我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个我是我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵我是一个兵"
				, true, new Rect(0.1f, 0.3f, 0.4f, 0.6f), 1);
		}
	}

}
