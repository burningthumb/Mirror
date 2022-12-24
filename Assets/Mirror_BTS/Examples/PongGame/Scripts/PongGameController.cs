using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.PongGame;
using UnityEngine;

namespace Mirror.Examples.PongGame
{
    public class PongGameController : NetworkBehaviour
    {
        [SyncVar(hook = nameof(SetGameOver))]
        public bool m_gameOver = true;

        [SerializeField] GameObject m_gameOverGO;
        [SerializeField] GameObject m_startButtonGO;

        public bool GameOver
        {
            get
            {
                return m_gameOver;
            }

            set
            {
                m_gameOver = value;
            }
        }

        public void StartGame()
        {
            ((NetworkManagerPongGame)NetworkManager.singleton).ClearScore();

            PongGameBall l_ball = ((NetworkManagerPongGame)NetworkManager.singleton).GetBall();

            if (null != l_ball)
            {
                l_ball.rpcResetBall();
            }

            GameOver = false;
        }

        public void SetGameOver(bool a_old, bool a_new)
        {

            if (m_gameOverGO)
            {
                m_gameOverGO.SetActive(a_new);

                HideShowStartButton(a_new);
            }
        }

        public void HideShowStartButton(bool a_flag)
        {
            if (isServer)
            { 
                m_startButtonGO.SetActive(a_flag);
            }
        }
    }
}
