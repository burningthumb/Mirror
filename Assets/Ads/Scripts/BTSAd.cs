using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTSAd : MonoBehaviour
{
    [SerializeField] Button m_buttonMoreInfo;
   [SerializeField] Button m_buttonContinue;

	public void OnEnable()
	{
		m_buttonMoreInfo.Select();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonContinue()
	{
        AdmobAdSingleton.ContinueFromBTSAd();
	}
}
