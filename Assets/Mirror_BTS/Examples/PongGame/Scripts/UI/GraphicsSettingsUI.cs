using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Rendering.PostProcessing;

public class GraphicsSettingsUI : MonoBehaviour
{
    public string m_qualityKey = "GrahpicsQuality";

    public TMP_Dropdown graphicsDropdown;
    //public Toggle postprocessingToggle;

    private void Awake()
    {
        InitGraphicsDropdown();
    }

    public void InitGraphicsDropdown()
    {
        string[] names = QualitySettings.names;
        List<string> options = new List<string>();

        for (int i = 0; i < names.Length; i++)
        {
            options.Add(names[i]);
        }
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(options);

        int l_quality = Mathf.Max(0, (int)(graphicsDropdown.options.Count / 2 - 0.5f));

        l_quality = PlayerPrefs.GetInt(m_qualityKey, l_quality);

        QualitySettings.SetQualityLevel(l_quality);
        graphicsDropdown.value = l_quality;
    }

    public void SetGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);

        PlayerPrefs.SetInt(m_qualityKey, graphicsDropdown.value);
        PlayerPrefs.Save();
    }

    public void TogglePostProcessing()
    {
        //if (Camera.main.TryGetComponent(out PostProcessLayer ppl))
        //{
        //    ppl.enabled = postprocessingToggle.isOn;
        //}
    }
}
