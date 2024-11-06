using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSOpenWebSite : MonoBehaviour
{

	static string m_url;

	static public string URL
	{
		get
		{
			return m_url;
		}

		set
		{
			m_url = value;
		}
	}


    public void OpenURL(string a_urlIfNull)
	{
		if (null == m_url)
		{ 
			Application.OpenURL(a_urlIfNull);
		}
		else
		{
			Application.OpenURL(m_url);
		}
	}

}
