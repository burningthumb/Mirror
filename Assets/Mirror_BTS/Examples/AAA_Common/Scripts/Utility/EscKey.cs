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

	[SerializeField] Button m_show;
	[SerializeField] Button m_hide;

	[SerializeField] Selectable m_selectThisOnShow;
	[SerializeField] GameObject m_savedSelectedGO;

	[SerializeField] bool m_lockCursor = true;

	// Start is called before the first frame update
	void Start()
	{

		Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false; // Show cursor

		//m_canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

		//if (!Application.isMobilePlatform)
		//{
		//foreach (Canvas l_canvas in m_canvases)
		//{
		//	l_canvas.gameObject.SetActive(false);
		//}
		//}

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
	}

	// Update is called once per frame
	void Update()
	{
		//if (!Application.isMobilePlatform)
		//{
		bool l_didToggleVisible = false;

		if (m_pauseIA.WasPressedThisFrame())
		{

			if (null != m_settingsPanel)
			{
				l_didToggleVisible = !m_settingsPanel.activeSelf;
			}
			//else if (null != m_nmui && null != m_nmui.PanelMain)
			//{
			//	l_didToggleVisible = !m_nmui.PanelMain.activeSelf;
			//}

			if (null != m_settingsPanel)
			{
				m_settingsPanel.SetActive(l_didToggleVisible);
			}

			//if (null != m_nmui && null != m_nmui.PanelMain)
			//{
			//	m_nmui.PanelMain.SetActive(l_didToggleVisible);
			//}

			//foreach (Canvas l_canvas in m_canvases)
			//{
			//	l_didToggleVisible = !l_canvas.gameObject.activeSelf;

			//	l_canvas.gameObject.SetActive(l_didToggleVisible);
			//}

			//if (null != m_showCanvasImage)
			//{
			//	m_showCanvasImage.gameObject.SetActive(!l_didToggleVisible);
			//}

			if (l_didToggleVisible)
			{
				Time.timeScale = 0.01f;
			}
			else
			{
				Time.timeScale = 1.0f;
			}

			StartCoroutine(SelectSelectable(l_didToggleVisible));
		}


		//}

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

//		Debug.Log($"{Time.time} a_makeVisible  {a_makeVisible} Cursor.lockState = {Cursor.lockState} Cursor.visible = {Cursor.visible}");

		//if (m_hide.gameObject.activeSelf)
		//{
		//	m_hide.Select();
		//}
		//else
		//{
		//	m_show.Select();
		//}
	}
}
