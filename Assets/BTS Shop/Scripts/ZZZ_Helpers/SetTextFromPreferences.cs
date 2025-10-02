using System.Collections;
using System.Collections.Generic;
using com.burningthumb;
using UnityEngine;
using UnityEngine.UI;

public class SetTextFromPreferences : MonoBehaviour
{
	public enum PrefType { kString, kInt, kPasswordString}

	public PrefType m_preferenceType;
	public bool m_disableIfNotEmpty = false;
	public Button m_relatedButton;
	public string m_preferencesKey = "kUnknown";
	public string m_formatString = "{0}";
	Text m_text;
	InputField m_inputField;

	string m_realKey;

	public void Start ()
	{
		ShowValue();
	}

	public void OnEnable ()
	{
		ShowValue();
	}

	public void ShowValue()
	{
		m_text = GetComponent<Text>();
		m_inputField = GetComponent<InputField>();

		if (!SecurePlayerPrefs.isInitialized())
		{ 
			SecurePlayerPrefs.Init();
		}

		m_realKey = BTSShopKey.GetRealKey(m_preferencesKey);

		string l_value;

		switch (m_preferenceType)
		{
			case PrefType.kInt:	
				l_value = string.Format(m_formatString,SecurePlayerPrefs.GetInt(m_realKey));
				break;

			default:
				l_value = string.Format(m_formatString,SecurePlayerPrefs.GetString(m_realKey));
				break;
		}

		if (null != m_inputField)
		{
			m_inputField.text = l_value;

			if ((m_disableIfNotEmpty) && (l_value.Length > 0))
			{
				m_inputField.interactable = false;
				if (null != m_relatedButton)
				{
					m_relatedButton.interactable = false;
				}
			}
		}

		if (null != m_text)
		{
			m_text.text = l_value;
		}

	}

	public void SaveValue()
	{
		string l_value = m_inputField.text;

		if (m_preferenceType == PrefType.kPasswordString)
		{
			l_value = Sha512Helper.GetSHA512(l_value);
		}

		SecurePlayerPrefs.SetString(m_realKey, l_value);
		SecurePlayerPrefs.Save();

	}
}
