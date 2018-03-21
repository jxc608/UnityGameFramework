using UnityEngine;
using LitJson;

public class SelectScene : MonoBehaviour
{
	void Start()
	{
		Snaplingo.UI.PageManager.Instance.OpenPage("SelectScenePage");
	}
}
