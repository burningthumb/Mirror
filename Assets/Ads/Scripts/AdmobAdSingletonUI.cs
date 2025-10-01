//
// Author:  Robert Wiebe
// Company: Burningthumb Studios
// Updated: 25 Sep 06
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

    [SerializeField] private bool m_skipNextAd = false;

    [SerializeField] private UnityEvent m_adPlayed;



    public bool SkipNextAd
    {
        get
        {
            return m_skipNextAd;
        }

        set
        {
            m_skipNextAd = value;
        }
    }

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

        if (SkipNextAd)
        {
            SkipNextAd = false;
            OnAdPlayed(true);
        }
        else
        { 
            AdmobAdSingleton.ShowRV();
        }
    }
}

