using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System;

public class AdmobAdSingletonUI : MonoBehaviour
{
    public enum keys { AdDiceRoll };

    [SerializeField] string m_classname = "AdmobAdSingletonUI";
    [SerializeField] int m_minDice = 3;

    [SerializeField] Button m_playButton;
    [SerializeField] TMP_Text m_playText;
    [SerializeField] TMP_Text m_hintText;
    [SerializeField] Image m_playImage;

    [SerializeField] string m_enabledString = "Show Ad";
    [SerializeField] string m_disabledString = "Waiting...";
    [SerializeField] string m_unsupportedPlatformString = "Play";

    [SerializeField] string m_playAdHint = "Watch Ad, Play Game";

    [SerializeField] Sprite m_enabledSprite;
    [SerializeField] Sprite m_disabledSprite;

    [SerializeField] UnityEvent m_adPlayed;

    [SerializeField] bool m_adAvailableButtonInteractable = true;
	[SerializeField] bool m_adNotAvailableButtonInteractable = false;

    string _adUnitId = null; // This will be set to unused for unsupported platforms

    private string _gameId;

    private int m_adDiceRoll = 7;
    private bool m_platformSupportsAdvertising = false;

	public void OnEnable()
	{
		AdmobAdSingleton.AdAvailable += AdAvailableDelegate;
		AdmobAdSingleton.AdPlayed += AdPlayedDelegate;
	}

    public void OnDisable()
	{
		AdmobAdSingleton.AdAvailable -= AdAvailableDelegate;
		AdmobAdSingleton.AdPlayed -= AdPlayedDelegate;
	}

	public void AdAvailableDelegate(bool a_flag)
	{
		Debug.Log($"AdAvailableDelegate: {a_flag}");

		if (null != m_playButton)
		{
			if (a_flag)
			{
				m_playButton.interactable = m_adAvailableButtonInteractable;
			}
			else
			{
				m_playButton.interactable = m_adNotAvailableButtonInteractable;
			}
			
		}

		if ((null != m_hintText) && (!string.IsNullOrEmpty(m_playAdHint)))
		{
			m_hintText.text = m_playAdHint;
		}
	}

    public void AdPlayedDelegate(bool a_flag)
	{
        
		PlayerPrefs.DeleteKey(AdmobAdSingletonUI.keys.AdDiceRoll.ToString());
		PlayerPrefs.Save();

        m_adPlayed?.Invoke();
	}

	void Awake()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        m_adDiceRoll = PlayerPrefs.GetInt(keys.AdDiceRoll.ToString(), 6);
#endif

        if (m_adDiceRoll > m_minDice)
        {
            m_platformSupportsAdvertising = false;
            PlayerPrefs.DeleteKey(keys.AdDiceRoll.ToString());
            PlayerPrefs.Save();
        }
        else
        {
            m_platformSupportsAdvertising = true;
        }

#if (!(UNITY_ANDROID || UNITY_IOS)) || (UNITY_EDITOR)
        m_platformSupportsAdvertising = false;
#endif
        // Testing
//#if UNITY_EDITOR
//        m_platformSupportsAdvertising = true;
//#endif

        // ALL PLATFORMS NOW SUPPORT ADS USING BUILTIN BTS ADS WHEN AD NETWORKS ARE NOT AVAILABLE
        m_platformSupportsAdvertising = true;
    }

    public void Start()
    {

        Debug.Log($"m_adDiceRoll={m_adDiceRoll}  m_platformSupportsAdvertising={m_platformSupportsAdvertising}");

        if (m_platformSupportsAdvertising)
        {
            // Advertising is always available
            // EnablePlayButton(AdmobAdSingleton.IsAdAvailable);
            EnablePlayButton(true);

            // If an Ad is not available at this time, the delegate will be called when it is
        }
        else
        {
            m_playButton.interactable = true;
            m_playText.text = m_unsupportedPlatformString;
            m_hintText.text = "";
            m_playImage.sprite = m_enabledSprite;
        }

    }


    public void ShowRV()
    {
        Debug.Log($"{m_classname}: m_platformSupportsAdvertising = {m_platformSupportsAdvertising}");

        if (m_platformSupportsAdvertising)
        {
            AdmobAdSingleton.ShowRV();
        }
        else
        {
            m_adPlayed.Invoke();
        }
    }


    private void EnablePlayButton(bool a_bool)
    {
        if (a_bool)
        {
            m_playButton.interactable = true;
            m_playText.text = m_enabledString;
            m_playImage.sprite = m_enabledSprite;
        }
        else
        {
            m_playButton.interactable = false;
            m_playText.text = m_disabledString;
            m_playImage.sprite = m_disabledSprite;
        }
    }


}