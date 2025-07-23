using UnityEngine;
using System.Runtime.InteropServices;
using System;
using AOT;

namespace Burningthumb.NativeWebview
{
    public class NativeWebView : NativeView
    {
        public new static NativeWebView instance => (NativeWebView)NativeView.instance;

        public static void OpenInNativeWebViewStatic(string a_url)
        {
            if (instance == null)
            {
                Debug.LogError("NativeWebView instance not found. Please add NativeWebView component to a GameObject.");
                return;
            }
            instance.OpenInNativeView(a_url);
        }

#if UNITY_STANDALONE_OSX
        [DllImport("__Internal")]
        private static extern void OpenNativeWebView(string url, string gameObjectName, 
            ViewCallbackMacOS successCallback, ViewCallbackMacOS closeCallback, ViewCallbackMacOS failureCallback,
            int cursorVisible, int cursorLocked);
#endif

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void OpenNativeWebView(string url, string gameObjectName,
            ViewCallback successCallback, ViewCallback closeCallback, ViewCallback failureCallback);
#endif

#if UNITY_TVOS
        [DllImport("__Internal")]
        private static extern void OpenNativeWebView(string url, string gameObjectName,
            ViewCallback successCallback, ViewCallback closeCallback, ViewCallback failureCallback);
#endif

        protected override void OpenNativeView(string url)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                Debug.Log("Opening WebView on Android");
                try
                {
                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaClass webViewPlugin = new AndroidJavaClass("com.burningthumb.mobile.webview.WebViewPlugin");

                    if (TV_Type.IsAndroidTV)
                    {
                        webViewPlugin.CallStatic("openNativeImageView", currentActivity, url, gameObject.name, "OnViewSuccess", "OnViewClose", "OnViewFailure");
                    }
                    else
                    {
                        webViewPlugin.CallStatic("openNativeWebView", currentActivity, url, gameObject.name, "OnViewSuccess", "OnViewClose", "OnViewFailure");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Android WebView failed: " + e.Message);
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                try
                {
                    OpenNativeWebView(url, gameObject.name, SuccessCallback, CloseCallback, FailureCallback);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"iOS WebView failed: {e.Message}");
                }
#else
                Debug.LogError("WebView not supported in this build.");
                Application.OpenURL(url);
#endif
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer)
            {
#if UNITY_STANDALONE_OSX
                try
                {
                    Debug.Log("Opening WebView on macOS");
                    int cursorVisible = Cursor.visible ? 1 : 0;
                    int cursorLocked = (Cursor.lockState == CursorLockMode.Locked || Cursor.lockState == CursorLockMode.Confined) ? 1 : 0;
                    OpenNativeWebView(url, gameObject.name, SuccessCallbackMacOS, CloseCallbackMacOS, FailureCallbackMacOS, cursorVisible, cursorLocked);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"macOS WebView failed: {e.Message}");
                }
#else
                Debug.LogError("WebView not supported in this build.");
                Application.OpenURL(url);
#endif
            }
            else if (Application.platform == RuntimePlatform.tvOS)
            {
#if UNITY_TVOS
                Debug.Log("Opening ImageView on tvOS");
                try
                {
                    OpenNativeWebView(url, gameObject.name, SuccessCallback, CloseCallback, FailureCallback);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"tvOS ImageView failed: {e.Message}");
                }
#else
                Debug.LogError("ImageView not supported in this build.");
                Application.OpenURL(url);
#endif
            }
            else
            {
                Debug.Log("WebView only supported on Android (non-TV)/iOS/macOS. Opening URL in browser.");
                Application.OpenURL(url);
            }
        }
    }
}