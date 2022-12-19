using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValue : MonoBehaviour
{
	Text m_text;

	// Start is called before the first frame update
	void Start()
	{
		m_text = GetComponent<Text>();
	}

	// Update is called once per frame
	public void SetSliderText(float a_value)
	{
		if (null != m_text)
		{
			m_text.text = a_value.ToString();
		}
	}
}
