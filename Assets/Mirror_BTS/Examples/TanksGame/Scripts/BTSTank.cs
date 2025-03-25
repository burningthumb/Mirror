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
        public static HashSet<BTSTank> PlayerTanks = new HashSet<BTSTank>();
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
        [Tooltip("How far in degrees can you move the turret left/right from center")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the turret left/right from center")]
        public float BottomClamp = -90.0f;
        [Tooltip("Speed of turret rotation when using D-pad back")]
        public float turretRotationSpeed = 50f;
        [Tooltip("Minimum input value for left stick/D-pad down to trigger turret rotation (0 to 1)")]
        public float backInputThreshold = 0.75f;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public GameObject projectilePrefab;
        public Transform projectileMount;
        public float m_autoReloadTime = 10.0f;

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

        // Turret ping-pong rotation
        private bool isRotatingTurret = false;
        private float turretRotationDirection = 1f;
        private bool wasBackPressedLastFrame = false;

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

                PlayerTanks.Add(this);
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

            // Configure NavMeshAgent
            agent.updateRotation = false; // Manual rotation control
            agent.updatePosition = true;  // Allow velocity to move the agent
            agent.acceleration = 1000f;   // High acceleration to match velocity instantly
            agent.angularSpeed = 0;       // Disable agent-driven rotation
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
                // Set target speed based on sprint input
                float targetSpeed = m_input.sprint ? SprintSpeed : MoveSpeed;
                agent.speed = targetSpeed;

                // Rotate tank left/right
                float horizontal = m_input.move.x;
                transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

                // Move forward only
                float vertical = m_input.move.y;
                Vector3 forward = transform.forward;
                Vector3 forwardVelocity = forward * Mathf.Max(vertical, 0) * agent.speed;
                agent.velocity = new Vector3(forwardVelocity.x, agent.velocity.y, forwardVelocity.z); // Preserve y for gravity/slopes
                animator.SetBool("Moving", agent.velocity.sqrMagnitude > 0.01f);

                // Shoot
                if (m_input.jump)
                {
                    m_input.jump = false;
                    CmdFire();
                }

                // Turret rotation
                RotateTurret();
                HandleDPadTurretRotation();
            }
        }

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
            ServerFire();
        }

        IEnumerator AutoReload()
        {
            yield return null;

            if (m_autoReloadTime > 0)
            {
                yield return new WaitForSeconds(m_autoReloadTime);
                projectile = m_maxProjectile;
                projectileBar.color = m_saveProjectileBarColor;
            }
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
                float currentAngle = turret.localEulerAngles.y;
                if (currentAngle > 180f) currentAngle -= 360f;
                float newAngle = currentAngle + m_rotationVelocity;
                newAngle = ClampAngle(newAngle, BottomClamp, TopClamp);
                turret.localEulerAngles = new Vector3(0, newAngle, 0);
            }
        }

        void HandleDPadTurretRotation()
        {
            float dPadVertical = m_input.move.y;

            bool isBackPressed = dPadVertical < -backInputThreshold;
            if (isBackPressed && !wasBackPressedLastFrame)
            {
                turretRotationDirection = -turretRotationDirection;
                isRotatingTurret = true;
            }
            else if (!isBackPressed)
            {
                isRotatingTurret = false;
            }

            wasBackPressedLastFrame = isBackPressed;

            if (isRotatingTurret)
            {
                float currentAngle = turret.localEulerAngles.y;
                if (currentAngle > 180f) currentAngle -= 360f;

                float rotationStep = turretRotationSpeed * turretRotationDirection * Time.deltaTime;
                float newAngle = currentAngle + rotationStep;

                if (newAngle >= TopClamp || newAngle <= BottomClamp)
                {
                    turretRotationDirection = -turretRotationDirection;
                    newAngle = Mathf.Clamp(newAngle, BottomClamp, TopClamp);
                }

                turret.localEulerAngles = new Vector3(0, newAngle, 0);
            }
        }
    }
}