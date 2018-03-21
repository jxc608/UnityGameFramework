using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Snaplingo.Config;

public class TextByID : MonoBehaviour
{
	public int ID;

	void Start()
	{
		if (transform.GetComponent<Text>() != null && !Equals(ID, 0)) {
			transform.GetComponent<Text>().text = LocalizationConfig.Instance.GetStringById(ID);
		}
	}
}
