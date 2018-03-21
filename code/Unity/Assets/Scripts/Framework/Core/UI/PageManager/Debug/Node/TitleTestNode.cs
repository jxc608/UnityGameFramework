using UnityEngine;
using System.Collections;
using Snaplingo.UI;
using LitJson;
using UnityEngine.SceneManagement;

public class TitleTestNode : Node
{
	public override void Init(params object[] args)
	{
		base.Init(args);

		UIUtils.RegisterButton("Back", delegate {
			PageManager.Instance.OpenLastPage();
		}, transform);
	}
}
