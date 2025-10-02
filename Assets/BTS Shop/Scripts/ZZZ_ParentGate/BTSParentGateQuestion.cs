using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTSParentGateQuestion : MonoBehaviour
{
    [SerializeField] Text m_answerText;
    // Start is called before the first frame update
    void Awake()
    {
       m_answerText = GetComponent<Text>();

       BTSLocalMessenger.RegisterFor(gameObject, BTSParentGate.PGMessages.PGSetQuestion.ToString());
        
    }

    // Update is called once per frame
    public void PGSetQuestion(Hashtable a_params)
    {
        if (m_answerText)
		{
            if (a_params.ContainsKey(BTSParentGate.PGKey.kPGQuestion.ToString()))
			{
                m_answerText.text = (string)a_params[BTSParentGate.PGKey.kPGQuestion.ToString()];
			}
		}
    }
}
