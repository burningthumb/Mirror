using System.Collections;
using System.Collections.Generic;
using com.burningthumb;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SetButtonColorBlock : MonoBehaviour
{
	public bool m_useShopColors = true;
	public BTSShop m_BTSShop;
	public ColorBlock m_colorBlock;

	// Start is called before the first frame update
	void Start()
	{
		if (null == m_BTSShop)
		{
			m_BTSShop = FindObjectOfType<BTSShop>();
		}

		Selectable l_button = GetComponent<Selectable>();

		if (null != l_button)
		{
			if (m_useShopColors)
			{
				l_button.colors = m_BTSShop.ShopColors;
			}
			else
			{
				l_button.colors = m_colorBlock;
			}
		}
	}
}
