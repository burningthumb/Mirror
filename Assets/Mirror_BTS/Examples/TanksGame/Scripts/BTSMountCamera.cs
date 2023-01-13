using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BTSMountCamera : NetworkBehaviour
{
    [SerializeField] Camera m_mainCamera;
    [SerializeField] Transform m_cameraMount;

    // Start is called before the first frame update
    void Start()
    {
        m_mainCamera = Camera.main;

        if (isClient)
        {
            m_mainCamera.transform.parent = m_cameraMount;
            m_mainCamera.transform.localPosition = Vector3.zero;
            m_mainCamera.transform.localRotation = Quaternion.identity;
        }

    }
}
