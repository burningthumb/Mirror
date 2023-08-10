using System.Collections;
using System.Collections.Generic;
using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace com.burningthumb.examples
{
    public class BTSTank : NetworkBehaviour
    {
        public static HashSet<BTSTank> ActivePlayers = new HashSet<BTSTank>();
        public static Hashtable m_playerID = new Hashtable();

        [Header("Destruction")]
        public Transform m_explosion;

        [Header("Mobile Input")]
        public GameObject m_mobileInput;

        [SyncVar]
        int PlayerId = -1;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Header("Components")]
        public NavMeshAgent agent;
        public Animator animator;
        public TextMesh healthBar;
        public TextMesh projectileBar;
        public Transform turret;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Turret")]
        public KeyCode rotateLeftKey = KeyCode.Q;
        public KeyCode rotateRightKey = KeyCode.E;
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public GameObject projectilePrefab;
        public Transform projectileMount;

        [Header("Stats")]
        [SyncVar] public int health = 4;
        [SyncVar] public int projectile = 4;

        public Camera m_mainCamera;
        public StarterAssetsInputs m_input;

        private const float m_threshold = 0.01f;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput m_playerInput;
#endif

        // cinemachine
        private float m_cinemachineTargetPitch;

        // player
        private float m_speed;
        private float m_rotationVelocity;
        private float m_verticalVelocity;
        private float m_terminalVelocity = 53.0f;

        public void Start()
        {
            if (isLocalPlayer)
            {
                if (m_mainCamera == null)
                {
                    m_mainCamera = Camera.main;
                }

                m_input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                m_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

                if (null != m_mobileInput)
                {
                    if ((Application.platform == RuntimePlatform.Android) ||
                        (Application.platform == RuntimePlatform.IPhonePlayer))
                    {
                        m_mobileInput.SetActive(true);
                    }

                }
            }



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
            // (SyncVar hook would only update on clients, not on server) -- Really ?
            healthBar.text = new string('-', health);

            // always update health bar.
            // (SyncVar hook would only update on clients, not on server) -- Really ?
            if (projectile >= 0)
            {
                projectileBar.text = new string('o', projectile);
            }

            // movement for local player
            if (isLocalPlayer)
            {
                // set target speed based on move speed, sprint speed and if sprint is pressed
                float targetSpeed = m_input.sprint ? SprintSpeed : MoveSpeed;
                agent.speed = targetSpeed / MoveSpeed;

                // rotate
                //float horizontal = Input.GetAxis("Horizontal");
                float horizontal = m_input.move.x;
                transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

                // move
                //float vertical = Input.GetAxis("Vertical");
                float vertical = m_input.move.y;
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                agent.velocity = forward * Mathf.Max(vertical, 0) * agent.speed;
                animator.SetBool("Moving", agent.velocity != Vector3.zero);

                // shoot
                //if (Input.GetKeyDown(shootKey))
                if (m_input.jump)
                {
                    m_input.jump = false;
                    CmdFire();
                }

                RotateTurret();
            }
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            if (projectile > 0)
            {
                projectile--;
                GameObject projectileGO = Instantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
                NetworkServer.Spawn(projectileGO);
                RpcOnFire();

                if (0 == projectile)
                {
                    StopAllCoroutines();
                    StartCoroutine(AutoReload());
                }
            }
            else
            {
                projectile = -1;
                projectileBar.text = "wait";
                StopAllCoroutines();
                StartCoroutine(AutoReload());
            }


        }

        IEnumerator AutoReload()
        {
            yield return new WaitForSeconds(10.0f);
            projectile = 4;
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnDestroy()
        {
            if (m_explosion)
            {
                m_explosion.parent = null;
                m_explosion.gameObject.SetActive(true);
                if (isServer)
                {
                    NetworkServer.Destroy(gameObject);
                }
            }
        }

        [ServerCallback]
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<BTSProjectile>() != null)
            {
                --health;
                if (health == 0)
                {
                    RpcOnDestroy();
                    //NetworkServer.Destroy(gameObject);
                }
            }
        }

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return m_playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        void RotateTurret()
        {

            // if there is an input
            if (m_input.look.sqrMagnitude >= m_threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                m_cinemachineTargetPitch += m_input.look.y * RotationSpeed * deltaTimeMultiplier;
                m_rotationVelocity = m_input.look.x * RotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                m_cinemachineTargetPitch = ClampAngle(m_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                //CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(m_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                turret.transform.Rotate(Vector3.up * m_rotationVelocity);
            }

            //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //    RaycastHit hit;
            //    if (Physics.Raycast(ray, out hit, 100))
            //    {
            //        Debug.DrawLine(ray.origin, hit.point);
            //        Vector3 lookRotation = new Vector3(hit.point.x, turret.transform.position.y, hit.point.z);
            //        turret.transform.LookAt(lookRotation);
            //    }

            //bool l_left = Input.GetKey(rotateLeftKey);
            //bool l_right = Input.GetKey(rotateRightKey);


            // ----------------------- The old way

            //bool l_left = (m_input.look.x < -0.05f);
            //bool l_right = (m_input.look.x > 0.05f);

            //Debug.Log(m_input.look.x);

            //if (l_left)
            //{
            //    turret.transform.Rotate(0, -1 * rotationSpeed * Time.deltaTime, 0);
            //}

            //if (l_right)
            //{
            //    turret.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            //}
        }
    }
}
