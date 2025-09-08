using System;
using Unity.Advertisement.IosSupport.Components;
using UnityEngine;

namespace Unity.Advertisement.IosSupport.Samples
{
    /// <summary>
    /// This component will trigger the context screen to appear when the scene starts,
    /// if the user hasn't already responded to the iOS tracking dialog.
    /// </summary>
    public class ContextScreenManager : MonoBehaviour
    {
        /// <summary>
        /// The prefab that will be instantiated by this component.
        /// The prefab has to have an ContextScreenView component on its root GameObject.
        /// </summary>
        /// 
        public bool m_doCheck = true;
        public ContextScreenView contextScreenPrefab;
        public GameObject m_mainCanvas;

        void Start()
        {
#if UNITY_IOS || UNITY_TVOS
            // check with iOS to see if the user has accepted or declined tracking

            if (m_doCheck)
            {
                // Only check one time
                m_doCheck = false;

                var status = ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;

                try
                {
                    Debug.Log($"ContextScreenManager about to invoke ATTrackingStatusBinding.GetAuthorizationTrackingStatus()");
                    status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                    Debug.Log($"ContextScreenManager ATTrackingStatusBinding.GetAuthorizationTrackingStatus SUCCESSFULLY RETURNED: {status}!");
                }
                catch (Exception e)
                {
                    Debug.Log($"ContextScreenManager caught exception {e.ToString()}");
                    status = ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;
                }


                Debug.Log($"ContextScreenManager status={status}");

                if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                {

                    if (null != m_mainCanvas)
                    {
                        m_mainCanvas.SetActive(false);
                    }

                    var contextScreen = Instantiate(contextScreenPrefab).GetComponent<ContextScreenView>();

                    // after the Continue button is pressed, and the tracking request
                    // has been sent, automatically destroy the popup to conserve memory
                    contextScreen.sentTrackingAuthorizationRequest += () =>
                    {
                        if (null != m_mainCanvas)
                        {
                            m_mainCanvas.SetActive(true);
                        }
                    };

                    if (null != contextScreen)
                    {
                        contextScreen.sentTrackingAuthorizationRequest += () => Destroy(contextScreen.gameObject);
                    }
                }
            }
            else
            {
                Debug.Log("Unity iOS Support: App Tracking Transparency status not checked, because the flag is false");
            }
#else
            Debug.Log("Unity iOS Support: App Tracking Transparency status not checked, because the platform is not iOS or tvOS.");
#endif
        }
    }
}
