using System.Collections;
using System.Collections.Generic;
using com.burningthumb;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SetTextColor : MonoBehaviour
{
	public bool m_useShopColors = true;
	public BTSShop m_BTSShop;
	public Color m_color;

	// Start is called before the first frame update
	void Start()
	{
		if (null == m_BTSShop)
		{
			m_BTSShop = FindFirstObjectByType<BTSShop>();
		}

		Text l_text = GetComponent<Text>();

		if (null != l_text)
		{
			if (m_useShopColors && (null != m_BTSShop) && (null != m_BTSShop.ShopTextColor))
			{
				l_text.color = m_BTSShop.ShopTextColor;
			}
			else
			{ 
				l_text.color = m_color;
			}
		}       
	}
}
