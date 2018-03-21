using System;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteLoading : MonoBehaviour
{
	private Image m_Image;
	private SpriteRenderer m_Sprite;
	void Start ()
	{
		m_Image = transform.GetComponent<Image>();
		m_Sprite = transform.GetComponent<SpriteRenderer>();
	}
	void Update ()
	{
		m_Image.sprite = m_Sprite.sprite;
	}
}