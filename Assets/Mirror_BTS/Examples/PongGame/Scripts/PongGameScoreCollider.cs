using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.PongGame
{
	public class PongGameScoreCollider : NetworkBehaviour
	{
		[SerializeField] int m_scoreValue = 1;
		[SerializeField] bool m_repositionOnScore = true;
		[SerializeField] bool m_destroyOnScore = false;

		[SerializeField] PongGameScore m_score;

		[SerializeField] string m_blockDestroySFX = "blockDestroySFX";

		SpriteRenderer m_spriteRenderer;
		BoxCollider2D m_boxCollider2D;

		[ClientRpc]
        public void rpcHide()
        {
            AudioManager.Play(m_blockDestroySFX, AudioManager.MixerTarget.SFX);

			if (null != m_spriteRenderer)
			{ 
				m_spriteRenderer.enabled = false;
			}

			if (null != m_boxCollider2D)
            {
				m_boxCollider2D.enabled = false;
            }
        }

		internal void rpcShow()
        {
 			if (null != m_spriteRenderer)
			{ 
				m_spriteRenderer.enabled = true;
			}

			if (null != m_boxCollider2D)
            {
				m_boxCollider2D.enabled = true;
            }        }

        public void Start()
        {
			m_spriteRenderer = GetComponent<SpriteRenderer>();
			m_boxCollider2D = GetComponent<BoxCollider2D>();
            
        }

        public int ScoreValue
		{
			get
            {
				return m_scoreValue;
            }
		}

		public bool RepositionOnScore
		{
			get
            {
				return m_repositionOnScore;
            }
		}

		public bool DestroyOnScore
		{
			get
            {
				return m_destroyOnScore;
            }
		}

		public PongGameScore GetPongGameScore
		{
            get
			{ 
				return m_score;
			}
		}
    }
}
