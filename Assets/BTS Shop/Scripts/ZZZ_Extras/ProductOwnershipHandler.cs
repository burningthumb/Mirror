using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.burningthumb
{

	public class ProductOwnershipHandler : MonoBehaviour
	{

		public BTSShop m_BTSShop;
		public Image m_purchasedImage;

		public string m_key = "kPleaseSupportUs1";

		public string m_newText = "Purchased";
		public Color m_newColor;

		public UnityEvent m_owned;

		// Start is called before the first frame update
		void Start()
		{
			if (null == m_BTSShop)
			{
				m_BTSShop = FindObjectOfType<BTSShop>();
			}

			if (null != m_BTSShop)
			{
				ShowCost();
			}

		}

		public void ShowCost()
		{
			Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Danger Danger Danger");
			Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> This is obsolete. Look at the OK button.");
			Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> This is obsolete. The behaviour is PurchaseOrSelect");
			Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Danger Danger Danger");

			Text l_textField = GetComponent<Text>();

			if (null != l_textField)
			{
				if (m_BTSShop.IsOwned(m_key))
				{
					l_textField.text = m_newText;
					if (m_purchasedImage)
					{
						m_purchasedImage.color = m_newColor;
					}
					m_owned.Invoke();
				}
				else
				{
					if (0 == m_BTSShop.Cost(m_key))
					{
						l_textField.text = "Free";
					}
					else
					{
						l_textField.text = m_BTSShop.Cost(m_key) + " Coins";
					}

				}
			}
		}

		public void BuyTrack()
		{

			if (m_BTSShop.Coins >= m_BTSShop.Cost(m_key))
			{
				m_BTSShop.Coins -= m_BTSShop.Cost(m_key);

				m_BTSShop.SetLocalAndRemoteStringValue(m_key, m_BTSShop.OwnedValue(m_key));

				ShowCost();
			}
		}
	}
}
