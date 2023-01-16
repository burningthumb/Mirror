using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Mirror;
using UnityEngine;

public class BTSMountCamera : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera m_cinemachineVirtualCamera;
    [SerializeField] CinemachineTransposer m_cinemachineTransposer;
    [SerializeField] Transform m_cameraMount;
    [SerializeField] Vector3 m_tankFollowOffset = Vector3.zero;

    Vector3 m_savePosition;
    Quaternion m_saveRotation;
    Transform m_saveParent;

    Transform m_saveFollow;
    Transform m_saveLookAt;
    Vector3 m_saveOffset;

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            m_cinemachineVirtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
            m_cinemachineTransposer = m_cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();

            m_saveFollow = m_cinemachineVirtualCamera.Follow;
            m_saveLookAt = m_cinemachineVirtualCamera.LookAt;
            m_saveOffset = m_cinemachineTransposer.m_FollowOffset;

            //m_saveParent = m_cinemachineVirtualCamera.transform.parent;
            //m_savePosition = m_cinemachineVirtualCamera.transform.position;
            //m_saveRotation = m_cinemachineVirtualCamera.transform.rotation;

            //m_cinemachineVirtualCamera.transform.parent = m_cameraMount;
            //m_cinemachineVirtualCamera.transform.localPosition = Vector3.zero;
            //m_cinemachineVirtualCamera.transform.localRotation = Quaternion.identity;
            //m_cinemachineVirtualCamera.transform.parent = null;

            m_cinemachineVirtualCamera.Follow = m_cameraMount;
            m_cinemachineVirtualCamera.LookAt = m_cameraMount;
            m_cinemachineTransposer.m_FollowOffset = m_tankFollowOffset;

            FollowTarget l_followTarget = m_cinemachineVirtualCamera.gameObject.GetComponent<FollowTarget>();
            if (l_followTarget)
            {
                l_followTarget.target = m_cameraMount.transform;
            }
        }

    }

    public void OnDestroy()
    {
        if (isLocalPlayer)
        {
            //m_mainCamera.transform.parent = m_saveParent;
            if (m_cinemachineVirtualCamera)
            {
                //m_cinemachineVirtualCamera.transform.rotation = m_saveRotation;
                //m_cinemachineVirtualCamera.transform.position = m_savePosition;
                m_cinemachineVirtualCamera.Follow = m_saveFollow;
                m_cinemachineVirtualCamera.LookAt = m_saveLookAt;
                m_cinemachineTransposer.m_FollowOffset = m_saveOffset;
            }
        }
    }
}
