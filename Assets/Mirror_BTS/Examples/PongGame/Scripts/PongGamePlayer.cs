using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Examples.PongGame
{
	public class PongGamePlayer : NetworkBehaviour
	{
		public float m_deadZone = 0.25f;
		public float speed = 30;
		public Rigidbody2D rigidbody2d;

		public Slider m_slider;

		float m_vertical;

		[ClientRpc]
		public void RpcSetSlider(int a_player)
		{
			if (!isLocalPlayer)
			{
				return;
			}

			m_slider = ((NetworkManagerPongGame)NetworkManager.singleton).GetSlider(a_player);
		}

		public override void OnStopLocalPlayer()
		{
			((NetworkManagerPongGame)NetworkManager.singleton).ActivateSliders(); ;
		}

		private void Update()
		{
			if (isLocalPlayer)
			{
				m_vertical = Input.GetAxisRaw("Vertical");
			}
		}

		// need to use FixedUpdate for rigidbody
		void FixedUpdate()
		{
			// only let the local player control the racket.
			// don't control other player's rackets
			if (isLocalPlayer)
			{
				if (!Mathf.Approximately(0.0f, m_vertical))
				{
					if (m_slider != null)
					{
						m_slider.value = 0;
						m_slider.gameObject.SetActive(false);
					}
				}

				rigidbody2d.velocity = new Vector2(0, m_vertical * speed) * Time.fixedDeltaTime;

				if (m_slider != null)
				{
					if (Mathf.Abs(m_slider.value) > m_deadZone)
					{
						rigidbody2d.velocity += new Vector2(0, m_slider.value) * speed * Time.fixedDeltaTime;
						//rigidbody2d.velocity += new Vector2(0, Mathf.Clamp(m_slider.value * 100, -1.0f, 1.0f)) * speed * Time.fixedDeltaTime;
					}
				}
			}
		}
	}
}
