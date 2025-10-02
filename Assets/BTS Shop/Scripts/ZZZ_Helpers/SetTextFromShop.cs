using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.burningthumb
{ 
	public class SetTextFromShop : MonoBehaviour
	{
		public BTSShop.ShopKey m_shopKey;
		public StringReference m_keySR;

		private BTSShop m_BTSShop;
		private string m_lastKeySet;
		private Text m_text;

		// Start is called before the first frame update
		void Start()
		{
			if (null == m_BTSShop)
			{
				m_BTSShop = FindObjectOfType<BTSShop>();
			}

			if (null != m_BTSShop)
			{ 
				m_text = GetComponent<Text>();
				SetValueForKey();
			}
        
		}

		// Update is called once per frame
		void Update()
		{
			if (null == m_BTSShop)
			{
				m_BTSShop = FindObjectOfType<BTSShop>();
			}

			if (null != m_BTSShop)
			{ 
				if (m_lastKeySet != m_keySR.Value)
				{
					SetValueForKey();
				}
			}
		}

		void SetValueForKey()
		{
			m_lastKeySet = m_keySR.Value;

			switch (m_shopKey)
			{
				case BTSShop.ShopKey.kUIDescripton:
				m_text.text = m_BTSShop.UIDescription(m_keySR.Value);
				break;

				case BTSShop.ShopKey.kUIName:
				m_text.text = m_BTSShop.UIName(m_keySR.Value);
				break;

			}
		}
	}
}
