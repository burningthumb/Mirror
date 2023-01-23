using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnityAdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public enum keys { AdDiceRoll };

    [SerializeField] int m_minDice = 3;

    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;

    [SerializeField] Button m_playButton;
    [SerializeField] TMP_Text m_playText;
    [SerializeField] TMP_Text m_hintText;
    [SerializeField] Image m_playImage;

    [SerializeField] string m_enabledString = "Show Ad";
    [SerializeField] string m_disabledString = "Waiting...";
    [SerializeField] string m_unsupportedPlatformString = "Play";

    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";

    [SerializeField] Sprite m_enabledSprite;
    [SerializeField] Sprite m_disabledSprite;

    [SerializeField] UnityEvent m_adPlayed;

    string _adUnitId = null; // This will remain null for unsupported platforms

    private string _gameId;

    private int m_adDiceRoll = 7;
    private bool m_needToShowAd = false;

    void Awake()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        m_adDiceRoll = PlayerPrefs.GetInt(keys.AdDiceRoll.ToString(), 6);
#endif

        if (m_adDiceRoll > m_minDice)
        {
            m_needToShowAd = false;
            PlayerPrefs.DeleteKey(keys.AdDiceRoll.ToString());
            PlayerPrefs.Save();
        }
        else
        {
            m_needToShowAd = true;
        }


        Debug.Log(m_adDiceRoll + " " + m_needToShowAd);

#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        if (m_needToShowAd)
        {
            InitializeAds();
        }
        else
        {
            m_playButton.interactable = true;
            m_playText.text = m_unsupportedPlatformString;
            m_hintText.text = "";
            m_playImage.sprite = m_enabledSprite;
        }

    }

    public void InitializeAds()
    {
        EnablePlayButton(false);

#if UNITY_IOS
        _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#endif

        //_gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
        //? _iOSGameId
        //: _androidGameId;

        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        else
        {
            LoadRV();
        }
    }

    IEnumerator RetryInitialize()
    {

        yield return new WaitForSeconds(5.0f);

        InitializeAds();

    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");

        LoadRV();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");

        // Try to initialize again
        StopCoroutine(RetryInitialize());
        StartCoroutine(RetryInitialize());
    }


    void LoadRV()
    {
        // IMPORTANT: Only load content AFTER initialization
        Advertisement.Load(_adUnitId, this);
    }

    IEnumerator RetryLoadRV()
    {

        yield return new WaitForSeconds(5.0f);

        LoadRV();

    }

    public void ShowRV()
    {

        if (m_needToShowAd)
        {
            Advertisement.Show(_adUnitId, this);
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

    public void OnUnityAdsAdLoaded(string a_placementId)
    {
        Debug.Log("Ad Loaded: " + a_placementId);

        if (a_placementId.Equals(_adUnitId))
        {
            EnablePlayButton(true);
        }
        else
        {
            EnablePlayButton(false);

            // Try to load again
            StopCoroutine(RetryLoadRV());
            StartCoroutine(RetryLoadRV());
        }
    }

    public void OnUnityAdsFailedToLoad(string a_placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {a_placementId}: {error.ToString()} - {message}");

        EnablePlayButton(false);

        // Try to load again
        StopCoroutine(RetryLoadRV());
        StartCoroutine(RetryLoadRV());
    }

    public void OnUnityAdsShowFailure(string a_placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {a_placementId}: {error.ToString()} - {message}");

        // Try to show again
        StopCoroutine(RetryShowRV());
        StartCoroutine(RetryShowRV());
    }

    public void OnUnityAdsShowComplete(string a_placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (a_placementId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            PlayerPrefs.DeleteKey(UnityAdManager.keys.AdDiceRoll.ToString());
            PlayerPrefs.Save();

            m_adPlayed.Invoke();
        }
        else
        {
            EnablePlayButton(false);

            // Try to load again
            StopCoroutine(RetryLoadRV());
            StartCoroutine(RetryLoadRV());
        }
    }

    public void OnUnityAdsShowStart(string placementId)
    {

    }

    public void OnUnityAdsShowClick(string placementId)
    {

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