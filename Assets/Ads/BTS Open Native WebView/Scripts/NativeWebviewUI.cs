using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Burningthumb.NativeWebview
{
    public class NativeWebViewUI : MonoBehaviour, INativeViewDelegate
    {
        [Header("UI Elements")]
        [SerializeField] private Button openWebViewButton;
        [SerializeField] private TMP_Text urlText;

        [Header("URL Elements")]
        [SerializeField] private string defaultURL = "https://burningthumb.com";
        [SerializeField] private string androidURL = "https://play.google.com/store/apps/details?id=com.burningthumb.mobile.mars2055";
        [SerializeField] private string appleURL = "https://apps.apple.com/us/app/mars-2055/id1591775159";
        [SerializeField] private string tvOSImageURL = "https://burningthumb.com/wp-content/uploads/bts-mars2055-s3.jpg";

        [Header("UI Times")]
        [SerializeField] private TMP_Text m_successTime;
        [SerializeField] private TMP_Text m_closedTime;
        [SerializeField] private TMP_Text m_failedTime;

        void Awake()
        {
            // Check if NativeWebView instance exists; if not, create it on this GameObject
            if (NativeWebView.instance == null)
            {
                Debug.Log("NativeWebView instance not found. Creating one on " + gameObject.name);
                gameObject.AddComponent<NativeWebView>();
                // After adding the component, Awake will have set NativeWebView.instance
            }

        }

        void Start()
        {

            NativeWebView.instance.SetDelegate(this);

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                urlText.text = appleURL;
            }
            else if (Application.platform == RuntimePlatform.tvOS)
            {
                urlText.text = tvOSImageURL;
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                urlText.text = appleURL;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                urlText.text = androidURL;
            }
            else
            {
                urlText.text = defaultURL;
            }

            openWebViewButton.onClick.AddListener(() =>
            {
                if (NativeWebView.instance != null)
                {
                    NativeWebView.instance.OpenInNativeView(urlText.text);
                }
                else
                {
                    Debug.LogError("NativeWebView instance is null. Cannot open web view.");
                }
            });
        }

        // Implement the delegate methods from INativeWebViewDelegate
        public void OnViewSuccess(string message)
        {
            Debug.Log("NativeWebViewUI received success: " + message);

            // Add any UI-specific logic here, e.g., update the UI to reflect success

            if (null != m_successTime)
            {
                m_successTime.text = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
            }
        }

        // Implement the delegate methods from INativeWebViewDelegate
        public void OnViewClose(string message)
        {
            Debug.Log("NativeWebViewUI received close: " + message);

            // Add any UI-specific logic here, e.g., update the UI to reflect success

            if (null != m_closedTime)
            {
                m_closedTime.text = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
            }
        }

        public void OnViewFailure(string message)
        {
            Debug.LogError("NativeWebViewUI received failure: " + message);

            // Add any UI-specific logic here, e.g., show an error message to the user

            if (null != m_failedTime)
            {
                m_failedTime.text = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
            }
        }
    }
}