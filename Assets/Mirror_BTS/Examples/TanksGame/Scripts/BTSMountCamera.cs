using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BTSMountCamera : NetworkBehaviour
{
    [SerializeField] Camera m_mainCamera;
    [SerializeField] Transform m_cameraMount;

    Vector3 m_savePosition;
    Quaternion m_saveRotation;
    Transform m_saveParent;

    // Start is called before the first frame update
    void Start()
    {
        m_mainCamera = Camera.main;

        if (isLocalPlayer)
        {
            m_saveParent = m_mainCamera.transform.parent;
            m_savePosition = m_mainCamera.transform.position;
            m_saveRotation = m_mainCamera.transform.rotation;

            m_mainCamera.transform.parent = m_cameraMount;
            m_mainCamera.transform.localPosition = Vector3.zero;
            m_mainCamera.transform.localRotation = Quaternion.identity;
        }

    }

    public void OnDestroy()
    {
        if (isLocalPlayer)
        {
            m_mainCamera.transform.parent = m_saveParent;
            m_mainCamera.transform.rotation = m_saveRotation;
            m_mainCamera.transform.position = m_savePosition;
        }
    }
}
