using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowVersion : MonoBehaviour
{
    TMP_Text m_versionTest;

    // Start is called before the first frame update
    void Start()
    {
        if (null == m_versionTest)
        {
            m_versionTest = GetComponent<TMP_Text>();
        }

        if (null != m_versionTest)
        {
            m_versionTest.text = "v" + Application.version;
        }
    }
}
