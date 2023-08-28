using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System;

public class AdmobAdManager : MonoBehaviour
{
    public enum keys { AdDiceRoll };

    [SerializeField] bool m_isTesting = false;

    // AdMob specific items
    public GameObject AdLoadedStatus;
    private RewardedAd _rewardedAd;

    [SerializeField] string m_classname = "AdmobAdManager";
    [SerializeField] int m_minDice = 3;

    [SerializeField] string m_noAdUnitID = "unused";
    [SerializeField] string m_androidAdUnitID = "ca-app-pub-3940256099942544/5224354917";
    [SerializeField] string m_iosAdUnitID = "ca-app-pub-3940256099942544/1712485313";

    [SerializeField] Button m_playButton;
    [SerializeField] TMP_Text m_playText;
    [SerializeField] TMP_Text m_hintText;
    [SerializeField] Image m_playImage;

    [SerializeField] string m_enabledString = "Show Ad";
    [SerializeField] string m_disabledString = "Waiting...";
    [SerializeField] string m_unsupportedPlatformString = "Play";

    [SerializeField] string m_showAdHint = "Watch Ad, Play Game";
    [SerializeField] string m_noAdHint = "";

    [SerializeField] Sprite m_enabledSprite;
    [SerializeField] Sprite m_disabledSprite;

    [SerializeField] UnityEvent m_adPlayed;

    string _adUnitId = null; // This will be set to unused for unsupported platforms

    private string _gameId;

    private int m_adDiceRoll = 7;

    private static bool m_isInitialized = false;
    private static bool m_needToShowAd = false;

    public static bool IsInitialized
    {
        get
        {
            return m_isInitialized;
        }

        set
        {
            m_isInitialized = value;
        }
    }

    public static bool NeedToShowAd
    {
        get
        {
            return m_needToShowAd;
        }

        set
        {
            m_needToShowAd = value;
        }
    }

    //void OnApplicationPause(bool isPaused)
    //{
    //    Debug.Log($"{m_classname}:  OnApplicationPause = " + isPaused);
    //}

    void Awake()
    {
        Debug.Log($"{m_classname} Awake");

        // On Android, Unity is paused when displaying interstitial or rewarded video.
        // This setting makes iOS behave consistently with Android.
#if UNITY_IOS
        MobileAds.SetiOSAppPauseOnBackground(true);
#endif

        // When true all events raised by GoogleMobileAds will be raised
        // on the Unity main thread. The default value is false.
        // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
#if (UNITY_ANDROID || UNITY_IOS)
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
#endif

#if (UNITY_ANDROID || UNITY_IOS)
        m_adDiceRoll = PlayerPrefs.GetInt(keys.AdDiceRoll.ToString(), 6);
#endif

        if (m_adDiceRoll > m_minDice)
        {
            NeedToShowAd = false;
            PlayerPrefs.DeleteKey(keys.AdDiceRoll.ToString());
            PlayerPrefs.Save();
        }
        else
        {
            NeedToShowAd = true;
        }

#if (!(UNITY_ANDROID || UNITY_IOS)) || (UNITY_EDITOR)
        NeedToShowAd = false;
#endif
        // Testing
        if (m_isTesting)
        { 
            NeedToShowAd = true;
        }
    }

   // public void Start()
   public void StartFromGDPRManager()
    {
        Debug.Log($"{m_classname} StartFromGDPRManager");

        //        Debug.Log(m_adDiceRoll + " " + NeedToShowAd);

        if (NeedToShowAd)
        {
            InitializeAds();
        }
        else
        {
            DoNotInitializeAds();
        }

    }

    public void DoNotInitializeAds()
    {
        m_playButton.interactable = true;
        m_playText.text = m_unsupportedPlatformString;
        m_hintText.text = m_noAdHint;
        m_playImage.sprite = m_enabledSprite;
    }

    public void InitializeAds()
    {
        EnablePlayButton(false);

#if UNITY_IOS
        _adUnitId = m_iosAdUnitID;
#elif UNITY_ANDROID
        _adUnitId = m_androidAdUnitID;
#else
        _adUnitId = m_noAdUnitID;
#endif

        if (!IsInitialized)
        {
            // SDK init
            Debug.Log($"{m_classname}: Invoking InitializeGoogleMobileAds()");
            InitializeGoogleMobileAds(); // --> The callback MUST call LoadRV()
        }
        else
        {
            LoadRV();
        }
    }

    /// <summary>
    /// Initializes the Google Mobile Ads Unity plugin.
    /// </summary>
    private void InitializeGoogleMobileAds()
    {
        // The Google Mobile Ads Unity plugin needs to be run only once and before loading any ads.
        if (IsInitialized)
        {
            return;
        }

        // Initialize the Google Mobile Ads Unity plugin.
        Debug.Log($"{m_classname}: Google Mobile Ads Initializing.");
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError($"{m_classname}: Google Mobile Ads initialization failed.");
                IsInitialized = false;
                return;
            }

            // If you use mediation, you can check the status of each adapter.
            var adapterStatusMap = initstatus.getAdapterStatusMap();
            if (adapterStatusMap != null)
            {
                foreach (var item in adapterStatusMap)
                {
                    Debug.Log($"{m_classname}: Adapter {item.Key} is {item.Value.InitializationState}");
                }
            }

            Debug.Log($"{m_classname}: Google Mobile Ads initialization complete.");
            IsInitialized = true;
            LoadRV();
        });
    }

    public void LoadAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            DestroyAd();
        }

        Debug.Log($"{m_classname}: Loading rewarded ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError($"{m_classname}: Rewarded ad failed to load an ad with error : {error.GetMessage()}");

                EnablePlayButton(false);
                StopCoroutine(RetryLoadRV());
                StartCoroutine(RetryLoadRV());

                return;
            }

            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError($"{m_classname}: Unexpected error: Rewarded load event fired with null ad and null error.");

                EnablePlayButton(false);
                StopCoroutine(RetryLoadRV());
                StartCoroutine(RetryLoadRV());

                return;
            }

            // The operation completed successfully.
            Debug.Log($"{m_classname}: Rewarded ad loaded with response : " + ad.GetResponseInfo());
            _rewardedAd = ad;

            // Register to ad events to extend functionality.
            RegisterEventHandlers(ad);

            // Inform the UI that the ad is ready.
            if (null != AdLoadedStatus)
            {
                AdLoadedStatus?.SetActive(true);
            }

            // UI hardcoded
            EnablePlayButton(true);
        });
    }

    public void ShowAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            Debug.Log($"{m_classname}: Showing rewarded ad.");
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"{m_classname}: Rewarded ad granted a reward: {reward.Amount} {reward.Type}");
            });
        }
        else
        {
            Debug.LogError($"{m_classname}: Rewarded ad is not ready yet.");
        }

        // Inform the UI that the ad is not ready.
        if (null != AdLoadedStatus)
        {
            AdLoadedStatus?.SetActive(false);
        }
    }

    public void DestroyAd()
    {
        if (_rewardedAd != null)
        {
            Debug.Log($"{m_classname}: Destroying rewarded ad.");
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        // Inform the UI that the ad is not ready.
        if (null != AdLoadedStatus)
        {
            AdLoadedStatus?.SetActive(false);
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"{m_classname}: Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}.");
        };

        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log($"{m_classname}: Rewarded ad recorded an impression.");
        };

        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log($"{m_classname}: Rewarded ad was clicked.");
        };

        // Raised when the ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log($"{m_classname}: Rewarded ad full screen content opened.");
        };

        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log($"{m_classname}: Rewarded ad full screen content closed.");

            PlayerPrefs.DeleteKey(UnityAdManager.keys.AdDiceRoll.ToString());
            PlayerPrefs.Save();

            m_adPlayed.Invoke();
        };

        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"{m_classname}: Rewarded ad failed to open full screen content with error : {error.GetMessage()}");

            StopCoroutine(RetryShowRV());
            StartCoroutine(RetryShowRV());
        };
    }


    void LoadRV()
    {
        if (null == _rewardedAd)
        {
            LoadAd();
        }
        else
        {
            EnablePlayButton(true);
        }

    }

    //It looks like IronSource does not require a rewarded video to be loaded
    IEnumerator RetryLoadRV()
    {
        Debug.Log($"{m_classname}: Waiting 5 seconds");
        yield return new WaitForSeconds(5.0f);

        LoadRV();

    }

    public void ShowRV()
    {
        Debug.Log($"{m_classname}: NeedToShowAd = {NeedToShowAd}");

        if (NeedToShowAd)
        {
            ShowAd();
        }
        else
        {
            m_adPlayed.Invoke();
        }
    }

    IEnumerator RetryShowRV()
    {

        yield return new WaitForSeconds(5.0f);

        ShowRV();

    }

    private void EnablePlayButton(bool a_bool)
    {
        if (a_bool)
        {
            m_playButton.interactable = true;
            m_playText.text = m_enabledString;
            m_hintText.text = m_showAdHint;
            m_playImage.sprite = m_enabledSprite;
        }
        else
        {
            m_playButton.interactable = false;
            m_playText.text = m_disabledString;
            m_hintText.text = m_noAdHint;
            m_playImage.sprite = m_disabledSprite;
        }
    }


}