using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.burningthumb
{ 
	public class SetSpriteFromShop : MonoBehaviour
	{
		public BTSShop.ShopKey m_shopKey;
		public StringReference m_keySR;

		private BTSShop m_BTSShop;
		private string m_lastKeySet;
		private Image m_image;

		// Start is called before the first frame update
		void Start()
		{
			if (null == m_BTSShop)
			{
				m_BTSShop = FindObjectOfType<BTSShop>();
			}

			if (null != m_BTSShop)
			{
				m_image = GetComponent<Image>();
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
				case BTSShop.ShopKey.kUISprite:
				m_image.sprite = m_BTSShop.UISprite(m_keySR.Value);
				break;

			}
		}
	}
}
