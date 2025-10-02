using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "BTSShopItemV1", menuName = "BTS Shop/AAA_Globals/BTSShop Item V1") ]

public class BTSShopItemV1 : BTSShopItemV1Base
{
	public List<string> m_keys;
	public string m_ownedValue;
	public int m_cost;
	public string m_uiName;
	public string m_uiDescription;
	public Sprite m_uiSprite;

	public string m_assetPath;

	public override string Key
	{
		get { return m_keys[0]; }
		set { m_keys[0] = value; }
	}

	public override List<string> Keys
	{
		get { return m_keys; }
		set { m_keys = value; }
	}

	public override string OwnedValue
	{
		get { return m_ownedValue; }
		set { m_ownedValue = value; }
	}

	public override int Cost
	{
		get { return m_cost; }
		set { m_cost = value; }
	}

	public override string UIName
	{
		get { return m_uiName; }
		set { m_uiName = value; }
	}

	public override string UIDescription
	{
		get { return m_uiDescription; }
		set { m_uiDescription = value; }
	}

	public override Sprite UISprite
	{
		get { return m_uiSprite; }
		set { m_uiSprite = value; }
	}

		public override string AssetPath
	{
		get { return m_assetPath; }
		set { m_assetPath = value; }
	}

}