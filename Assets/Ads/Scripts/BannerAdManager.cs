using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine.UI;
using System.Collections;
using System;

public class BannerAdManager : MonoBehaviour
{
    [SerializeField] string m_AndroidBannerAdID = "ca-app-pub-3940256099942544/6300978111";
    [SerializeField] string m_IOSBannerAdID = "ca-app-pub-3940256099942544/2934735716";

    private BannerView m_bannerView;
    private RectTransform m_rectTransform; // Reference to this panel's RectTransform

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>(); // Cache the RectTransform
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(InitializeBanner());
    }

    private void OnDestroy()
    {
        DestroyBannerViewIfNeeded();
    }

    private void DestroyBannerViewIfNeeded()
    {
        if (m_bannerView != null)
        {
            m_bannerView.Destroy();
            m_bannerView = null;
        }
    }

    public IEnumerator InitializeBanner()
    {
        yield return null;

        DestroyBannerViewIfNeeded();

        // Force layout rebuild to ensure RectTransform dimensions are up-to-date
        // This is crucial for stretched RectTransforms (like yours with anchors minX=0, maxX=1, minY=0, maxY=0.1)
        // where size is determined at runtime based on the parent Canvas and screen size.
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectTransform);

        Vector3[] corners = new Vector3[4];
        m_rectTransform.GetWorldCorners(corners);

        // Get min/max for x (left to right) and y (bottom to top, as world space y increases up)
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

        // Pixel dimensions (calculated dynamically at runtime, not assuming fixed size)
        float pixelWidth = maxX - minX;
        float pixelHeight = maxY - minY; // Not used for adaptive, but could for fixed custom size

        // Banner position: x = left edge, y = distance from top (flip y axis for top-left origin)
        int posX = Mathf.RoundToInt(minX);
        int posY = Mathf.RoundToInt(Screen.height - maxY);

        // Get device density to convert pixels to dp (AdSize uses dp)
        float density = MobileAds.Utils.GetDeviceScale();

        int dpWidth = Mathf.Max((int)pixelWidth, Mathf.RoundToInt(pixelWidth / density));


        // int dpHeight = Mathf.RoundToInt(pixelHeight / density); // Uncomment if using fixed custom size below

        // Create adaptive AdSize based on panel width (recommended for banners)
        // Since your panel is full-width (anchors x: 0-1), this will effectively be a full-width adaptive banner
        AdSize adSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(dpWidth);

        // Alternative: Fixed custom size matching panel exactly (less recommended, may reduce ad fill)
        // AdSize adSize = new AdSize(dpWidth, dpHeight);

        Debug.Log($"Screen: width={Screen.width}, height={Screen.height}");
        Debug.Log($"Panel pixels: width={pixelWidth}, height={pixelHeight}, minX={minX}, maxY={maxY}");
        Debug.Log($"Density={density}, dpWidth={dpWidth}");
        Debug.Log($"Position: posX={posX}, posY={posY}");

        Debug.Log($"Adsize = {adSize.AdType} {adSize.Width} {adSize.Height}");

        string adUnitId =
#if UNITY_ANDROID
            m_AndroidBannerAdID;

        if (BTS_TV_Type.IsAndroidTV)
        {
            adUnitId = string.Empty;
        }

#elif UNITY_IOS
            m_IOSBannerAdID;
#else
            string.Empty;
#endif

        try
        {
            if (!string.IsNullOrEmpty(adUnitId))
            {
                //m_bannerView = new BannerView(adUnitId, adSize, posX, posY);
                //m_bannerView = new BannerView(adUnitId, adSize, AdPosition.Bottom);
                m_bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

                m_bannerView.OnBannerAdLoaded += () =>
                {
                    Debug.Log("Banner ad loaded successfully.");
                };

                // Called when an ad fails to load
                m_bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
                {
                    Debug.LogError("Banner ad failed to load: " + error);
                };

                // Other optional events
                m_bannerView.OnAdPaid += (AdValue adValue) =>
                {
                    Debug.Log("Banner ad paid: " + adValue.Value + " " + adValue.CurrencyCode);
                };
                m_bannerView.OnAdClicked += () =>
                {
                    Debug.Log("Banner ad clicked.");
                };
                m_bannerView.OnAdImpressionRecorded += () =>
                {
                    Debug.Log("Banner ad impression recorded.");
                };
                m_bannerView.OnAdFullScreenContentOpened += () =>
                {
                    Debug.Log("Banner ad fullscreen opened.");
                };
                m_bannerView.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Banner ad fullscreen closed.");
                };

                AdRequest l_request = new AdRequest();
                m_bannerView.LoadAd(l_request);
            }
            else
            {
                Debug.Log("BannerAdManager destroyed itself because no banner view exists");
                Destroy(gameObject);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"BannerAdManager destroyed itself because an exception occured: {e.Message}");
            Destroy(gameObject);
        }

    }
}
