using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.burningthumb
{
	public class BTSShopEvent : MonoBehaviour
	{
		public enum BTSShopEventType { kNone, kNewAccount, kNewAccountError, kAccountFound, kAccountNotFound, kAccountInvalid, kNetworkError, kCoinSet, kAccountDeleted, kAccountDeletedError };

		public BTSShopEventType m_eventType;

		public bool m_interactable = false;
		public bool m_selected = false;
		public bool m_textEnabled = false;
		public bool m_setPreferenceText = false;
		public bool m_setScriptText = false;
		public string m_scriptText;
		public bool m_clearInput = false;

		Selectable m_selectable;
		Text m_text;
		SetTextFromPreferences m_setTextFromPreferences;

		public void OnEnable()
		 {
			m_selectable = GetComponent<Selectable>();
			m_setTextFromPreferences = GetComponent<SetTextFromPreferences>();
			m_text = GetComponent<Text>();

			switch (m_eventType)
			{
				case BTSShopEventType.kNetworkError:
					BTSShop.onNetworkError += HandleNetworkEvent;
					break;

				case BTSShopEventType.kAccountFound:
					BTSShop.onAccountFound += HandleEvent;
					break;

				case BTSShopEventType.kAccountNotFound:
					BTSShop.onAccountNotFound += HandleEvent;
					break;

				case BTSShopEventType.kNewAccount:
					BTSShop.onNewAccount += HandleEvent;
					break;

				case BTSShopEventType.kNewAccountError:
					BTSShop.onNewAccountError += HandleEvent;
					break;

				case BTSShopEventType.kAccountInvalid:
					BTSShop.onAccountInvalid += HandleEvent;
					break;

				case BTSShopEventType.kCoinSet:
					BTSShop.onCoinSet += HandleEvent;
					break;

				case BTSShopEventType.kAccountDeleted:
					BTSShop.onDeleteAccount += HandleEvent;
					break;

				case BTSShopEventType.kAccountDeletedError:
					BTSShop.onDeleteAccountError += HandleEvent;
					break;

				default:
					Debug.Log("Unsupported BTSShop Event Type.  Please migrage this item");
					break;
			}
        
		 }

		public void OnDisable()
		{

			switch (m_eventType)
			{
				case BTSShopEventType.kNetworkError:
					BTSShop.onNetworkError -= HandleNetworkEvent;
					break;

				case BTSShopEventType.kAccountFound:
					BTSShop.onAccountFound -= HandleEvent;
					break;

				case BTSShopEventType.kNewAccount:
					BTSShop.onNewAccount -= HandleEvent;
					break;

				case BTSShopEventType.kNewAccountError:
					BTSShop.onNewAccountError -= HandleEvent;
					break;

				case BTSShopEventType.kAccountNotFound:
					BTSShop.onAccountNotFound -= HandleEvent;
					break;

				case BTSShopEventType.kAccountInvalid:
					BTSShop.onAccountInvalid -= HandleEvent;
					break;

				case BTSShopEventType.kCoinSet:
					BTSShop.onCoinSet -= HandleEvent;
					break;

				case BTSShopEventType.kAccountDeleted:
					BTSShop.onDeleteAccount -= HandleEvent;
					break;

				case BTSShopEventType.kAccountDeletedError:
					BTSShop.onDeleteAccountError -= HandleEvent;
					break;

				default:
					Debug.Log("Unsupported BTSShop Event Type.  Please migrage this item");
					break;
			}
		}

		public void HandleNetworkEvent()
		{
			HandleEvent();
			m_text.text = m_text.text + "\n(" +  BTSShop.LastNetworkError + ")";

		}

		public void HandleEvent()
		{
			SetInteractable(m_interactable);

			if (m_setPreferenceText)
			{
				SetTextPreferenceText();
			}

			if (m_setScriptText)
			{
				SetTextScript();
			}

			SetTextEnabled(m_textEnabled);

			// Only select the item if the canvas is active
			Canvas l_canvas = GetComponentInParent<Canvas>();

			if (null != l_canvas && l_canvas.enabled)
			{ 
				SetSelected(m_selected);
			}

			if (m_clearInput)
			{
				ClearInputField();
			}
		}

		public void ClearInputField()
		{
			((InputField)m_selectable).text = "";
		}

		public void SetInteractable(bool a_bool)
		{
			if (null != m_selectable)
			{
				m_selectable.interactable = a_bool;
			}
		}

		public void SetSelected(bool a_bool)
		{
			if (null != m_selectable)
			{
				if (a_bool)
				{
					m_selectable.Select();
				}
			}
		}

		public void SetTextEnabled(bool a_bool)
		{
			if (null != m_text)
			{
				m_text.enabled = a_bool;
			}
		}

		public void SetTextPreferenceText()
		{
			if (null != m_setTextFromPreferences)
			{
				m_setTextFromPreferences.ShowValue();
			}
		}

		public void SetTextScript()
		{
		    if (null != m_text)
		    {
				m_textEnabled = true;
				m_text.enabled = m_textEnabled;
				m_text.text = m_scriptText;
		    }
        
		}

	}
}
