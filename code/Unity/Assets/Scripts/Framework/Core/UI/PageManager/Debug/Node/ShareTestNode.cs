using UnityEngine;
using System.Collections;
using Snaplingo.UI;
using LitJson;

public class ShareTestNode : Node
{
	public override void Init(params object[] args)
	{
		base.Init(args);

		UIUtils.RegisterButton("WechatSessionImage", delegate {
			WechatSessionImage();
		}, transform);

		UIUtils.RegisterButton("WechatTimeLineImage", delegate {
			WechatTimeLineImage();
		}, transform);

		UIUtils.RegisterButton("TestPage", delegate {
			TestPage();
		}, transform);
	}

	void WechatSessionImage()
	{
		StartCoroutine(ShareImage(ShareManager.WXScene.WXSceneSession));
	}

	void WechatTimeLineImage()
	{
		StartCoroutine(ShareImage(ShareManager.WXScene.WXSceneTimeline));
	}

	IEnumerator ShareImage(ShareManager.WXScene scene)
	{
		if (!ShareManager.Instance.IsWechatInstalled()) {
			PromptManager.Instance.MessageBox(PromptManager.Type.FloatingTip, "未安装微信");
			yield break;
		}

		yield return new WaitForEndOfFrame();

		// Create a texture the size of the screen, RGB24 format
		int width = Screen.width;
		int height = Screen.height;
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

		// Read screen contents into the texture
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		tex.Apply();

		// Encode texture into PNG
		byte[] bytes = tex.EncodeToJPG();

		var texScaled = MiscUtils.ScaleTextureBilinear(tex, .1f);
		byte[] bytesScaled = texScaled.EncodeToJPG();

		ShareManager.Instance.ShareImageWechat(scene, bytes, bytesScaled, delegate(ShareManager.WechatErrCode code) {
			// callback code
		});

		Destroy(tex);
		Destroy(texScaled);
	}

	void TestPage()
	{
		PageManager.Instance.OpenPage("LevelMap");
	}

}

