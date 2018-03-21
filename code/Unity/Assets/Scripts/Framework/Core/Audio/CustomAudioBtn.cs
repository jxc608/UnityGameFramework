using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class CustomAudioBtn : MonoBehaviour, IPointerClickHandler
{
	public string m_AudioType = "";

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(m_AudioType) && GetComponent<Button>() != null && GetComponent<Button>().interactable) {
			if (!AudioManager.Instance.IsSfxPlaying) {
				AudioManager.Instance.PlaySfx(m_AudioType);
			}
		}
	}

	public static CustomAudioBtn Get(GameObject go)
	{
		CustomAudioBtn cab = go.GetComponent<CustomAudioBtn>();
		if (cab == null) {
			cab = go.AddComponent<CustomAudioBtn>();
		}
		return cab;
	}
}
