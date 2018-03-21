using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BundleBugManager : GeneralUIManager
{
	static BundleBugManager _instance;

	[RuntimeInitializeOnLoadMethod]
	static void InitializeOnLoad()
	{
		_instance = Instance;
	}

	public static BundleBugManager Instance {
		get {
			if (_instance == null) {
				GameObject go = Instantiate(ResourceLoadUtils.Load<GameObject>("Framework/Core/UI/BundleBug/BundleBugManager"));
				_instance = go.GetComponent<BundleBugManager>();
				go.name = _instance.GetType().ToString();
				_instance.AddToUIManager();
				_instance.transform.Find("Btn").gameObject.SetActive(false);
				_instance.SetPriority(Priority.Lowest);
				_instance.Init();
				_instance.StartCoroutine(_instance.CloseUI());
			}
			return _instance;
		}
	}

	IEnumerator CloseUI()
	{
		yield return null;
		_instance.transform.Find("Btn").gameObject.SetActive(false);
	}

	void OnDestory()
	{
		_instance = null;
	}

	void Init()
	{
		_instance.transform.Find("Btn").gameObject.SetActive(true);
	}
}