using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BTSInteractableAfterTime : MonoBehaviour
{
    [SerializeField] float m_time = 30;
    [SerializeField] Button m_button;

	[SerializeField] TMP_Text m_hintText;

	string m_saveText;

	public void Start()
	{
		if (null == m_button)
		{
			m_button = GetComponent<Button>();
		}

		if (null != m_button)
		{
			m_button.interactable = false;
			//m_button.enabled = false;
		}

		if (null != m_hintText)
		{
			m_saveText = m_hintText.text;
		}

		Minus1();
	}

	public void Minus1()
	{
		m_time--;

		if (null != m_hintText)
		{
			m_hintText.text = $"{m_time} Seconds";
		}

		if (m_time > 0)
		{
			Invoke(nameof(Minus1), 1.0f);
			return;
		}

		if (null != m_button)
		{
			m_button.interactable = true;
			//m_button.enabled = true;
		}

		if (null != m_hintText)
		{
			m_hintText.text = m_saveText;
		}

	}
}
