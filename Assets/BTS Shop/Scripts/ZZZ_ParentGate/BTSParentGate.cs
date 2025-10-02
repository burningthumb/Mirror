using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSParentGate : MonoBehaviour
{
	public enum PGKey { kPGQuestion, kPGAnswer }

	public enum PGMessages { PGAnswer, PGSetQuestion }

	[SerializeField] bool m_forceIsIOS;
	[SerializeField] bool m_isIOS;

	[SerializeField] int m_answer;
	[SerializeField] int m_v1;
	[SerializeField] int m_o1;
	[SerializeField] int m_v2;

	void Awake()
	{
		m_isIOS = (Application.platform == RuntimePlatform.IPhonePlayer);

		if (m_forceIsIOS)
		{
			m_isIOS = true;
		}

		BTSLocalMessenger.RegisterFor(gameObject, PGMessages.PGAnswer.ToString());

		if (!m_isIOS)
		{
			gameObject.SetActive(false);
		}

	}

	void Start()
	{
		PGQuestion();
	}


	public void PGQuestion()
	{
		m_answer = Random.Range(1, 10);
		m_o1 = Random.Range(0, 2);

		string l_op_string = " plus ";

		if (0 == m_o1)
		{
			l_op_string = " minus ";
			m_o1 = -1; // Just as a hint for debugging

			m_v2 = Random.Range(0, 9 - m_answer);
			m_v1 = m_answer + m_v2;
		}
		else
		{
			m_v2 = Random.Range(0, m_answer);
			m_v1 = m_answer - m_v2;
		}

		string l_question = "What is " + ones(m_v1) + l_op_string + ones(m_v2);

		Hashtable l_params = new Hashtable();
		l_params.Add(PGKey.kPGQuestion.ToString(), l_question);

		BTSLocalMessenger.Send(PGMessages.PGSetQuestion.ToString(), l_params);

	}

	public void PGAnswer(Hashtable a_params)
	{
		int l_answer = -1;

		if (a_params.ContainsKey(BTSParentGate.PGKey.kPGAnswer.ToString()))
		{
			l_answer = (int)a_params[BTSParentGate.PGKey.kPGAnswer.ToString()];
		}

		if (l_answer != m_answer)
		{
			PGQuestion();
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private string ones(int a_number)
	{
		string l_name = "";

		switch (a_number)
		{
			case 0:
				l_name = "Zero";
				break;
			case 1:
				l_name = "One";
				break;
			case 2:
				l_name = "Two";
				break;
			case 3:
				l_name = "Three";
				break;
			case 4:
				l_name = "Four";
				break;
			case 5:
				l_name = "Five";
				break;
			case 6:
				l_name = "Six";
				break;
			case 7:
				l_name = "Seven";
				break;
			case 8:
				l_name = "Eight";
				break;
			case 9:
				l_name = "Nine";
				break;
		}

		return l_name;
	}
}
