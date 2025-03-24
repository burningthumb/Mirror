using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BTSOpenWebSite : MonoBehaviour
{
	static SOBTSAd m_btsAd;
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

	static public SOBTSAd BTSAd
	{
		get
		{
			return m_btsAd;
		}

		set
		{
			m_btsAd = value;
		}
	}

    public void OpenURL(string a_defaultURL)
	{
		if (null != BTSAd)
        {
			string l_url = $"{BTSAd.URL}?utm_source={UnityWebRequest.EscapeURL(BTSAd.GameID)}&utm_medium={UnityWebRequest.EscapeURL(BTSAd.Medium)}&utm_campaign={UnityWebRequest.EscapeURL(BTSAd.AdID)}";

			Application.OpenURL(l_url);

			GA4Analytics.ClickAd(BTSAd);
        }
		else if (null != URL)
		{
			Application.OpenURL(URL);
		}
		else
		{
			Application.OpenURL(a_defaultURL);
		}
	}

}
