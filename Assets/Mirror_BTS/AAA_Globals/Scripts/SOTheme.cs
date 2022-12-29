using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New SOTheme", menuName 
= "SO/Theme")]
public class SOTheme : ScriptableObject
{
    [SerializeField] Color m_textColor;
    [SerializeField] ColorBlock m_selectableColorBlock;

    public Color TextColor
    {
        get
        {
            return m_textColor;
        }
    }

    public ColorBlock SelectableColorBlock
    {
        get
        {
            return m_selectableColorBlock;
        }
    }
}
