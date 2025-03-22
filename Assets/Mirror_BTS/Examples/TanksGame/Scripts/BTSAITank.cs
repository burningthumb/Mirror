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

        private BTSTank targetEnemy;
        private int currentPatrolIndex = 0;
        private float lastDecisionTime;
        private Vector3 randomDestination;

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
            }
        }

        public override void Update()
        {
            base.Update();

            if (!isServer) return; // Only run AI logic on server

            // Clear player input since this is an AI tank
            m_input.move = Vector2.zero;
            m_input.look = Vector2.zero;
            m_input.jump = false;
            m_input.sprint = false;

            // Make decisions at regular intervals
            if (Time.time - lastDecisionTime >= decisionInterval)
            {
                MakeAIDecision();
                lastDecisionTime = Time.time;
            }

            // Handle turret aiming if we have a target
            if (targetEnemy != null)
            {
                AimAtTarget();
            }
        }

        void MakeAIDecision()
        {
            targetEnemy = FindNearestEnemy();

            if (targetEnemy != null)
            {
                agent.SetDestination(transform.position);
                if (Vector3.Distance(transform.position, targetEnemy.transform.position) <= firingRange)
                {
                    CmdFire();
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
                if (potentialTarget == this) continue;

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

            animator.SetBool("Moving", agent.velocity != Vector3.zero);
        }

        Vector3 GetRandomPosition()
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 20f;
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