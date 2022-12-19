using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Examples.PongGame
{
	public class PongGameScoreDigit : MonoBehaviour
	{
		[SerializeField] int m_value;
		[SerializeField] Sprite[] m_digits;

		[SerializeField] SpriteRenderer m_spriteRenderer;
		// Start is called before the first frame update
		void Start()
		{
			ShowDigit();
		}

		private void ShowDigit()
		{
			if (null == m_spriteRenderer)
			{
				m_spriteRenderer = GetComponent<SpriteRenderer>();
			}

			m_spriteRenderer.sprite = m_digits[m_value];
		}

		// Update is called once per frame
		public void SetValue(int a_int)
		{
			m_value = a_int;

			ShowDigit();
		}
	}

}
