using UnityEngine;
using System.Collections;
using Snaplingo.UI;
using LitJson;

public class SelectScenePageNode : Node
{
	public override void Init(params object[] args)
	{
		base.Init(args);

		UIUtils.RegisterButton("FrameworkTest", delegate {
			LoadSceneManager.Instance.LoadSceneAsyncAndOpenPage("FrameworkTestScene", "FrameworkTestPage");
		}, transform);

		UIUtils.RegisterButton("Play1", delegate {
		}, transform);

		UIUtils.RegisterButton("Play2", delegate {
		}, transform);

		UIUtils.RegisterButton("Play3", delegate {
		}, transform);

		UIUtils.RegisterButton("Explore", delegate {
		}, transform);

		UIUtils.RegisterButton("Module", delegate {
			LoadSceneManager.Instance.LoadSceneAsyncAndOpenPage("Module", "ModulePage");
		}, transform);
	}
}
