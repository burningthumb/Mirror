using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EscKey : MonoBehaviour
{
	[SerializeField] InputAction m_pauseIA;

	[SerializeField] Image m_showCanvasImage;

	[SerializeField] bool m_hideNMUI = false;
	[SerializeField] NetworkManagerUI m_nmui;

	[SerializeField] GameObject m_settingsPanel;

	[SerializeField] Selectable m_selectThisOnShow;
	[SerializeField] GameObject m_savedSelectedGO;

	[SerializeField] bool m_lockCursor = true;

	// Start is called before the first frame update
	void Start()
	{

		Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false; // Show cursor

		m_nmui = FindFirstObjectByType<NetworkManagerUI>();

		if (null != m_nmui && null != m_nmui.PanelMain)
		{
			if (m_hideNMUI)
			{
				m_nmui.PanelMain.SetActive(false);
			}
		}

		if (null != m_settingsPanel)
		{
			m_settingsPanel.SetActive(false);
		}

		if (null != m_showCanvasImage)
		{
			m_showCanvasImage.gameObject.SetActive(true);
		}

	}

	void OnEnable()
	{
		m_pauseIA.Enable();

		Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false; // Show cursor
	}

	void OnDisable()
	{
		m_pauseIA.Disable();

		Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; // Hide cursor

		Time.timeScale = 1.0f;
	}

	public void ShowMenu()
    {
		Debug.Log($"EscKey::ShowMenu {m_settingsPanel.activeSelf}");

		if (!m_settingsPanel.activeSelf)
        {
			ToggleMenu(false);
        }
    }

	// Update is called once per frame
	void Update()
	{
		if (m_pauseIA.WasPressedThisFrame())
        {
            ToggleMenu(true);
        }
    }

    private void ToggleMenu(bool a_andTimescale)
    {
		Debug.Log($"{Time.time} ToggleMenu: {a_andTimescale}");

        bool l_didToggleVisible = false;

        if (null != m_settingsPanel)
        {
            l_didToggleVisible = !m_settingsPanel.activeSelf;
        }

        if (null != m_settingsPanel)
        {
            m_settingsPanel.SetActive(l_didToggleVisible);
        }

		StopAllCoroutines();
        if (a_andTimescale) StartCoroutine(ToggleTimescale(l_didToggleVisible));
        StartCoroutine(SelectSelectable(l_didToggleVisible));
    }

	IEnumerator ToggleTimescale(bool a_makeVisible)
	{
		yield return new WaitForSecondsRealtime(0.5f);

		if (a_makeVisible)
        {
            Time.timeScale = 0.001f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
	}

    IEnumerator SelectSelectable(bool a_makeVisible)
	{
		yield return new WaitForEndOfFrame();

		if (a_makeVisible)
		{
			m_savedSelectedGO = EventSystem.current.currentSelectedGameObject;
			m_selectThisOnShow.Select();
		}
		else
		{
			if (null != m_savedSelectedGO)
			{
				Selectable l_selectable = m_savedSelectedGO.GetComponent<Selectable>();
				if (null != l_selectable)
				{
					l_selectable.Select();
				}

			}
		}

		yield return new WaitForEndOfFrame();

		if (a_makeVisible)
		{
			Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; // Show cursor
		}
		else
		{
			Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false; // Hide cursor
		}
	}
}
