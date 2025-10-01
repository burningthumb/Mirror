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
public class SceneInfo
{
    public string m_UIName;
    [TextArea] public string m_UIDescription;
    public string m_UIButtonHint;
    public Sprite m_UISprite;
    public string m_SceneName;
    public bool m_AdSupported;

}

public class BTSSelectScene : MonoBehaviour
{
    [Header("Scene Definitions")]
    [SerializeField] private SceneInfo[] m_Scenes;

    [Header("UI References")]
    [SerializeField] private TMP_Text m_UINameText;
    [SerializeField] private TMP_Text m_UIButtonHintText;
    [SerializeField] private TMP_Text m_UIDescriptionText;
    [SerializeField] private Image m_UISpriteImage;

    private int m_CurrentIndex = 0;

    private static BTSSelectScene s_Instance;

    public bool AdSupported
    {
        get
        {
            if (null == m_Scenes || 0 == m_Scenes.Length) return false;
            SceneInfo l_Info = m_Scenes[m_CurrentIndex];

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
    }

    private void Start()
    {
        UpdateUI();
    }

    // --- Instance Methods ---

    public void NextScene()
    {
        if (null == m_Scenes || 0 == m_Scenes.Length) return;
        m_CurrentIndex = (m_CurrentIndex + 1) % m_Scenes.Length;
        UpdateUI();
    }

    public void PreviousScene()
    {
        if (null == m_Scenes || 0 == m_Scenes.Length) return;
        m_CurrentIndex = (m_CurrentIndex - 1 + m_Scenes.Length) % m_Scenes.Length;
        UpdateUI();
    }

    public void LoadScene()
    {
        if (null == m_Scenes || 0 == m_Scenes.Length) return;
        SceneInfo l_Info = m_Scenes[m_CurrentIndex];
        if (!string.IsNullOrEmpty(l_Info.m_SceneName))
        {
            SceneManager.LoadScene(l_Info.m_SceneName);
        }
    }

    public void PlayAdOrLoadScene()
    {
        if (null == m_Scenes || 0 == m_Scenes.Length) return;

        SceneInfo l_Info = m_Scenes[m_CurrentIndex];

        if (!string.IsNullOrEmpty(l_Info.m_SceneName))
        {
            if (AdSupported)
            {
                AdmobAdSingletonUI l_ui = GetComponent<AdmobAdSingletonUI>();

                if (null != l_ui)
                { 
                    l_ui.ShowRV();
                }
            }
            else
            { 
                SceneManager.LoadScene(l_Info.m_SceneName);
            }
        }
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
            s_Instance.NextScene();
        }
    }

    public static void Previous()
    {
        if (null != s_Instance)
        {
            s_Instance.PreviousScene();
        }
    }

    public static void Load()
    {
        if (null != s_Instance)
        {
            s_Instance.LoadScene();
        }
    }

    public static void AdOrLoad()
    {
        if (null != s_Instance)
        {
            s_Instance.PlayAdOrLoadScene();
        }
    }

    // --- Helpers ---

    private void UpdateUI()
    {
        if (null == m_Scenes || 0 == m_Scenes.Length) return;
        SceneInfo l_Info = m_Scenes[m_CurrentIndex];

        if (null != m_UINameText) m_UINameText.text = l_Info.m_UIName;
        if (null != m_UIButtonHintText) m_UIButtonHintText.text = l_Info.m_UIButtonHint;
        if (null != m_UIDescriptionText) m_UIDescriptionText.text = l_Info.m_UIDescription;
        if (null != m_UISpriteImage) m_UISpriteImage.sprite = l_Info.m_UISprite;
    }
}
