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
        public static HashSet<BTSTank> ActivePlayers = new HashSet<BTSTank>(); // All tanks
        public static HashSet<BTSTank> PlayerTanks = new HashSet<BTSTank>();  // Only player-controlled tanks
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
        public int m_maxHealth = 4;
        public int m_maxProjectile = 4;
        [SyncVar(hook = nameof(HealthChanged))]
        public int health = -1;
        [SyncVar(hook = nameof(ProjectileChanged))]
        public int projectile = -1;

        public Camera m_mainCamera;
        public StarterAssetsInputs m_input;

        private const float m_threshold = 0.01f;
        private Color m_saveProjectileBarColor;

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

        public virtual void HealthChanged(int a_old, int a_new)
        {
            healthBar.text = new string('-', a_new);
        }

        void ProjectileChanged(int a_old, int a_new)
        {
            if (a_new >= 0)
            {
                projectileBar.text = new string('-', a_new);
            }
        }

        public virtual void Start()
        {
            m_saveProjectileBarColor = projectileBar.color;

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
                Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

                if (null != m_mobileInput)
                {
                    if ((Application.platform == RuntimePlatform.Android) ||
                        (Application.platform == RuntimePlatform.IPhonePlayer))
                    {
                        m_mobileInput.SetActive(true);
                    }
                }

                PlayerTanks.Add(this); // Add to player-specific set
                Debug.Log($"Player tank initialized. Name: {gameObject.name}, isLocalPlayer: {isLocalPlayer}");
            }

            ActivePlayers.Add(this);
            m_playerID.Add(this, m_playerID.Count);
            gameObject.name = "BTS Tank (" + m_playerID[this] + ")";

            if (isServer)
            {
                health = m_maxHealth;
                projectile = m_maxProjectile;
            }
        }

        public void OnDestroy()
        {
            ActivePlayers.Remove(this);
            PlayerTanks.Remove(this);
            m_playerID.Remove(this);
        }

        public virtual void Update()
        {
            if (isLocalPlayer)
            {
                // Debug player state
//                Debug.Log($"Player Update - Move: {m_input.move}, Jump: {m_input.jump}, Camera: {(m_mainCamera != null ? m_mainCamera.name : "null")}");

                // Set target speed based on move speed, sprint speed and if sprint is pressed
                float targetSpeed = m_input.sprint ? SprintSpeed : MoveSpeed;
                agent.speed = targetSpeed / MoveSpeed;

                // Rotate
                float horizontal = m_input.move.x;
                transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

                // Move
                float vertical = m_input.move.y;
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                agent.velocity = forward * Mathf.Max(vertical, 0) * agent.speed;
                animator.SetBool("Moving", agent.velocity != Vector3.zero);

                // Shoot
                if (m_input.jump)
                {
                    m_input.jump = false;
                    CmdFire();
                }

                RotateTurret();
            }
        }

        // Server-side firing method for AI
        [Server]
        protected void ServerFire()
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
                projectileBar.text = "----";
                projectileBar.color = Color.gray;
                StopAllCoroutines();
                StartCoroutine(AutoReload());
            }
        }

        [Command]
        void CmdFire()
        {
            ServerFire(); // Delegate to server-side method
        }

        IEnumerator AutoReload()
        {
            yield return new WaitForSeconds(10.0f);
            projectile = m_maxProjectile;
            projectileBar.color = m_saveProjectileBarColor;
        }

        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }

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
            if (m_input.look.sqrMagnitude >= m_threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                m_cinemachineTargetPitch += m_input.look.y * RotationSpeed * deltaTimeMultiplier;
                m_rotationVelocity = m_input.look.x * RotationSpeed * deltaTimeMultiplier;

                m_cinemachineTargetPitch = ClampAngle(m_cinemachineTargetPitch, BottomClamp, TopClamp);
                turret.transform.Rotate(Vector3.up * m_rotationVelocity);
            }
        }
    }
}