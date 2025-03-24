using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New SOBTSAd", menuName = "SO/BTS Ad")]
public class SOBTSAd : ScriptableObject
{
    public enum GAMEID { none, ad2442, canyon_run, coconut_hut, idrone, karts, mars_2055, net_pong, net_tanks}
    public enum MEDIUM { affiliate}
    public enum ADID   { none, ad2442, canyon_run, coconut_hut, drive_download, idrone, karts, mars_2055, mireth_music, net_pong, netshred, net_tanks, shredit, videokiosk}
    public enum ADVARIANT { _01 }

    public enum ADDITIONALPLATFORMS { AndroidTV, FireOS}

    [SerializeField] string m_title = "Ad Title";
    [SerializeField] string m_subtitle = "Ad Subtitle";
    [TextArea(1, 4)] [SerializeField] string m_blurb = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

    [SerializeField] Sprite m_adSprite;
    [SerializeField] string m_url = "https://www.domain.com/";

    [SerializeField] GAMEID m_gameID = GAMEID.none;
    [SerializeField] MEDIUM m_medium = MEDIUM.affiliate;
    [SerializeField] ADID m_adID = ADID.none;
    [SerializeField] ADVARIANT m_adVariant = ADVARIANT._01;

    public string Title
    {
        get
        {
            return m_title;
        }
    }

    public string Subtitle
    {
        get
        {
            return m_subtitle;
        }
    }

    public string Blurb
    {
        get
        {
            return m_blurb;
        }
    }

    public Sprite AdSprite
    {
        get
        {
            return m_adSprite;
        }
    }

    public string URL
    {
        get
        {
            return m_url;
        }
    }

    public string GameID
	{
        get
		{
            return m_gameID.ToString();
		}
	}

    public string Medium
	{
		get
		{
            return m_medium.ToString();
		}
	}

    public string AdID
	{
        get
		{
            return $"{m_adID.ToString()}{m_adVariant.ToString()}";
        }

    }

    public string Platform
	{
        get
		{
            string l_result = Application.platform.ToString();

            if (RuntimePlatform.Android == Application.platform)
			{
                if (BTS_TV_Type.IsAmazonFireTV)
				{
                    l_result = ADDITIONALPLATFORMS.FireOS.ToString();
				}
                else if (BTS_TV_Type.IsAndroidTV)
				{
                    l_result = ADDITIONALPLATFORMS.AndroidTV.ToString();
				}
			}

            return l_result;
		}
	}

    
}
