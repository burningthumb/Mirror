using UnityEngine;

namespace Mirror.Examples.PongGame
{
    public class PongGameBall : NetworkBehaviour
    {

        public float speed = 30;
        public Vector2 m_direction = Vector2.right;
        public Rigidbody2D rigidbody2d;

        [SerializeField] string m_ballBounceSFX = "ballBounceSFX";

        PongGameController m_pongGameController;

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Get a reverence to the GameController
            m_pongGameController = ((NetworkManagerPongGame)(NetworkManager.singleton)).GetGameController();

            // only simulate ball physics on server
            rigidbody2d.simulated = true;

            // Serve the ball from left player
            rigidbody2d.velocity = m_direction * speed;
        }

        float HitFactor(Vector2 ballPos, Vector2 racketPos, float racketHeight)
        {
            // ascii art:
            // ||  1 <- at the top of the racket
            // ||
            // ||  0 <- at the middle of the racket
            // ||
            // || -1 <- at the bottom of the racket
            return (ballPos.y - racketPos.y) / racketHeight;
        }

        [ClientRpc]
        public void rpcResetBall()
        {
            rigidbody2d.position = Vector2.zero;
        }

        [ClientRpc]
        public void rpcPlayBounceSound()
        {
            AudioManager.Play(m_ballBounceSFX, AudioManager.MixerTarget.SFX);
        }

        // only call this on server
        [ServerCallback]
        void OnCollisionEnter2D(Collision2D col)
        {
            // Note: 'col' holds the collision information. If the
            // Ball collided with a racket, then:
            //   col.gameObject is the racket
            //   col.transform.position is the racket's position
            //   col.collider is the racket's collider

            PongGamePlayer l_player = col.transform.GetComponent<PongGamePlayer>();

            PongGameScoreCollider l_scoreCollider = col.transform.GetComponent<PongGameScoreCollider>();

            // did we hit a racket? then we need to calculate the hit factor
            if (null != l_player)
            {
                // Calculate y direction via hit Factor
                float y = HitFactor(transform.position,
                                    col.transform.position,
                                    col.collider.bounds.size.y);

                // Calculate x direction via opposite collision
                float x = col.relativeVelocity.x > 0 ? 1 : -1;

                // Calculate direction, make length=1 via .normalized
                Vector2 dir = new Vector2(x, y).normalized;

                // Set Velocity with dir * speed
                rigidbody2d.velocity = dir * speed;

                // Play the bounce audio
                if (!m_pongGameController.GameOver)
                {
                    rpcPlayBounceSound();
                }

            }
            else if (null != l_scoreCollider)
            {

                if (!m_pongGameController.GameOver)
                {
                    PongGameScore l_score = l_scoreCollider.GetScore();

                    if (null != l_score)
                    {
                        l_score.rpcSetValue(l_score.Value + 1);

                        if (l_score.Value >= 20)
                        {
                            m_pongGameController.GameOver = true;
                        }
                    }

                    rigidbody2d.position = Vector2.zero;
                }
            }
            else
            {
                // Play the bounce audio
                if (!m_pongGameController.GameOver)
                {
                    rpcPlayBounceSound();
                }
            }
        }
    }
}
