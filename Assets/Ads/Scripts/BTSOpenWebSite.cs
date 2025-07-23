using System.Collections;
using System.Collections.Generic;
using Burningthumb.NativeWebview;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class BTSOpenWebSite : MonoBehaviour, INativeViewDelegate
{
	[SerializeField] UnityEvent m_onClose;

	static SOBTSAd m_btsAd;
	static string m_url;

	static public string URL
	{
		get => m_url;
		set => m_url = value;
	}

	static public SOBTSAd BTSAd
	{
		get => m_btsAd;
		set => m_btsAd = value;
	}

	EventSystem m_eventsystem;

	void Awake()
	{
		// Add both components if not present
		if (NativeWebView.instance == null)
		{
			Debug.Log("NativeWebView instance not found. Creating one on " + gameObject.name);
			gameObject.AddComponent<NativeWebView>();
		}
	}

	public void Start()
	{

	}

	public void OpenURL(string a_defaultURL)
	{
		if (null != BTSAd)
		{
			string l_url = $"{BTSAd.URL}?utm_source={UnityWebRequest.EscapeURL(BTSAd.GameID)}&utm_medium={UnityWebRequest.EscapeURL(BTSAd.Medium)}&utm_campaign={UnityWebRequest.EscapeURL(BTSAd.AdID)}";

			if (RuntimePlatform.tvOS == Application.platform || (RuntimePlatform.Android == Application.platform && BTS_TV_Type.IsAndroidTV))
			{
				l_url = $"{BTSAd.JpgURL}?utm_source={UnityWebRequest.EscapeURL(BTSAd.GameID)}&utm_medium={UnityWebRequest.EscapeURL(BTSAd.Medium)}&utm_campaign={UnityWebRequest.EscapeURL(BTSAd.AdID)}";
			}

			Debug.Log($"Opening WebView with URL: {l_url}");

			NativeWebView.instance.SetDelegate(this);
			NativeWebView.OpenInNativeWebViewStatic(l_url);

			GA4Analytics.ClickAd(BTSAd);
		}
		else if (null != URL)
		{
			Debug.Log($"Opening WebView with URL: {URL}");
			NativeWebView.instance.SetDelegate(this);
			NativeWebView.OpenInNativeWebViewStatic(URL);
		}
		else
		{
			Debug.Log($"Opening WebView with default URL: {a_defaultURL}");
			NativeWebView.instance.SetDelegate(this);
			NativeWebView.OpenInNativeWebViewStatic(a_defaultURL);
		}
	}

	// Implement the delegate methods from INativeWebViewDelegate
	public void OnViewSuccess(string message)
	{
		Debug.Log("NativeWebViewUI received success: " + message);

		// Add any UI-specific logic here, e.g., update the UI to reflect success

		m_eventsystem = EventSystem.current;
		m_eventsystem.enabled = false;
	}

	// Implement the delegate methods from INativeWebViewDelegate
	public void OnViewClose(string message)
	{
		Debug.Log("NativeWebViewUI received close: " + message);
		// Add any UI-specific logic here, e.g., update the UI to reflect success

		if (null != m_eventsystem)
		{
			m_eventsystem.enabled = true;
			m_eventsystem = null;
		}

		m_onClose.Invoke();
	}

	public void OnViewFailure(string message)
	{
		Debug.LogError("NativeWebViewUI received failure: " + message);

		// Add any UI-specific logic here, e.g., show an error message to the user

		if (null != m_eventsystem)
		{
			m_eventsystem.enabled = true;
			m_eventsystem = null;
		}
	}
}