using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

public class GDPRManager : MonoBehaviour
{
    [SerializeField] bool m_isTesting = false;
    [SerializeField] string m_classname = "GDPRManager";
    [SerializeField] AdmobAdManager m_admobAdManager;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"{m_classname} Start m_isTesting = {m_isTesting}");

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

        if (null == m_admobAdManager)
        {
            m_admobAdManager = GetComponent<AdmobAdManager>();
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

            if (!ConsentInformation.CanRequestAds())
            {
                AdmobAdManager.NeedToShowAd = false;
            }

            m_admobAdManager.StartFromGDPRManager();


        });

    }

    public void Update()
    {
        if (null != m_admobAdManager)
        {

        }
    }


}
