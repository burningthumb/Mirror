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

        private BTSTank targetEnemy;
        private int currentPatrolIndex = 0;
        private float lastDecisionTime;
        private Vector3 randomDestination;

        // Event for when tank is destroyed
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

                if (!isLocalPlayer)
                {
                    if (m_mobileInput != null) m_mobileInput.SetActive(false);
                }
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

        // Hook into the existing HealthChanged method to detect destruction
        public override void HealthChanged(int oldHealth, int newHealth)
        {
            base.HealthChanged(oldHealth, newHealth); // Call base to update health bar
            
            if (isServer && newHealth <= 0)
            {
                OnTankDestroyed?.Invoke(this);
                // Destruction is already handled in BTSTank's OnTriggerEnter
            }
        }

        void MakeAIDecision()
        {
            targetEnemy = FindNearestEnemy();

            if (targetEnemy != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);
                
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
                    ServerFire();
                }
            }
            else
            {
                Patrol();
            }
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