using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static StoredValue;

public class CameraSettings : MonoBehaviour
{
    [SerializeField] StoredValueKey m_orthographicSizeKey;

    // Start is called before the first frame update
    void Start()
    {
        float l_floatValue = PlayerPrefs.GetFloat(m_orthographicSizeKey.ToString(), 50.0f);
        Camera.main.orthographicSize = l_floatValue;
    }
}
