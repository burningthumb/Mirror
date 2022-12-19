using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoredValue : MonoBehaviour
{
	public enum StoredValueKey { kUnknown, kOrthographicSize }
	public enum StoredValueKind { kUnknown, kFloat }
	public enum SelectableKind { kUnknown, kSlider }

	[SerializeField] StoredValueKey m_svKey;
	[SerializeField] StoredValueKind m_svKind;
	[SerializeField] SelectableKind m_selectableKind;

	[SerializeField] float m_floatDefault;
	[SerializeField] float m_floatValue;

	[SerializeField] Selectable m_uiSelectable;

	void Start()
	{
		m_uiSelectable = GetComponent<Selectable>();

		switch (m_svKind)
		{
			case StoredValueKind.kFloat:
				m_floatValue = PlayerPrefs.GetFloat(m_svKey.ToString(), m_floatDefault);
				break;

			default:
				m_floatValue = m_floatDefault;
				break;
		}

		if (null != m_uiSelectable)
		{
			switch (m_selectableKind)
			{
				case SelectableKind.kSlider:
					((Slider)m_uiSelectable).value = m_floatValue;
					break;

				default:
					break;
			}

		}
	}

	public void SaveValue()
	{
		switch (m_selectableKind)
		{
			case SelectableKind.kSlider:
				m_floatValue = ((Slider)m_uiSelectable).value;
				break;

			default:
				break;
		}

		switch (m_svKind)
		{
			case StoredValueKind.kFloat:
				 PlayerPrefs.SetFloat(m_svKey.ToString(), m_floatValue);
				break;

			default:
				m_floatValue = m_floatDefault;
				break;
		}

		PlayerPrefs.Save();
	}
}
