using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.PongGame
{
	public class PongGameScoreCollider : MonoBehaviour
	{
		[SerializeField] PongGameScore m_score;

		public PongGameScore GetScore()
		{
			return m_score;
		}
	}
}
