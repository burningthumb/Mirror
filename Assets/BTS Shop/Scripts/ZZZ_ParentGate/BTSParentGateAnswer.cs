using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTSParentGateAnswer : MonoBehaviour
{
	[SerializeField] Text m_buttonText;
	// Start is called before the first frame update
	void Awake()
	{
		m_buttonText = GetComponentInChildren<Text>();
	}

	// Update is called once per frame
	public void SendAnswer()
	{
		int l_answer = 0;

		if (m_buttonText)
		{
			int.TryParse(m_buttonText.text, out l_answer);
		}

		Hashtable l_params = new Hashtable();

		l_params.Add(BTSParentGate.PGKey.kPGAnswer.ToString(), l_answer);

		BTSLocalMessenger.Send(BTSParentGate.PGMessages.PGAnswer.ToString(), l_params);

	}
}
