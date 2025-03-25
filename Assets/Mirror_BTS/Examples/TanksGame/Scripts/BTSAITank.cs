using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

namespace com.burningthumb.examples
{
    public class BTSAITank : BTSTank
    {
        [Header("AI Settings")]
        [Tooltip("Distance at which AI detects enemies")]
        public float detectionRange = 20f;
        [Tooltip("Distance at which AI will start firing")]
        public float firingRange = 15f;
        [Tooltip("Time between AI decisions in seconds")]
        public float decisionInterval = 1f;
        [Tooltip("Minimum distance to maintain from target")]
        public float minEngageDistance = 5f;
        [Tooltip("Delay in seconds before attacking a newly detected target")]
        public float attackDelay = 3f;
        [Tooltip("Cooldown time in seconds between projectile firings")]
        public float firingCooldown = 2f;

        [Header("Patrol Settings")]
        [Tooltip("Minimum distance for random patrol points")]
        public float minPatrolDistance = 10f;
        [Tooltip("Maximum distance for random patrol points")]
        public float maxPatrolDistance = 20f;
        [Tooltip("Time window to check for stuck detection (seconds)")]
        public float stuckCheckInterval = 2f;
        [Tooltip("Number of patrol points to evaluate when out of ammo")]
        public int patrolPointCandidates = 5;

        [Header("Line of Sight")]
        [Tooltip("Layer mask for line of sight checks (exclude tank itself)")]
        public LayerMask losLayerMask;

        private BTSTank targetEnemy;
        private float lastDecisionTime;
        private float targetDetectionTime;
        private bool isDelayingAttack;
        private float lastFireTime;
        private Vector3 patrolDestination;
        private Vector3[] patrolPathCorners;
        private int currentCornerIndex;
        private Vector3 lastPosition; // For stuck detection
        private float lastPositionTime; // Time of last position update
        private bool wasOutOfAmmo; // Track ammo state to trigger new patrol selection

        public event Action<BTSAITank> OnTankDestroyed;

        public override void Start()
        {
            base.Start();
            
            if (isServer)
            {
                patrolDestination = GetRandomPatrolPosition();
                UpdatePatrolPath();
                lastDecisionTime = Time.time;
                targetDetectionTime = 0f;
                isDelayingAttack = false;
                lastFireTime = -firingCooldown;
                lastPosition = transform.position;
                lastPositionTime = Time.time;
                wasOutOfAmmo = projectile <= 0;

                if (!isLocalPlayer)
                {
                    if (m_mobileInput != null) m_mobileInput.SetActive(false);
                }

                gameObject.name = "BTS AI Tank (" + m_playerID[this] + ")";
            }
        }

        public override void Update()
        {
            if (!isServer) return;

            if (Time.time - lastDecisionTime >= decisionInterval)
            {
                MakeAIDecision();
                lastDecisionTime = Time.time;
            }

            // Handle turret and movement
            if (targetEnemy != null && projectile > 0)
            {
                AimAtTarget();
            }
            else
            {
                turret.localEulerAngles = Vector3.zero; // Face forward when patrolling or out of ammo
                Patrol();
            }

            animator.SetBool("Moving", agent.velocity != Vector3.zero);
        }

        public override void HealthChanged(int oldHealth, int newHealth)
        {
            base.HealthChanged(oldHealth, newHealth);
            
            if (isServer && newHealth <= 0)
            {
                OnTankDestroyed?.Invoke(this);
            }
        }

        void MakeAIDecision()
        {
            BTSTank newTarget = FindNearestEnemy();

            if (newTarget != targetEnemy)
            {
                targetEnemy = newTarget;
                if (targetEnemy != null)
                {
                    targetDetectionTime = Time.time;
                    isDelayingAttack = true;
                }
                else
                {
                    isDelayingAttack = false;
                }
            }

            // Check if we just ran out of ammo
            bool isOutOfAmmo = projectile <= 0;
            if (isOutOfAmmo && !wasOutOfAmmo)
            {
                patrolDestination = ChooseSafestPatrolPosition();
                UpdatePatrolPath();
            }
            wasOutOfAmmo = isOutOfAmmo;

            if (targetEnemy != null && projectile > 0)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);

                if (distanceToTarget > minEngageDistance)
                {
                    MoveToward(targetEnemy.transform.position);
                }
                else
                {
                    StopMoving();
                }

                if (distanceToTarget <= firingRange)
                {
                    if (isDelayingAttack)
                    {
                        if (Time.time - targetDetectionTime >= attackDelay)
                        {
                            isDelayingAttack = false;
                            if (HasClearLineOfSight(targetEnemy) && CanFire())
                            {
                                ServerFire();
                                lastFireTime = Time.time;
                            }
                        }
                    }
                    else if (HasClearLineOfSight(targetEnemy) && CanFire())
                    {
                        ServerFire();
                        lastFireTime = Time.time;
                    }
                }
            }
        }

        void MoveToward(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation.x = 0;
            targetRotation.z = 0;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            Vector3 forward = transform.forward;
            agent.velocity = forward * MoveSpeed;
        }

        void StopMoving()
        {
            agent.velocity = Vector3.zero;
        }

        void Patrol()
        {
            // Check if stuck every 2 seconds
            if (Time.time - lastPositionTime >= stuckCheckInterval)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastPosition);
                float tankSize = agent.radius * 2f; // Approximate tank size as diameter
                if (distanceMoved < tankSize)
                {
                    // Stuck: Pick a new destination (or safest if out of ammo)
                    patrolDestination = projectile > 0 ? GetRandomPatrolPosition() : ChooseSafestPatrolPosition();
                    UpdatePatrolPath();
                }
                lastPosition = transform.position;
                lastPositionTime = Time.time;
            }

            // If no path or we've reached the current corner, update the path
            if (patrolPathCorners == null || currentCornerIndex >= patrolPathCorners.Length || Vector3.Distance(transform.position, patrolPathCorners[currentCornerIndex]) < 1f)
            {
                if (Vector3.Distance(transform.position, patrolDestination) < 2f || patrolPathCorners == null)
                {
                    patrolDestination = projectile > 0 ? GetRandomPatrolPosition() : ChooseSafestPatrolPosition();
                }
                UpdatePatrolPath();
            }

            // Move toward the next corner
            if (patrolPathCorners != null && currentCornerIndex < patrolPathCorners.Length)
            {
                Vector3 nextPosition = patrolPathCorners[currentCornerIndex];
                MoveToward(nextPosition);
            }
        }

        void UpdatePatrolPath()
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, patrolDestination, NavMesh.AllAreas, path))
            {
                patrolPathCorners = path.corners;
                currentCornerIndex = 1; // Start at the first corner after current position
            }
            else
            {
                // If path fails, pick a new destination
                patrolDestination = projectile > 0 ? GetRandomPatrolPosition() : ChooseSafestPatrolPosition();
                patrolPathCorners = null;
                currentCornerIndex = 0;
            }
        }

        Vector3 GetRandomPatrolPosition()
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(minPatrolDistance, maxPatrolDistance);
            Vector3 randomPoint = transform.position + randomDirection;
            NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, maxPatrolDistance, NavMesh.AllAreas);
            return hit.position;
        }

        Vector3 ChooseSafestPatrolPosition()
        {
            Vector3[] candidates = new Vector3[patrolPointCandidates];
            float[] minDistancesToPlayers = new float[patrolPointCandidates];

            // Generate 5 random patrol points
            for (int i = 0; i < patrolPointCandidates; i++)
            {
                candidates[i] = GetRandomPatrolPosition();
                minDistancesToPlayers[i] = float.MaxValue;

                // Find the minimum distance to any human player
                foreach (BTSTank player in ActivePlayers)
                {
                    if (player.isLocalPlayer)
                    {
                        float distance = Vector3.Distance(candidates[i], player.transform.position);
                        minDistancesToPlayers[i] = Mathf.Min(minDistancesToPlayers[i], distance);
                    }
                }
            }

            // Choose the point with the maximum minimum distance to any player
            int safestIndex = 0;
            float maxMinDistance = minDistancesToPlayers[0];
            for (int i = 1; i < patrolPointCandidates; i++)
            {
                if (minDistancesToPlayers[i] > maxMinDistance)
                {
                    maxMinDistance = minDistancesToPlayers[i];
                    safestIndex = i;
                }
            }

            return candidates[safestIndex];
        }

        bool CanFire()
        {
            return Time.time - lastFireTime >= firingCooldown;
        }

        bool HasClearLineOfSight(BTSTank target)
        {
            Vector3 start = projectileMount.position;
            Vector3 direction = (target.transform.position - start).normalized;
            float distance = Vector3.Distance(start, target.transform.position);

            RaycastHit hit;
            if (Physics.Raycast(start, direction, out hit, distance, losLayerMask))
            {
                if (hit.transform != target.transform)
                {
                    return false;
                }
            }
            return true;
        }

        BTSTank FindNearestEnemy()
        {
            BTSTank nearest = null;
            float minDistance = float.MaxValue;

            foreach (BTSTank potentialTarget in ActivePlayers)
            {
                if (potentialTarget == this || !potentialTarget.isLocalPlayer) continue;

                float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                if (distance <= detectionRange && distance < minDistance)
                {
                    minDistance = distance;
                    nearest = potentialTarget;
                }
            }

            return nearest;
        }

        void AimAtTarget()
        {
            Vector3 targetDirection = (targetEnemy.transform.position - turret.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            targetRotation.x = 0;
            targetRotation.z = 0;

            Vector3 targetDirectionRelative = transform.InverseTransformDirection(targetDirection);
            float targetYaw = Mathf.Atan2(targetDirectionRelative.x, targetDirectionRelative.z) * Mathf.Rad2Deg;

            float currentTurretYaw = turret.localEulerAngles.y;
            if (currentTurretYaw > 180f) currentTurretYaw -= 360f;

            float newTurretYaw = Mathf.MoveTowardsAngle(currentTurretYaw, targetYaw, rotationSpeed * Time.deltaTime);
            float clampedTurretYaw = Mathf.Clamp(newTurretYaw, BottomClamp, TopClamp);
            turret.localEulerAngles = new Vector3(0, clampedTurretYaw, 0);

            float yawError = Mathf.DeltaAngle(clampedTurretYaw, targetYaw);
            if (Mathf.Abs(yawError) > 1f)
            {
                Quaternion tankTargetRotation = Quaternion.LookRotation(targetDirection);
                tankTargetRotation.x = 0;
                tankTargetRotation.z = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, tankTargetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}