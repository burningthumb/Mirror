using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.burningthumb
{
	public class PurchaseDispatcher : MonoBehaviour
	{
		public BTSShop m_BTSShop;

		public GameObject m_trackButtons;
		public ProductOwnershipHandler[] m_productOwnershipHandler;
	 
		public StringReference m_selectedLevelKey;

		public UnityEvent m_buyCoin;
		public UnityEvent m_onSetInteractable;
		public UnityEvent m_TrackPurchased;

		public void Start ()
		{
			if (null == m_BTSShop)
			{
				m_BTSShop = FindObjectOfType<BTSShop>();
			}

			SetInteractableState();

		}

		public void SetInteractableState()
		{
			bool l_found = false;
			Button l_button = GetComponent<Button>();

			m_productOwnershipHandler = m_trackButtons.GetComponentsInChildren<ProductOwnershipHandler>();

			string l_label = "Buy Track";

			if (0 == m_BTSShop.Coins)
			{
				l_label = "Buy Coins";
				l_button.interactable = true;
			}
			else
			{
				foreach (ProductOwnershipHandler l_poh in m_productOwnershipHandler)
				{
					if (l_poh.m_key == m_selectedLevelKey.Value)
					{

						if (m_BTSShop.IsOwned(l_poh.m_key))
						{
							l_button.interactable = false;
						}
						else
						{
							if (m_BTSShop.Cost(l_poh.m_key) > m_BTSShop.Coins)
							{
								l_label = "Buy Coins";
							}

							l_button.interactable = true;
							// StartCoroutine(SelectButton(l_button));
						}

						l_found = true;
						break;
					}

				}

				if (!l_found)
				{
					l_label = "Owned";
					l_button.interactable = false;
				}
			}

			l_button.GetComponentInChildren<Text>().text = l_label;
			m_onSetInteractable.Invoke();
		}

		//IEnumerator SelectButton(Button a_button)
		//{
		//	yield return new WaitForEndOfFrame();

		//	Debug.Log(a_button.name);

		//	if (null != a_button) { 
		//	EventSystem.current.SetSelectedGameObject(null);
		//	a_button.Select();
		//	}
		//}

		public void DispatchEvents()
	        {

			int l_coins = m_BTSShop.Coins;

			if ((0 == l_coins) || (l_coins < m_BTSShop.Cost(m_selectedLevelKey.Value)))
			{ 
				m_buyCoin.Invoke();
			}
			else
			{
				foreach (ProductOwnershipHandler l_poh in m_productOwnershipHandler)
				{
					if (l_poh.m_key == m_selectedLevelKey.Value)
					{
						l_poh.BuyTrack();

						// Do not move this line of code outside of this else condition
						// If you do bad things will happen
						SetInteractableState();

						m_TrackPurchased.Invoke();
						break;
					}

				}


			}
		}

	}
}
