using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New SOBTSAd", menuName = "SO/BTS Ad")]
public class SOBTSAd : ScriptableObject
{
    [System.Serializable]
    public class PlatformAlternate
    {
        public RuntimePlatform[] m_platforms;
        public SOBTSAd m_btsAd;
    }

    public enum GAMEID { none, ad2442, canyon_run, coconut_hut, idrone, karts, mars_2055, net_pong, net_tanks }
    public enum MEDIUM { affiliate }
    public enum ADID { none, ad2442, canyon_run, coconut_hut, drive_download, idrone, karts, mars_2055, mireth_music, net_pong, netshred, net_tanks, shredit, videokiosk }
    public enum ADVARIANT { _01 }
    public enum ADDITIONALPLATFORMS { AndroidTV, FireOS }

    [SerializeField] string m_title = "Ad Title";
    [SerializeField] string m_subtitle = "Ad Subtitle";
    [TextArea(1, 4)] [SerializeField] string m_blurb = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
    [SerializeField] Sprite m_adSprite;
    [SerializeField] string m_url = "https://www.domain.com/";
    [SerializeField] GAMEID m_gameID = GAMEID.none;
    [SerializeField] MEDIUM m_medium = MEDIUM.affiliate;
    [SerializeField] ADID m_adID = ADID.none;
    [SerializeField] ADVARIANT m_adVariant = ADVARIANT._01;
    [SerializeField] PlatformAlternate[] m_platformAlternatives;

    // Helper method to get the applicable ad based on platform
    private SOBTSAd GetPlatformSpecificAd()
    {
        if (m_platformAlternatives == null || m_platformAlternatives.Length == 0)
            return this;

        RuntimePlatform currentPlatform = Application.platform;

        foreach (var alternate in m_platformAlternatives)
        {
            if (alternate.m_platforms != null)
            {
                foreach (var platform in alternate.m_platforms)
                {
                    if (platform == currentPlatform)
                    {
                        return alternate.m_btsAd != null ? alternate.m_btsAd : this;
                    }
                }
            }
        }

        return this; // Default to this ad if no match
    }

    public string Title
    {
        get
        {
            return GetPlatformSpecificAd().m_title;
        }
    }

    public string Subtitle
    {
        get
        {
            return GetPlatformSpecificAd().m_subtitle;
        }
    }

    public string Blurb
    {
        get
        {
            return GetPlatformSpecificAd().m_blurb;
        }
    }

    public Sprite AdSprite
    {
        get
        {
            return GetPlatformSpecificAd().m_adSprite;
        }
    }

    public string URL
    {
        get
        {
            return GetPlatformSpecificAd().m_url;
        }
    }

    public string GameID
    {
        get
        {
            return GetPlatformSpecificAd().m_gameID.ToString();
        }
    }

    public string Medium
    {
        get
        {
            return GetPlatformSpecificAd().m_medium.ToString();
        }
    }

    public string AdID
    {
        get
        {
            SOBTSAd l_ad = GetPlatformSpecificAd();
            return $"{l_ad.m_adID.ToString()}{l_ad.m_adVariant.ToString()}";
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