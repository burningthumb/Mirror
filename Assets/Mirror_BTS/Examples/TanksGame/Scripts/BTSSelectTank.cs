//
// Author: Robert Wiebe
// Company: Burningthumb Studios
// Date: 2025 Oct 01
//

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TankInfo
{
    public string m_UIName;
    public string m_UISpeed;
    public string m_UITurret;
    public string m_UIShells;
    public string m_UIArmor;
    public string m_UIButtonHint;
    public GameObject m_UIMesh;
    public bool m_AdSupported;

}

public class BTSSelectTank : MonoBehaviour
{
    [Header("Scene Definitions")]
    [SerializeField] private TankInfo[] m_Tanks;

    [Header("UI References")]
    [SerializeField] private TMP_Text m_UINameText;
    [SerializeField] private TMP_Text m_UIButtonHintText;
    [SerializeField] private TMP_Text m_UISpeedText;
    [SerializeField] private TMP_Text m_UITurretText;
    [SerializeField] private TMP_Text m_UIShellsText;
    [SerializeField] private TMP_Text m_UIArmorText;

    [SerializeField] private AdmobAdSingletonUI m_AdmobAdSingletonUI;

    GameObject m_activeUIMesh = null;

    private int m_CurrentIndex = 0;

    private static BTSSelectTank s_Instance;

    public bool AdSupported
    {
        get
        {
            if (null == m_Tanks || 0 == m_Tanks.Length) return false;
            TankInfo l_Info = m_Tanks[m_CurrentIndex];

            return l_Info.m_AdSupported;
        }
    }

    private void Awake()
    {
        if (null != s_Instance)
        {
            Destroy(gameObject);
            return;
        }

        s_Instance = this;

        if (null == m_AdmobAdSingletonUI)
        {
            m_AdmobAdSingletonUI = FindFirstObjectByType<AdmobAdSingletonUI>();
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    // --- Instance Methods ---

    public void NextTank()
    {
        if (null == m_Tanks || 0 == m_Tanks.Length) return;
        m_CurrentIndex = (m_CurrentIndex + 1) % m_Tanks.Length;
        UpdateUI();
    }

    public void PreviousTank()
    {
        if (null == m_Tanks || 0 == m_Tanks.Length) return;
        m_CurrentIndex = (m_CurrentIndex - 1 + m_Tanks.Length) % m_Tanks.Length;
        UpdateUI();
    }

    // --- Class Methods (forward to instance) ---

    public static bool IsAdSupported()
    {
        if (null != s_Instance)
        {
            return s_Instance.AdSupported;
        }

        return false;
    }

    public static void Next()
    {
        if (null != s_Instance)
        {
            s_Instance.NextTank();
        }
    }

    public static void Previous()
    {
        if (null != s_Instance)
        {
            s_Instance.PreviousTank();
        }
    }

    // --- Helpers ---

    private void UpdateUI()
    {
        if (null == m_Tanks || 0 == m_Tanks.Length) return;

        TankInfo l_Info = m_Tanks[m_CurrentIndex];

        if (null != m_UINameText) m_UINameText.text = l_Info.m_UIName;
        if (null != m_UIButtonHintText) m_UIButtonHintText.text = l_Info.m_UIButtonHint;
        
        if (null != m_UISpeedText) m_UISpeedText.text = l_Info.m_UISpeed;
        if (null != m_UITurretText) m_UITurretText.text = l_Info.m_UITurret;
        if (null != m_UIShellsText) m_UIShellsText.text = l_Info.m_UIShells;
        if (null != m_UIArmorText) m_UIArmorText.text = l_Info.m_UIArmor;

        if (null != m_activeUIMesh)
        {
            m_activeUIMesh.SetActive(false);
        }

        if (null != l_Info.m_UIMesh)
        {
            m_activeUIMesh = l_Info.m_UIMesh;
            m_activeUIMesh.SetActive(true);
        }

        if (null != m_AdmobAdSingletonUI)
        {
            if (l_Info.m_AdSupported)
            {
                m_AdmobAdSingletonUI.SkipNextAd = false;
            }
            else
            {
                m_AdmobAdSingletonUI.SkipNextAd = true;
            }
        }

        ClientGlobals.SelectedTank = m_CurrentIndex;
        
    }
}
