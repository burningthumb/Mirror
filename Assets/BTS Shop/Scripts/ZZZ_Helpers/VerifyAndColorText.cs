using System.Collections;
using System.Collections.Generic;
using com.burningthumb;
using UnityEngine;
using UnityEngine.UI;

public class VerifyAndColorText : MonoBehaviour
{
	public BTSShop m_btsShop;
	public Button m_button;

	public int m_minLength = 0;

	public Text m_text1;
	public Text m_text2;
	public Color m_sameColor;
	public Color m_differentColor;


	// Update is called once per frame
	void Update()
	{
		if (!m_btsShop.Online)
		{
			Verify();
		}
	}

	public void Verify()
	{
		if ((null != m_text1) && (null != m_text2))
		{
			if ((m_text1.text == m_text2.text) &&
				(m_text1.text.Length >= m_minLength))
			{
				if (m_text1.color != m_sameColor)
				{
					m_text1.color = m_sameColor;
				}

				if (m_text2.color != m_sameColor)
				{
					m_text2.color = m_sameColor;
				}

				if (null != m_button)
				{
					if (!m_button.interactable)
					{
						m_button.interactable = true;
					}
				}
			}
			else
			{
				if (m_text1.color != m_differentColor)
				{
					m_text1.color = m_differentColor;
				}

				if (m_text2.color != m_differentColor)
				{
					m_text2.color = m_differentColor;
				}

				if (null != m_button)
				{
					if (m_button.interactable)
					{
						m_button.interactable = false;
					}
				}
			}

		}
	}
}
