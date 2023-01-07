using UnityEngine;
using UnityEngine.UI;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace Mirror.Examples.PongGame
{

    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    public class NetworkManagerPongGame : NetworkManagerDelegates
    {
        public int m_gamePlayers = 2;

        public Transform m_player1SpawnLeft;
        public Transform m_player2SpawnRight;

        public Transform m_player3SpawnLeft;
        public Transform m_player4SpawnRight;


        public PongGameScoreCollider m_leftScore;
        public PongGameScoreCollider m_rightScore;

        public Slider m_leftSlider;
        public Slider m_rightSlider;
        public Vector3 m_ballSpawnPos = Vector3.zero;

        public PongGameController m_gameController;

        public PongGameScoreCollider[] m_activateThese;

        GameObject m_ballGO;
        PongGameBall m_ball;

        public PongGameBall GetBall()
        {
            return m_ball;
        }

        public void ActivateSliders()
        {
            m_rightSlider.gameObject.SetActive(true);
            m_leftSlider.gameObject.SetActive(true);
        }

        public PongGameController GetGameController()
        {
            return m_gameController;
        }

        public Slider GetSlider(int a_playerNum)
        {
            Slider l_result;
            Slider l_disable;

            switch (a_playerNum)
            {
                case 0:
                case 2:
                    l_result = m_leftSlider;
                    l_disable = m_rightSlider;
                    break;

                case 1:
                case 3:
                    l_result = m_rightSlider;
                    l_disable = m_leftSlider;
                    break;

                default:
                    l_disable = m_rightSlider;
                    l_result = m_leftSlider;
                    break;

            }

            l_disable.gameObject.SetActive(false);
            return l_result;

        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Transform start;

            switch (numPlayers)
            {
                case 0:
                    start = m_player1SpawnLeft;
                    break;
                case 1:
                    start = m_player2SpawnRight;
                    break;
                case 2:
                    start = m_player3SpawnLeft;
                    break;
                case 3:
                    start = m_player4SpawnRight;
                    break;
                default:
                    start = m_player1SpawnLeft;
                    break;
            }

            GameObject playerGO = Instantiate(playerPrefab, start.position, start.rotation);

            // *** WARNING ***
            // This adds 1 to numPlayers
            NetworkServer.AddPlayerForConnection(conn, playerGO);

            PongGamePlayer l_player = playerGO.GetComponent<PongGamePlayer>();

            // *** WARNING ***
            // numPlayers was incremented by 1 when the player was added for the connection
            l_player.RpcSetSlider(numPlayers - 1);

            ClearScore();

            // spawn ball if two players -- more players are allowed to join
            if (numPlayers == m_gamePlayers)
            {
                float random = Random.Range(0f, 260f);
                Vector2 l_direction = new Vector2(Mathf.Cos(random), Mathf.Sin(random));
                SpawnBall(l_direction);
            }
        }

        public void SpawnBall(Vector2 a_direction)
        {
            m_ballGO = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "PongGameBall"), m_ballSpawnPos, Quaternion.identity);
            m_ball = m_ballGO.GetComponent<PongGameBall>();
            m_ball.m_direction = a_direction;

            NetworkServer.Spawn(m_ballGO);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {

            // End the game if there is only 1 player

            if (1 == numPlayers)
            {
                if (null != m_gameController)
                {
                    if (false == m_gameController.GameOver)
                    {
                        m_gameController.GameOver = true;
                    }
                }

                // Clear the score
                ClearScore();

                // destroy ball
                if (m_ballGO != null)
                {

                    m_gameController.GameOver = true;

                    NetworkServer.Destroy(m_ballGO);
                }
            }

            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }

        public void ClearScore()
        {

            m_activateThese = GameObject.FindObjectsOfType<PongGameScoreCollider>();

            foreach (PongGameScoreCollider l_pgsc in m_activateThese)
            {
                if (l_pgsc.DestroyOnScore)
                {
                    l_pgsc.rpcShow();
                }
            }

            PongGameScore l_score = m_leftScore.GetPongGameScore;

            if (null != l_score)
            {
                l_score.rpcSetValue(0);
            }

            l_score = m_rightScore.GetPongGameScore;

            if (null != l_score)
            {
                l_score.rpcSetValue(0);
            }
        }
    }
}
