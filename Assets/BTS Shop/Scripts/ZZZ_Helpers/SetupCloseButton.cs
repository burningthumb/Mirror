using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupCloseButton : MonoBehaviour
{
    [SerializeField] HandleShopButton m_handleShopButton;

    [SerializeField] Canvas m_disableCanvas;
	[SerializeField] Canvas m_enableCanvas;
	[SerializeField] Button m_selectButton;

    
    public void OnSetupCloseButton()
    {
        if (m_handleShopButton)
		{
            m_handleShopButton.DisableCanvas = m_disableCanvas;
            m_handleShopButton.EnableCanvas = m_enableCanvas;
            m_handleShopButton.SelectButton = m_selectButton;
		}
        
    }
}
