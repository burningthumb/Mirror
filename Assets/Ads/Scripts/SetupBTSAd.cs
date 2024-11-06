using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetupBTSAd : MonoBehaviour
{
	[SerializeField] SOBTSAd[] m_ads;

	[SerializeField] TMP_Text m_titleText;
	[SerializeField] TMP_Text m_subtitleText;

	[SerializeField] TMP_Text m_blurbText;

	[SerializeField] Image m_image;
	[SerializeField] BTSOpenWebSite m_btsOpenWebSite;

	// Start is called before the first frame update
	void Awake()
	{
		int l_choice = Random.Range(0, m_ads.Length);

		m_titleText.text = m_ads[l_choice].Title;
		m_subtitleText.text = m_ads[l_choice].Subtitle;
		m_blurbText.text = m_ads[l_choice].Blurb;

		m_image.sprite = m_ads[l_choice].AdSprite;

		BTSOpenWebSite.URL = m_ads[l_choice].URL;

	}


}
