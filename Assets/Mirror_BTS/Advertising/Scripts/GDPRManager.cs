using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

public class GDPRManager : MonoBehaviour
{
    [SerializeField] bool m_isTesting = false;
    [SerializeField] string m_classname = "GDPRManager";
    [SerializeField] AdmobAdSingleton m_admobAdSingleton;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"{m_classname} Start");

#if (UNITY_IOS || UNITY_ANDROID)

        if (m_isTesting)
        {
            ConsentInformation.Reset();
        }

        var debugSettings = new ConsentDebugSettings
        {
            // Geography appears as in EEA for debug devices.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
        {
            "24B4207F149E57640E5D52985D597EA5"
        }
        };

        if (null == m_admobAdSingleton)
        {
            if (null != AdmobAdSingleton.SharedInstance)
			{
                m_admobAdSingleton = AdmobAdSingleton.SharedInstance;
			}
            else
            { 
                m_admobAdSingleton = FindFirstObjectByType<AdmobAdSingleton>();
            }
        }

        // Set tag for under age of consent.
        // Here false means users are not under age of consent.
        ConsentRequestParameters request;

        if (m_isTesting)
        {
            request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = debugSettings,

            };
        }
        else
        {
            request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,

            };
        }

        // Check the current consent information status.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
#else
    m_admobAdSingleton.gameObject.SetActive(true);
#endif


    }

    void OnConsentInfoUpdated(FormError consentError)
    {
        Debug.Log($"{m_classname} *** OnConsentInfoUpdated ***");

        if (consentError != null)
        {
            // Handle the error.
            Debug.Log($"{m_classname}:  /{consentError.ErrorCode}/ /{consentError.Message}/");
            return;
        }
        else
        {
            Debug.Log($"{m_classname}:  consentError == null");

        }

        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.

        Debug.Log($"{m_classname} LoadAndShowConsentFormIfRequired INVOKE");

        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            Debug.Log($"{m_classname} LoadAndShowConsentFormIfRequired CALLBACK");

            if (formError != null)
            {
                // Consent gathering failed.
                Debug.Log($"{m_classname}:  /{consentError.ErrorCode}/ /{consentError.Message}/");
                return;
            }

            //// Consent has been gathered.
            //if (ConsentInformation.CanRequestAds())
            //{
            //    MobileAds.Initialize((InitializationStatus initstatus) =>
            //    {
            //        // TODO: Request an ad.
            //    });
            //}

            // Make sure AdMob know that it can show non-limited ads and make sure iOS does not crash if
			// a duplicate call is made

			bool l_canRequestAds = false;

			try
			{
				l_canRequestAds = ConsentInformation.CanRequestAds();
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning($"{m_classname}: ConsentInformation.CanRequestAds() threw: {ex}");
				// fallback: treat as limited ads
				l_canRequestAds = false;
			}

            Debug.Log($"{m_classname}: CanRequestAds={l_canRequestAds}");

            if (null != m_admobAdSingleton)
            {
                Debug.Log($"{m_classname}: Setting m_admobAdSingleton Active");
                m_admobAdSingleton.gameObject.SetActive(true /*ConsentInformation.CanRequestAds()*/);
            }
            else
            {
                Debug.Log($"{m_classname}:  m_admobAdSingleton is NULL - what the puck!");
            }

        });

    }




}
