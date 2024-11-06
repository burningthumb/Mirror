using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System;

public class AdmobAdSingleton : MonoBehaviour
{
	public enum AdmobState { initializing, initialized, unknown }

	public delegate void AdAvailableDelegate(bool a_flag);
	public static AdAvailableDelegate m_adAvailableDelegate;

	public delegate void AdPlayedDelegate(bool a_flag);
	public static AdPlayedDelegate m_adPlayedDelegate;

	private static RewardedAd m_rewardedAd;

	[SerializeField] BTSAd m_BTSAd;

	[SerializeField] bool m_dontDestroyOnLoad = false;

	[SerializeField] string m_classname = "AdmobAdSingleton";

	[SerializeField] string m_noAdUnitID = "unused";
	[SerializeField] string m_androidAdUnitID = "ca-app-pub-3940256099942544/5224354917";
	[SerializeField] string m_iosAdUnitID = "ca-app-pub-3940256099942544/1712485313";

	string _adUnitId = null; // This will be set to unused for unsupported platforms

	private string _gameId;

	private bool m_needToLoadAd = false;

	private static AdmobState m_admobState = AdmobState.unknown;

	static bool m_isAdAvailable = false;

	static string m_static_classname = "AdmobAdSingleton";

	static AdmobAdSingleton m_sharedInstance = null;

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

	public static AdAvailableDelegate AdAvailable
	{
		get
		{
			return m_adAvailableDelegate;
		}

		set
		{
			m_adAvailableDelegate = value;

			int l_count = 0;

			if (null != m_adAvailableDelegate)
			{
				Delegate[] l_delegates = m_adAvailableDelegate.GetInvocationList();

				if (null != l_delegates)
				{
					l_count = l_delegates.Length;
				}
			}

			Debug.Log($"{m_static_classname}: m_adAvailableDelegate.GetInvocationList().Length = {l_count}");
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

			int l_count = 0;

			if (null != m_adPlayedDelegate)
			{
				Delegate[] l_delegates = m_adPlayedDelegate.GetInvocationList();


				if (null != l_delegates)
				{
					l_count = l_delegates.Length;
				}
			}

			Debug.Log($"{m_static_classname}: m_adPlayedDelegate.GetInvocationList().Length = {l_count}");

		}
	}

	public static bool IsAdAvailable
	{
		get
		{
			return m_isAdAvailable;
		}

		set
		{
			m_isAdAvailable = value;

			m_adAvailableDelegate?.Invoke(m_isAdAvailable);
		}
	}

	void Awake()
	{
		if (m_dontDestroyOnLoad)
		{
			// There can be only one
			if (null != SharedInstance)
			{
				Debug.LogWarning($"{m_classname} - There can be only 1. Self destruct second instance!");
				Destroy(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);
		}

		SharedInstance = this;

		m_static_classname = m_classname;

#if (UNITY_IOS || UNITY_ANDROID)
		m_needToLoadAd = true;
#else
		m_needToLoadAd = false;
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

		if (m_needToLoadAd)
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

		if (AdmobState.unknown == m_admobState)
		{
			// Need to Init the SDK
			m_admobState = AdmobState.initializing;
			Debug.Log($"{m_classname}: Invoking InitializeGoogleMobileAds()");
			InitializeGoogleMobileAds(); // --> The callback MUST call LoadRV()

		}
		else
		{

			if (AdmobState.initialized == m_admobState)
			{
				LoadRV();
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
		if (AdmobState.initialized == m_admobState)
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
				m_admobState = AdmobState.unknown;
				Invoke(nameof(InitializeGoogleMobileAds), 5.0f);
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
			m_admobState = AdmobState.initialized;

			LoadRV();
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

				StopCoroutine(RetryLoadRV());
				StartCoroutine(RetryLoadRV());

				return;
			}

			// If the operation failed for unknown reasons.
			// This is an unexpected error, please report this bug if it happens.
			if (ad == null)
			{
				Debug.LogError($"{m_classname}: Unexpected error: Rewarded load event fired with null ad and null error.");

				StopCoroutine(RetryLoadRV());
				StartCoroutine(RetryLoadRV());

				return;
			}

			// The operation completed successfully.
			Debug.Log($"{m_classname}: Rewarded ad loaded with response : " + ad.GetResponseInfo());
			m_rewardedAd = ad;

			// Register to ad events to extend functionality.
			RegisterEventHandlers(ad);

			// Inform the UI (or anyone else that cares) that the ad is ready.
			IsAdAvailable = true;

		});
	}

	public static void ShowAd()
	{
		if (m_rewardedAd != null && m_rewardedAd.CanShowAd())
		{
			Debug.Log($"{m_static_classname}: Showing rewarded ad.");
			m_rewardedAd.Show((Reward reward) =>
			{
				Debug.Log($"{m_static_classname}: Rewarded ad granted a reward: {reward.Amount} {reward.Type}");
			});
		}
		else
		{
			if (null != SharedInstance)
			{
				if (null != SharedInstance.m_BTSAd)
				{
					Debug.Log($"{m_static_classname}: Rewarded ad is not ready yet - show your BTS Ad here");
					SharedInstance.m_BTSAd.gameObject.SetActive(true);
				}
				else
				{
					Debug.LogWarning($"{m_static_classname}: Rewarded ad is not ready yet - no BTS Ad - play the game");
					m_adPlayedDelegate.Invoke(true);
				}
			}
			else
			{
				Debug.LogError($"{m_static_classname}: What the Puck! SharedInstance is NULL!");
			}


		}

		// Inform the UI (or anyone else that cares) that the ad is not ready.
		IsAdAvailable = false;
	}

	public void DestroyAd()
	{
		if (m_rewardedAd != null)
		{
			Debug.Log($"{m_classname}: Destroying rewarded ad.");
			m_rewardedAd.Destroy();
			m_rewardedAd = null;
		}

		// Inform the UI (or anyone else that cares) that the ad is not ready.
		IsAdAvailable = false;

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

			// Load another ad
			DestroyAd();
			LoadRV();

			m_adPlayedDelegate.Invoke(true);
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
		if (AdmobState.initialized != m_admobState)
		{
			StopCoroutine(RetryLoadRV());
			StartCoroutine(RetryLoadRV());

			IsAdAvailable = false;

			return;
		}

		if (null == m_rewardedAd)
		{
			LoadAd();
		}
		else
		{
			IsAdAvailable = true;
		}

	}

	// Try to load the rewarded video in 5 seconds
	IEnumerator RetryLoadRV()
	{
		Debug.Log($"{m_classname}: Waiting 5 seconds");
		yield return new WaitForSeconds(5.0f);

		LoadRV();

	}

	public static void ShowRV()
	{
		Debug.Log($"{m_static_classname}: ShowRV");

		ShowAd();

	}

	IEnumerator RetryShowRV()
	{

		yield return new WaitForSeconds(5.0f);

		ShowRV();

	}

	public static void ContinueFromBTSAd()
	{
		MyBTSAd.gameObject.SetActive(false);
		m_adPlayedDelegate.Invoke(true);
	}

}