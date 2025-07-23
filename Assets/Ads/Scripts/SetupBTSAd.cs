using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SetupBTSAd : MonoBehaviour
{
	[SerializeField] SOBTSAd[] m_ads;

	[SerializeField] TMP_Text m_titleText;
	[SerializeField] TMP_Text m_subtitleText;

	[SerializeField] TMP_Text m_blurbText;

	[SerializeField] Image m_image;
	[SerializeField] BTSOpenWebSite m_btsOpenWebSite;

	void OnEnable()
	{
		int l_choice = Random.Range(0, m_ads.Length);

		SOBTSAd l_ad = m_ads[l_choice];

		GA4Analytics.DisplayAd(l_ad);

		m_titleText.text = l_ad.Title;
		m_subtitleText.text = l_ad.Subtitle;
		m_blurbText.text = l_ad.Blurb;

		m_image.sprite = l_ad.AdSprite;

		BTSOpenWebSite.BTSAd = l_ad;
		BTSOpenWebSite.URL = $"{l_ad.URL}?utm_source={UnityWebRequest.EscapeURL(l_ad.GameID)}&utm_medium={UnityWebRequest.EscapeURL(l_ad.Medium)}&utm_campaign={UnityWebRequest.EscapeURL(l_ad.AdID)}";
	}
}
