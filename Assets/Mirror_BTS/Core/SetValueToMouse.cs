using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetValueToMouse : MonoBehaviour
{
	[SerializeField] Vector2 m_XRange;
	[SerializeField] Vector2 m_YRange;

	Slider m_slider;

	Vector2 m_screen;

	// Start is called before the first frame update
	void Start()
	{
		m_slider = GetComponent<Slider>();

		if (Application.isMobilePlatform)
		{
			if (null != m_slider)
			{
				m_slider.interactable = false;
			}
		}
		else
		{
			this.enabled = false;
		}

		m_screen.x = Screen.width / 2;
		m_screen.y = Screen.height / 2;
	}

	// Update is called once per frame
	void Update()
	{
		if (Application.isMobilePlatform)
		{
			m_screen.x = Screen.width / 2;
			m_screen.y = Screen.height / 2;

			Vector2 l_mousePosition = (Input.mousePosition / m_screen) - Vector2.one;

			if (l_mousePosition.x > m_XRange.x && l_mousePosition.x < m_XRange.y)
			{
				if (l_mousePosition.y > m_YRange.x && l_mousePosition.y < m_YRange.y)
				{
					m_slider.value = Mathf.Clamp(l_mousePosition.y * 2, -1, 1);
				}
			}
		}
	}
}
