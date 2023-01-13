using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace com.burningthumb.examples
{
    public class BTSTank : NetworkBehaviour
    {
        [SyncVar]
        int PlayerId = -1;

        [Header("Components")]
        public NavMeshAgent agent;
        public Animator animator;
        public TextMesh healthBar;
        public Transform turret;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Turret")]
        public KeyCode rotateLeftKey = KeyCode.Q;
        public KeyCode rotateRightKey = KeyCode.E;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public GameObject projectilePrefab;
        public Transform projectileMount;

        [Header("Stats")]
        [SyncVar] public int health = 4;

        public static HashSet<BTSTank> ActivePlayers = new HashSet<BTSTank>();
        public static Hashtable m_playerID = new Hashtable();

        public void Start()
        {
            ActivePlayers.Add(this);
            m_playerID.Add(this, m_playerID.Count);

            gameObject.name = "BTS Tank (" + m_playerID[this] + ")";
        }

        public void OnDestroy()
        {
            ActivePlayers.Remove(this);
            m_playerID.Remove(this);
        }

        void Update()
        {
            // always update health bar.
            // (SyncVar hook would only update on clients, not on server)
            healthBar.text = new string('-', health);

            // movement for local player
            if (isLocalPlayer)
            {
                // rotate
                float horizontal = Input.GetAxis("Horizontal");
                transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

                // move
                float vertical = Input.GetAxis("Vertical");
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                agent.velocity = forward * Mathf.Max(vertical, 0) * agent.speed;
                animator.SetBool("Moving", agent.velocity != Vector3.zero);

                // shoot
                if (Input.GetKeyDown(shootKey))
                {
                    CmdFire();
                }

                RotateTurret();
            }
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
            NetworkServer.Spawn(projectile);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }

        [ServerCallback]
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<BTSProjectile>() != null)
            {
                --health;
                if (health == 0)
                    NetworkServer.Destroy(gameObject);
            }
        }

        void RotateTurret()
        {
            //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //    RaycastHit hit;
            //    if (Physics.Raycast(ray, out hit, 100))
            //    {
            //        Debug.DrawLine(ray.origin, hit.point);
            //        Vector3 lookRotation = new Vector3(hit.point.x, turret.transform.position.y, hit.point.z);
            //        turret.transform.LookAt(lookRotation);
            //    }

            bool l_left = Input.GetKey(rotateLeftKey);
            bool l_right = Input.GetKey(rotateRightKey);

            if (l_left)
            {
                turret.transform.Rotate(0, -1 * rotationSpeed * Time.deltaTime, 0);
            }

            if (l_right)
            {
                turret.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }
        }
    }
}
