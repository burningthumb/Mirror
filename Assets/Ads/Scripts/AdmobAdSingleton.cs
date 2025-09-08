using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System;
using GoogleMobileAds.Common;

public class AdmobAdSingleton : MonoBehaviour
{
    public enum AdmobState { initializing, initialized, unknown }

    public delegate void AdAvailableDelegate(bool a_flag);
    public static AdAvailableDelegate m_adAvailableDelegate;

    public delegate void AdPlayedDelegate(bool a_flag);
    public static AdPlayedDelegate m_adPlayedDelegate;

    private static RewardedAd m_rewardedAd;

    private static AdmobState s_admobState = AdmobState.unknown;

    private static bool m_isAdmobAdAvailable = false;

    private static AdmobAdSingleton m_sharedInstance = null;

    [SerializeField] BTSAd m_BTSAd;

    [SerializeField] bool m_dontDestroyOnLoad = false;

    [SerializeField] string m_noAdUnitID = "unused";
    [SerializeField] string m_androidAdUnitID = "ca-app-pub-3940256099942544/5224354917";
    [SerializeField] string m_iosAdUnitID = "ca-app-pub-3940256099942544/1712485313";

    string _adUnitId = null; // This will be set to unused for unsupported platforms

    private string _gameId;

    private bool m_needToLoadAdmobAd = false;

    // Coroutine references
    private Coroutine m_retryLoadCoroutine;
    private Coroutine m_retryShowCoroutine;

    public static BTSAd MyBTSAd
    {
        get
        {
            if (null == SharedInstance)
            {
                return null;
            }

            return SharedInstance.m_BTSAd;
        }
    }

    public static AdmobAdSingleton SharedInstance
    {
        get
        {
            return m_sharedInstance;
        }

        set
        {
            m_sharedInstance = value;
        }
    }

    public static AdAvailableDelegate AdmobAdAvailable
    {
        get
        {
            return m_adAvailableDelegate;
        }

        set
        {
            m_adAvailableDelegate = value;
         }
    }

    public static AdPlayedDelegate AdPlayed
    {
        get
        {
            return m_adPlayedDelegate;
        }

        set
        {
            m_adPlayedDelegate = value;
        }
    }

    public static bool IsAdmobAdAvailable
    {
        get
        {
            return m_isAdmobAdAvailable;
        }

        set
        {
            m_isAdmobAdAvailable = value;

            m_adAvailableDelegate?.Invoke(m_isAdmobAdAvailable);
        }
    }

    void Awake()
    {
        if (m_dontDestroyOnLoad)
        {
            // There can be only one
            if (null != SharedInstance)
            {
                Debug.LogWarning($"There can be only 1. Self destruct second instance!");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        SharedInstance = this;

#if (UNITY_IOS || UNITY_ANDROID)
        m_needToLoadAdmobAd = true;
#else
        m_needToLoadAdmobAd = false;
#endif
    }

    public void Start()
    {
        // On Android, Unity is paused when displaying interstitial or rewarded video.
        // This setting makes iOS behave consistently with Android.
#if UNITY_IOS
		MobileAds.SetiOSAppPauseOnBackground(true);
#endif

        // When true all events raised by GoogleMobileAds will be raised
        // on the Unity main thread. The default value is false.
        // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
#if (UNITY_IOS || UNITY_ANDROID)
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
#endif

        if (m_needToLoadAdmobAd)
        {
            InitializeAds();
        }

    }

    public void InitializeAds()
    {

#if UNITY_IOS
		_adUnitId = m_iosAdUnitID;
#elif UNITY_ANDROID
        _adUnitId = m_androidAdUnitID;
#else
        _adUnitId = m_noAdUnitID;
#endif

        if (AdmobState.unknown == s_admobState)
        {
            // Need to Init the SDK
            s_admobState = AdmobState.initializing;

            InitializeGoogleMobileAds(); // --> The callback MUST call LoadRV()

        }
        else
        {

            if (AdmobState.initialized == s_admobState)
            {
                if (!IsAdmobAdAvailable)
                { 
                    LoadRV();
                }
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(RetryLoadRV());
            }
        }
    }

    /// <summary>
    /// Initializes the Google Mobile Ads Unity plugin.
    /// </summary>
    private void InitializeGoogleMobileAds()
    {

        // The Google Mobile Ads Unity plugin needs to be run only once and before loading any ads.
        if (AdmobState.initialized == s_admobState)
        {
            return;
        }

        // Initialize the Google Mobile Ads Unity plugin.
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (initstatus == null)
                {
                    s_admobState = AdmobState.unknown;
                    Invoke(nameof(InitializeGoogleMobileAds), 5.0f);
                    return;
                }

                s_admobState = AdmobState.initialized;

                LoadRV();
            });
        });
    }

    public void LoadAd()
    {
        // Clean up the old ad before loading a new one.
        if (m_rewardedAd != null)
        {
            DestroyAd();

            // Invoking DestroyAd()informs the delegates so no need to do it again here
        }

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {

                // If the operation failed with a reason.
                if (error != null)
                { 
                    RestartRetryLoadRV();

                    return;
                }

                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
  
                    RestartRetryLoadRV();

                    return;
                }

                // The operation completed successfully.
                m_rewardedAd = ad;

                // Register to ad events to extend functionality.
                RegisterEventHandlers(ad);

                // Inform the UI (or anyone else that cares) that the ad is ready.
                IsAdmobAdAvailable = true;
            });

        });
    }

    public static void ShowAd()
    {
        if (m_rewardedAd != null && m_rewardedAd.CanShowAd())
        {
            m_rewardedAd.Show((Reward reward) =>
            {
                // Show must be invoked, this callback does nothing
            });
        }
        else
        {
            if (null != SharedInstance)
            {
                if (null != SharedInstance.m_BTSAd)
                {
                    SharedInstance.m_BTSAd.gameObject.SetActive(true);
                }
                else
                {
                    // This should never happen
                    m_adPlayedDelegate?.Invoke(true);
                }
            }
            else
            {
                // This should never happen
                Debug.LogError("Shared instance is null. Something went wrong");
                m_adPlayedDelegate?.Invoke(true);
            }
        }

        // Inform the UI (or anyone else that cares) that the ad is not ready.
        IsAdmobAdAvailable = false;
    }

    public void DestroyAd()
    {
        if (m_rewardedAd != null)
        {
             m_rewardedAd.Destroy();
            m_rewardedAd = null;
        }

        // Inform the UI (or anyone else that cares) that the ad is not ready.
        IsAdmobAdAvailable = false;

    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //Debug.Log($"{m_classname}: Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}.");
            });
        };

        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //Debug.Log($"{m_classname}: Rewarded ad recorded an impression.");
            });
        };

        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //Debug.Log($"{m_classname}: Rewarded ad was clicked.");
            });
        };

        // Raised when the ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //Debug.Log($"{m_classname}: Rewarded ad full screen content opened.");
            });
        };

        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //Debug.Log($"{m_classname}: Rewarded ad full screen content closed.");

                // Load another ad
                DestroyAd();
                LoadRV();

                m_adPlayedDelegate?.Invoke(true);
            });
        };

        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                //Debug.LogError($"{m_classname}: Rewarded ad failed to open full screen content with error : {error.GetMessage()}");

                RestartRetryShowRV();
            });
        };
    }


    void LoadRV()
    {
        if (AdmobState.initialized != s_admobState)
        {
            RestartRetryLoadRV();

            IsAdmobAdAvailable = false;

            return;
        }

        if (null == m_rewardedAd)
        {
            LoadAd();
        }
        else
        {
            IsAdmobAdAvailable = true;
        }

    }

    // Try to load the rewarded video in 5 seconds
    IEnumerator RetryLoadRV()
    {
        yield return new WaitForSeconds(5.0f);

        LoadRV();
    }

    IEnumerator RetryShowRV()
    {
        yield return new WaitForSeconds(5.0f);

        ShowRV();

    }

    private void RestartRetryLoadRV()
    {
        if (m_retryLoadCoroutine != null)
        {
            StopCoroutine(m_retryLoadCoroutine);
        }

        m_retryLoadCoroutine = StartCoroutine(RetryLoadRV());
    }

    private void RestartRetryShowRV()
    {
        if (m_retryShowCoroutine != null)
        {
            StopCoroutine(m_retryShowCoroutine);
        }

        m_retryShowCoroutine = StartCoroutine(RetryShowRV());
    }

    public static void ShowRV()
    {
        ShowAd();
    }

    public static void ContinueFromBTSAd()
    {
        MyBTSAd.gameObject.SetActive(false);
        m_adPlayedDelegate?.Invoke(true);
    }

}