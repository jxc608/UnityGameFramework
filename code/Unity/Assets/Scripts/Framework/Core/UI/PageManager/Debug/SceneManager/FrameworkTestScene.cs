using UnityEngine;
using System.Collections;

public class FrameworkTestScene : MonoBehaviour
{
	void Start()
	{
		Snaplingo.UI.PageManager.Instance.OpenPage("FrameworkTestPage");
	}
}
