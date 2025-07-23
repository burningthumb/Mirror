using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using System;
using AOT;

namespace Burningthumb.NativeWebview
{
    public interface INativeViewDelegate
    {
        void OnViewSuccess(string message);
        void OnViewClose(string message);
        void OnViewFailure(string message);
    }

    public abstract class NativeView : MonoBehaviour
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ViewCallback(string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ViewCallbackMacOS(string message, int cursorVisible, int cursorLocked);

        protected static NativeView instance; // Changed to protected for subclass access
        protected INativeViewDelegate viewDelegate;
        private bool isPaused = false;
        private EventSystem eventSystem;
        private bool savedCursorVisible; // Store cursor visibility
        private CursorLockMode savedCursorLockState; // Store cursor lock state

        protected virtual void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("No EventSystem found in the scene. UI pausing may not work as expected.");
            }
        }

        public void SetDelegate(INativeViewDelegate delegateObj)
        {
            viewDelegate = delegateObj;
        }

        [MonoPInvokeCallback(typeof(ViewCallback))]
        protected static void SuccessCallback(string message)
        {
            if (instance != null)
            {
                instance.OnViewSuccess(message);
            }
            else
            {
                Debug.LogWarning("No NativeView instance available for success callback");
            }
        }

        [MonoPInvokeCallback(typeof(ViewCallbackMacOS))]
        protected static void SuccessCallbackMacOS(string message, int cursorVisible, int cursorLocked)
        {
            if (instance != null)
            {
                instance.OnViewSuccessMacOS(message, cursorVisible, cursorLocked);
            }
            else
            {
                Debug.LogWarning("No NativeView instance available for macOS success callback");
            }
        }

        [MonoPInvokeCallback(typeof(ViewCallback))]
        protected static void CloseCallback(string message)
        {
            if (instance != null)
            {
                instance.OnViewClose(message);
            }
            else
            {
                Debug.LogWarning("No NativeView instance available for close callback");
            }
        }

        [MonoPInvokeCallback(typeof(ViewCallbackMacOS))]
        protected static void CloseCallbackMacOS(string message, int cursorVisible, int cursorLocked)
        {
            if (instance != null)
            {
                instance.OnViewCloseMacOS(message, cursorVisible, cursorLocked);
            }
            else
            {
                Debug.LogWarning("No NativeView instance available for macOS close callback");
            }
        }

        [MonoPInvokeCallback(typeof(ViewCallback))]
        protected static void FailureCallback(string message)
        {
            if (instance != null)
            {
                instance.OnViewFailure(message);
            }
            else
            {
                Debug.LogWarning("No NativeView instance available for failure callback");
            }
        }

        [MonoPInvokeCallback(typeof(ViewCallbackMacOS))]
        protected static void FailureCallbackMacOS(string message, int cursorVisible, int cursorLocked)
        {
            if (instance != null)
            {
                instance.OnViewFailureMacOS(message, cursorVisible, cursorLocked);
            }
            else
            {
                Debug.LogWarning("No NativeView instance available for macOS failure callback");
            }
        }

        public void OpenInNativeView(string a_url)
        {
            Debug.Log($"Platform: {Application.platform}");
            // Save cursor state before opening view
            savedCursorVisible = Cursor.visible;
            savedCursorLockState = Cursor.lockState;
            Debug.Log($"Saving cursor state - Visible: {savedCursorVisible}, Locked: {savedCursorLockState}");
            OpenNativeView(a_url);
        }

        protected abstract void OpenNativeView(string url);

        protected virtual void OnViewSuccess(string message)
        {
            Debug.Log("View loaded successfully: " + message);
            viewDelegate?.OnViewSuccess(message);
        }

        protected virtual void OnViewSuccessMacOS(string message, int cursorVisible, int cursorLocked)
        {
            Debug.Log($"macOS view loaded successfully: {message}, CursorVisible: {cursorVisible}, CursorLocked: {cursorLocked}");
            viewDelegate?.OnViewSuccess(message);
        }

        protected virtual void OnViewClose(string message)
        {
            Debug.Log("View closed successfully: " + message);
            viewDelegate?.OnViewClose(message);
        }

        protected virtual void OnViewCloseMacOS(string message, int cursorVisible, int cursorLocked)
        {
            Debug.Log($"macOS view closed successfully: {message}, CursorVisible: {cursorVisible}, CursorLocked: {cursorLocked}");
            RestoreCursorState(cursorVisible, cursorLocked);
            viewDelegate?.OnViewClose(message);
        }

        protected virtual void OnViewFailure(string message)
        {
            Debug.LogError("View failed: " + message);
            viewDelegate?.OnViewFailure(message);
        }

        protected virtual void OnViewFailureMacOS(string message, int cursorVisible, int cursorLocked)
        {
            Debug.LogError($"macOS view failed: {message}, CursorVisible: {cursorVisible}, CursorLocked: {cursorLocked}");
            RestoreCursorState(cursorVisible, cursorLocked);
            viewDelegate?.OnViewFailure(message);
        }

        private void RestoreCursorState(int cursorVisible, int cursorLocked)
        {
            try
            {
                Cursor.visible = cursorVisible != 0;
                Cursor.lockState = cursorLocked != 0 ? CursorLockMode.Locked : CursorLockMode.None;
                Debug.Log($"Restored cursor state - Visible: {Cursor.visible}, Locked: {Cursor.lockState}");
            }
            catch (Exception e)
            {
                // Fallback to saved states
                Cursor.visible = savedCursorVisible;
                Cursor.lockState = savedCursorLockState;
                Debug.LogError($"Failed to restore cursor state. Error: {e.Message}. Restored saved cursor state - Visible: {savedCursorVisible}, Locked: {savedCursorLockState}");
            }
        }
    }
}