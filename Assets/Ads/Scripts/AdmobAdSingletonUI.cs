//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Advertisements;
//using UnityEngine.Events;
//using UnityEngine.UI;
//using GoogleMobileAds.Api;
//using System;

//public class AdmobAdSingletonUI : MonoBehaviour
//{
//    [SerializeField] string m_classname = "AdmobAdSingletonUI";

//    [SerializeField] Button m_playButton;
//    [SerializeField] TMP_Text m_playText;
//    [SerializeField] TMP_Text m_hintText;
//    [SerializeField] Image m_playImage;

//    [SerializeField] string m_enabledString = "Show Ad";
//    [SerializeField] string m_disabledString = "Waiting...";
//    [SerializeField] string m_unsupportedPlatformString = "Play";

//    [SerializeField] string m_playAdHint = "Watch Ad, Play Game";

//    [SerializeField] Sprite m_enabledSprite;
//    [SerializeField] Sprite m_disabledSprite;

//    [SerializeField] UnityEvent m_adPlayed;

//    [SerializeField] bool m_adAvailableButtonInteractable = true;
//	[SerializeField] bool m_adNotAvailableButtonInteractable = false;

//    string _adUnitId = null; // This will be set to unused for unsupported platforms

//    private string _gameId;

//	public void OnEnable()
//	{
//		AdmobAdSingleton.AdAvailable += AdAvailableDelegate;
//		AdmobAdSingleton.AdPlayed += AdPlayedDelegate;
//	}

//    public void OnDisable()
//	{
//		AdmobAdSingleton.AdAvailable -= AdAvailableDelegate;
//		AdmobAdSingleton.AdPlayed -= AdPlayedDelegate;
//	}

//	public void AdAvailableDelegate(bool a_flag)
//	{
//		if (null != m_playButton)
//		{
//			if (a_flag)
//			{
//				m_playButton.interactable = m_adAvailableButtonInteractable;
//			}
//			else
//			{
//				m_playButton.interactable = m_adNotAvailableButtonInteractable;
//			}

//		}

//		if ((null != m_hintText) && (!string.IsNullOrEmpty(m_playAdHint)))
//		{
//			m_hintText.text = m_playAdHint;
//		}
//	}

//    public void AdPlayedDelegate(bool a_flag)
//	{
//        m_adPlayed?.Invoke();
//	}

//    public void Start()
//    {
//        SetupButtons(AdmobAdSingleton.IsAdmobAdAvailable);
//    }


//    public void ShowRV()
//    {
//        AdmobAdSingleton.ShowRV();
//    }


//    private void SetupButtons(bool a_admobAdIsAvailable)
//    {

//        m_playButton.interactable = true;
//        m_playText.text = m_enabledString;
//        m_playImage.sprite = m_enabledSprite;

//        if (a_admobAdIsAvailable)
//        {

//        }
//        else
//        {

//        }
//    }


//}

//
// Author:  Robert Wiebe
// Company: Burningthumb Studios
// Updated: 25 Aug 27
//

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdmobAdSingletonUI : MonoBehaviour
{
    private static GameObject s_requestingGO;

    [SerializeField] private Button m_playButton;
    [SerializeField] private TMP_Text m_playText;
    [SerializeField] private TMP_Text m_hintText;
    [SerializeField] private TMP_Text m_sourceText;
    [SerializeField] private Image m_playImage;

    [SerializeField] private string m_playAdHint = "Watch Ad, Play Game";

    [SerializeField] string m_sourceAdMobAdAvailable = "AdMob Ad";
    [SerializeField] string m_sourceAdMobAdNotAvailable = "BTS Ad";

    [SerializeField] private UnityEvent m_adPlayed;

	private void OnEnable()
    {
        // Clear the requesting gameobject
        if (gameObject == s_requestingGO)
		{
            s_requestingGO = null;
		}

        // Setup the callbacks
        AdmobAdSingleton.AdmobAdAvailable += OnAdmobAdAvailable;
        AdmobAdSingleton.AdPlayed += OnAdPlayed;

        // Setup based on the current value since there will have been no notification the first time
        OnAdmobAdAvailable(AdmobAdSingleton.IsAdmobAdAvailable);
    }

    private void OnDisable()
    {
        // Remove the callbacks
        AdmobAdSingleton.AdmobAdAvailable -= OnAdmobAdAvailable;
        AdmobAdSingleton.AdPlayed -= OnAdPlayed;

        // Clear the requesting gameobject
        if (gameObject == s_requestingGO)
		{
            s_requestingGO = null;
		}
    }

    private void OnAdmobAdAvailable(bool a_flag)
    {

        // Indicate if the ad will be served from Admob or BTSAds
        if (null != m_sourceText)
		{
			if (a_flag)
			{
				m_sourceText.text = m_sourceAdMobAdAvailable;
			}
			else
			{
				m_sourceText.text = m_sourceAdMobAdNotAvailable;
			}
		}

        // Display any hint text
		if ((null != m_hintText) && (!string.IsNullOrEmpty(m_playAdHint)))
		{
			m_hintText.text = m_playAdHint;
		}
    }

    // An Add played, it could have been from AdMob or from BTS Ads
    // We don't know, and we don't care, just invoke the Unity events either way
    // In the future maybe we will care, but not today
    public void OnAdPlayed(bool a_flag)
	{
		if (gameObject == s_requestingGO)
		{
            s_requestingGO = null;
			m_adPlayed?.Invoke();
		}
	}

    // Typically this is envoked from a UI Button and we just pass it on to the
    // AdmobAdSingleton
    //
    // We are expecting a callback to OnAdPlayed which is why we made a note of which game object
    // made the request. This just handles the case where multiple AdmobAdSingletonUI are active at the
    // same time. Its a rare event, but it could happen
    public void ShowRV()
    {
		s_requestingGO = gameObject;

        AdmobAdSingleton.ShowRV();
    }
}

