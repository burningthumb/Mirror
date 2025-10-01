using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetSelectableTheme : MonoBehaviour
{
    [SerializeField] SOTheme m_Theme;

    public ColorBlock ThemeSelectableColorBlock
    {
        get
        {
            ColorBlock l_result = new ColorBlock();

            if (null != m_Theme)
            {
                l_result = m_Theme.SelectableColorBlock;
            }

            return l_result;
        }
    }

    void OnEnable()
    {
        Selectable l_selectable = GetComponent<Selectable>();

        if ((null != l_selectable) && (null != m_Theme))
        {
            l_selectable.colors = ThemeSelectableColorBlock;
        }

        Image l_image = GetComponent<Image>();
        if (null != l_image)
        {
            l_image.color = ThemeSelectableColorBlock.normalColor;
        }
    }
}
