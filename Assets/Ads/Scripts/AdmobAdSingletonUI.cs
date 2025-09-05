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
    [SerializeField] string m_classname = "AdmobAdSingletonUI";

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
        m_adPlayed?.Invoke();
	}

    public void Start()
    {
        SetupButtons(AdmobAdSingleton.IsAdmobAdAvailable);
    }


    public void ShowRV()
    {
        AdmobAdSingleton.ShowRV();
    }


    private void SetupButtons(bool a_admobAdIsAvailable)
    {

        m_playButton.interactable = true;
        m_playText.text = m_enabledString;
        m_playImage.sprite = m_enabledSprite;

        if (a_admobAdIsAvailable)
        {

        }
        else
        {

        }
    }


}