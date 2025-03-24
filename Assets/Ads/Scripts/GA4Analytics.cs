using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public static class GA4Analytics
{
    public enum WEB_EVENT { btsad_impression, btsad_click}
    private const string MEASUREMENT_ID = "G-0TXRZ02PR6"; // Your GA4 Measurement ID
    private const string API_SECRET = "OjRUCw-tTPumekrBgk2LHQ";   // Your GA4 API Secret
    private const string ENDPOINT = "https://www.google-analytics.com/mp/collect";
    private static string clientId;

    static GA4Analytics()
    {
        EnsureCoroutineRunner();

        clientId = GetOrGenerateClientId();
        Debug.Log("ClientId initialized: " + clientId);
    }

    public static void DisplayAd(SOBTSAd ad)
    {
        EnsureCoroutineRunner();
        GA4CoroutineRunner.Instance.StartCoroutine(TrackAd(WEB_EVENT.btsad_impression, ad));
    }

    public static void ClickAd(SOBTSAd ad)
    {
        EnsureCoroutineRunner();
        GA4CoroutineRunner.Instance.StartCoroutine(TrackAd(WEB_EVENT.btsad_click, ad));
    }

    private static IEnumerator TrackAd(WEB_EVENT a_webEvent, SOBTSAd ad)
    {
        string url = $"{ENDPOINT}?measurement_id={MEASUREMENT_ID}&api_secret={API_SECRET}";

        var l_webparams = new GA4EventParams
        {
            utm_source = ad.GameID,
            utm_medium = ad.Medium,
            utm_campaign = ad.AdID,
            utm_source_platform = ad.Platform
        };

        string jsonPayload = CreateJsonPayload(a_webEvent.ToString(), l_webparams);
        Debug.Log("Payload: " + jsonPayload);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"GA4 Event {a_webEvent} sent successfully. Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"Error sending GA4 event {a_webEvent}: {request.error}. Response: {request.downloadHandler.text}");
        }
    }

    [System.Serializable]
    private class GA4EventPayload
    {
        public string client_id;
        public GA4Event[] events;
    }

    [System.Serializable]
    private class GA4Event
    {
        public string name;
        public GA4EventParams @params;
    }

    [System.Serializable]
    private class GA4EventParams
    {
        public string utm_source;
        public string utm_medium;
        public string utm_campaign;
        public string utm_source_platform;
    }

    private static string CreateJsonPayload(string eventName, GA4EventParams parameters)
    {
        var payload = new GA4EventPayload
        {
            client_id = clientId,
            events = new[]
            {
                new GA4Event
                {
                    name = eventName,
                    @params = parameters
                }
            }
        };

        string json = JsonUtility.ToJson(payload);
        return json;
    }

    private static string GetOrGenerateClientId()
    {
        const string CLIENT_ID_KEY = "GA4ClientId";
        if (PlayerPrefs.HasKey(CLIENT_ID_KEY))
        {
            return PlayerPrefs.GetString(CLIENT_ID_KEY);
        }
        string newClientId = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString(CLIENT_ID_KEY, newClientId);
        PlayerPrefs.Save();
        return newClientId;
    }

    private static void EnsureCoroutineRunner()
    {
        if (GA4CoroutineRunner.Instance == null)
        {
            GameObject runnerObject = new GameObject("GA4CoroutineRunner");
            GA4CoroutineRunner runner = runnerObject.AddComponent<GA4CoroutineRunner>();
            runner.Initialize(); // Explicitly initialize to set the instance
            Object.DontDestroyOnLoad(runnerObject);
        }
    }
}

public class GA4CoroutineRunner : MonoBehaviour
{
    private static GA4CoroutineRunner instance;

    public static GA4CoroutineRunner Instance
    {
        get
        {
            return instance;
        }
    }

    // Explicit initialization method
    public void Initialize()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}