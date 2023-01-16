using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilt : MonoBehaviour
{
    public Transform m_raycastTransform;
    public LayerMask mask;
    public float speed = 1.0f;

    public void Start()
    {
        if (null == m_raycastTransform)
        {
            m_raycastTransform = transform;
        }
    }

    void LateUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(m_raycastTransform.position, -m_raycastTransform.up, out hit))
        {
            var slopeRotation = Quaternion.FromToRotation(m_raycastTransform.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation * transform.rotation, speed * Time.deltaTime);
        }

    }
}
