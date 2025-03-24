using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
        [Tooltip("Patrol waypoints (if empty, tank will move randomly)")]
        public Transform[] patrolPoints;
        [Tooltip("Minimum distance to maintain from target")]
        public float minEngageDistance = 5f;
        [Tooltip("Delay in seconds before attacking a newly detected target")]
        public float attackDelay = 3f;
        [Tooltip("Distance to flee when out of projectiles")]
        public float fleeDistance = 10f; // New setting for how far to run away

        [Header("Line of Sight")]
        [Tooltip("Layer mask for line of sight checks (exclude tank itself)")]
        public LayerMask losLayerMask;

        private BTSTank targetEnemy;
        private int currentPatrolIndex = 0;
        private float lastDecisionTime;
        private Vector3 randomDestination;
        private float targetDetectionTime;
        private bool isDelayingAttack;

        public event Action<BTSAITank> OnTankDestroyed;

        public override void Start()
        {
            base.Start();
            
            if (isServer)
            {
                if (patrolPoints.Length == 0)
                {
                    randomDestination = GetRandomPosition();
                    agent.SetDestination(randomDestination);
                }
                lastDecisionTime = Time.time;
                targetDetectionTime = 0f;
                isDelayingAttack = false;

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

            if (targetEnemy != null)
            {
                AimAtTarget();
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

            if (targetEnemy != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);

                if (projectile <= 0) // Out of projectiles, flee
                {
                    Vector3 fleeDirection = (transform.position - targetEnemy.transform.position).normalized;
                    Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;
                    if (UnityEngine.AI.NavMesh.SamplePosition(fleePosition, out UnityEngine.AI.NavMeshHit hit, fleeDistance, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                    else
                    {
                        Patrol(); // Fallback to patrol if no valid flee position
                    }
                }
                else // Normal behavior when projectiles are available
                {
                    if (distanceToTarget > minEngageDistance)
                    {
                        Vector3 pursuePosition = targetEnemy.transform.position;
                        agent.SetDestination(pursuePosition);
                    }
                    else
                    {
                        agent.SetDestination(transform.position);
                    }

                    if (distanceToTarget <= firingRange)
                    {
                        if (isDelayingAttack)
                        {
                            if (Time.time - targetDetectionTime >= attackDelay)
                            {
                                isDelayingAttack = false;
                                if (HasClearLineOfSight(targetEnemy))
                                {
                                    ServerFire();
                                }
                            }
                        }
                        else if (HasClearLineOfSight(targetEnemy))
                        {
                            ServerFire();
                        }
                    }
                }
            }
            else
            {
                Patrol();
            }
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

        void Patrol()
        {
            if (patrolPoints.Length > 0)
            {
                if (agent.remainingDistance < 1f)
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
            }
            else
            {
                if (agent.remainingDistance < 1f)
                {
                    randomDestination = GetRandomPosition();
                    agent.SetDestination(randomDestination);
                }
            }
        }

        Vector3 GetRandomPosition()
        {
            Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * 20f;
            UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 20f, UnityEngine.AI.NavMesh.AllAreas);
            return hit.position;
        }

        void AimAtTarget()
        {
            Vector3 targetDirection = (targetEnemy.transform.position - turret.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            
            targetRotation.x = 0;
            targetRotation.z = 0;
            
            turret.rotation = Quaternion.RotateTowards(
                turret.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}