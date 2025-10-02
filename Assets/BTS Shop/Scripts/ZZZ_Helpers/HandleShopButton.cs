using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandleShopButton : MonoBehaviour
{
	[SerializeField] private Canvas m_disableCanvas;
	[SerializeField] Canvas m_enableCanvas;
	[SerializeField] Button m_selectButton;

	public Canvas DisableCanvas
	{
		get
		{
			return m_disableCanvas;
		}

		set
		{
			m_disableCanvas = value;
		}
	}

	public Canvas EnableCanvas
	{
		get
		{
			return m_enableCanvas;
		}

		set
		{
			m_enableCanvas = value;
		}
	}


	public Button SelectButton
	{
		get
		{
			return m_selectButton;
		}

		set
		{
			m_selectButton = value;
		}
	}

	public void OnCloseShop()
	{
		EnsureDisableCanvas();

		if (m_enableCanvas)
		{
			m_enableCanvas.gameObject.SetActive(true);
			m_enableCanvas.enabled = true;
		}

		if (m_disableCanvas)
		{
			m_disableCanvas.enabled = false;
			m_disableCanvas.gameObject.SetActive(false);
		}

		if (m_selectButton)
		{
			StartCoroutine(SelectShopButton(m_selectButton));
		}

	}


	public void OnOpenShop()
	{
		EnsureDisableCanvas();

		if (m_disableCanvas)
		{
			m_disableCanvas.enabled = false;
			m_disableCanvas.gameObject.SetActive(false);
		}

		if (m_enableCanvas)
		{
			m_enableCanvas.gameObject.SetActive(true);
			m_enableCanvas.enabled = true;
		}

		if (m_selectButton)
		{
			StartCoroutine(SelectShopButton(m_selectButton));
		}

	}

	IEnumerator SelectShopButton(Button a_button)
	{
		float l_frameTime = 1.0f / 30.0f;

		yield return new WaitForSecondsRealtime(l_frameTime);

		a_button.Select();
	}

	private void EnsureDisableCanvas()
	{
		if (m_disableCanvas == null)
		{
			// Try to find "Shop Canvas" specifically under this shop
			var shopRoot = transform.root.Find("Shop Canvas");
			if (shopRoot != null)
			{
				m_disableCanvas = shopRoot.GetComponent<Canvas>();
			}
		}
	}
}
