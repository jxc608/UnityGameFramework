using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIShowAnimation : MonoBehaviour
{
	public enum Type
	{
		Spring,
	}

	public Type _type = Type.Spring;

	void OnEnable()
	{
		switch (_type) {
			case Type.Spring:
				float duration = .05f;
				transform.DOScale(1.05f, duration).OnComplete(delegate {
					transform.DOScale(1f, duration).OnComplete(delegate {
						transform.DOScale(1.01f, duration).OnComplete(delegate {
							transform.DOScale(1f, duration);
						});
					});
				});
				break;
		}
	}

}
