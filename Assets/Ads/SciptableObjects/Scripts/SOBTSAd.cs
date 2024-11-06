using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New SOBTSAd", menuName 
= "SO/BTS Ad")]
public class SOBTSAd : ScriptableObject
{
    [SerializeField] string m_title = "Ad Title";
    [SerializeField] string m_subtitle = "Ad Subtitle";
    [TextArea(1, 4)] [SerializeField] string m_blurb = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

    [SerializeField] Sprite m_adSprite;
    [SerializeField] string m_url = "https://www.domain.com";

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
}
