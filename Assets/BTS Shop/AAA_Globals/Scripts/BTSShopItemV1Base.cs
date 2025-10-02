using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BTSShopItemV1Base : ScriptableObject
{
	public abstract string Key
	{
		get;
		set;
	}

	public abstract List<string> Keys
	{
		get;
		set;
	}

   public abstract string OwnedValue
	{
		get;
		set;
	}

		public abstract int Cost
	{
		get;
		set;
	}

	public abstract string UIName
	{
		get;
		set;
	}

	public abstract string UIDescription
	{
		get;
		set;
	}

	public abstract Sprite UISprite
	{
		get;
		set;
	}

		public abstract string AssetPath
	{
		get;
		set;
	}
}
