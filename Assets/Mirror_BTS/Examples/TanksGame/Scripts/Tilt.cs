using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilt : MonoBehaviour
{
    public LayerMask mask;
    public float speed = 1.0f;

    void LateUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {

            if ((hit.normal.x < 0) || (hit.normal.y < 0) || (hit.normal.z < 0))
            {
            }
            else
            {
                var slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal);
                transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation * transform.rotation, speed * Time.deltaTime);
            }

        }
    }
}