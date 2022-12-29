using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetTMPTextTheme : MonoBehaviour
{
    [SerializeField] SOTheme m_Theme;

    public Color ThemeTextColor
    {
        get
        {
            Color l_result = Color.black;

            if (null != m_Theme)
            {
                l_result = m_Theme.TextColor;
            }

            return l_result;
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        TMP_Text l_text = GetComponent<TMP_Text>();

        if ((null != l_text) && (null != m_Theme))
        {
            l_text.color = ThemeTextColor;
        }

    }
}
