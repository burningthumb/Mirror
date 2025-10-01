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
    public Sprite m_UISprite;
    public string m_SceneName;
}

public class BTSSelectScene : MonoBehaviour
{
    [Header("Scene Definitions")]
    [SerializeField] private SceneInfo[] m_Scenes;

    [Header("UI References")]
    [SerializeField] private TMP_Text m_UINameText;
    [SerializeField] private TMP_Text m_UIDescriptionText;
    [SerializeField] private Image m_UISpriteImage;

    private int m_CurrentIndex = 0;

    private static BTSSelectScene s_Instance;

    private void Awake()
    {
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

    // --- Class Methods (forward to instance) ---

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

    // --- Helpers ---

    private void UpdateUI()
    {
        if (null == m_Scenes || 0 == m_Scenes.Length) return;
        SceneInfo l_Info = m_Scenes[m_CurrentIndex];

        if (null != m_UINameText) m_UINameText.text = l_Info.m_UIName;
        if (null != m_UIDescriptionText) m_UIDescriptionText.text = l_Info.m_UIDescription;
        if (null != m_UISpriteImage) m_UISpriteImage.sprite = l_Info.m_UISprite;
    }
}
