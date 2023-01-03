using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mirror.Examples.PongGame
{
    public class PongGameScore : NetworkBehaviour
    {
        [SerializeField] int m_value;
        [SerializeField] PongGameScoreDigit m_tens;
        [SerializeField] PongGameScoreDigit m_ones;

        [SerializeField] string m_scoreSFX = "scoreSFX";

        public int Value
        {
            get
            {
                return m_value;
            }

            set
            {
                m_value = value;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            SetDigitsToValue();
        }

        private void SetDigitsToValue()
        {
            m_tens.SetValue(m_value / 10);
            m_ones.SetValue(m_value - (m_value / 10) * 10);
        }

        private void PlayAudio()
        {
            if (m_value > 0)
            {
                AudioManager.Play(m_scoreSFX, AudioManager.MixerTarget.SFX);
            }
        }


        public void SetValue(int a_int)
        {
            m_value = a_int;

            PlayAudio();
            SetDigitsToValue();
        }

        [ClientRpc]
        public void rpcSetValue(int a_int)
        {
            SetValue(a_int);
        }
    }
}
