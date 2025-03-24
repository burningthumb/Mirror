using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoredActiveState : MonoBehaviour
{
    public enum StoredActiveStateKey { kUnknown,
        kSettingsButtonShow, kSettingsButtonHide, kSettingsPanel,
        kNetworkButtonShow, kNetworkButtonHide, kNetworkPanel }

    [SerializeField] StoredActiveStateKey m_sasKey;

    [SerializeField] bool m_Default;
    [SerializeField] bool m_Value;

    [SerializeField] GameObject m_gameObject;

    void Start()
    {
        if (null == m_gameObject)
        {
            m_gameObject = gameObject;
        }
        else
        {
            m_Value = Convert.ToBoolean(PlayerPrefs.GetInt(m_sasKey.ToString(), Convert.ToInt16(m_Default)));
            m_gameObject.SetActive(m_Value);
        }
    }

    public void SetValue(bool a_bool)
    {
        if (null == m_gameObject)
        {
            m_gameObject = gameObject;
        }

        m_Value = a_bool;
        SaveValue();
        m_gameObject.SetActive(m_Value);

    }


    public void SaveValue()
    {
        PlayerPrefs.SetInt(m_sasKey.ToString(), Convert.ToInt16(m_Value));
        PlayerPrefs.Save();
    }
}
