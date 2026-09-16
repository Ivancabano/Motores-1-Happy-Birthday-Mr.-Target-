using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private TargetHealth targetHealth;
    private NavMeshAgent agent;
    public AlertManager alertManager;
    public Transform Target;
    [Header("Patrol Configuration")]
    public float hallwayDistance = 8f;
    private Vector3 pointA;
    private Vector3 pointB;
    private bool headingToPointB = true;
    [Header("Scannig and Rotation")]
    public float scanWaitTime = 3f;
    public float scanAngle = 45f;
    public float rotationSpeed = 2f;
    private float scanTimer = 0f;
    private Quaternion rotationOnArrival;
    private bool isWaiting = false;
    [Header("Vision")]
    public float visionDistance = 12f;
    [UnityEngine.Range(0f, 360f)] public float visionAngle = 70f;
    public LayerMask obstacleLayer;
    private float visionTolerance = 1.2f;
    private enum AIState { Patrolling, Chasing }
    private AIState currentState = AIState.Patrolling;

    public bool IsChasing => currentState == AIState.Chasing;

    void Start()
    {
        targetHealth = GetComponent<TargetHealth>();
        agent = GetComponent<NavMeshAgent>();
        pointA = transform.position;
        pointB = transform.position + transform.forward * hallwayDistance;
    }
    void Update()
    {
        switch(currentState)
        {
                case AIState.Patrolling:
                if (HasDirectVision())
                {
                    alertManager.EnemyOnSight();
                    break;
                }
                if (gameObject.CompareTag("EnemyMelee"))
                {
                    if (isWaiting) ExecuteScanRotation();
                    else HandlePatrolMovement();
                }
                break;

                case AIState.Chasing:
                if (Target != null) agent.SetDestination(Target.position);
                break;

        }
    }
    

    public void ForceChase()
    {
        isWaiting = false;
    agent.isStopped = false;
    currentState = AIState.Chasing;

    if (targetHealth != null)
    {
        targetHealth.SetAlertState(true);
    }
    }

    private bool HasDirectVision()
    {
        if (Target == null) return false;
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 TargetCenterPosition = Target.position + Vector3.up * 1f;
        Vector3 dir = (TargetCenterPosition - eyePosition).normalized;
        float dist = Vector3.Distance(eyePosition, TargetCenterPosition);
        if (dist <= visionDistance)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, dir);
            float qualifiedCutOffAngle = (visionAngle / 2f) * visionTolerance;
            if (angleToPlayer < qualifiedCutOffAngle)
            {
                RaycastHit hit;
                if (Physics.Raycast(eyePosition, dir, out hit, visionDistance + 2f))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        return true;
                    }
                }

            }
        }
        return false;
    }
    private void HandlePatrolMovement()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
            isWaiting = true;
            agent.isStopped = true;
            scanTimer = 0f;
            rotationOnArrival = transform.rotation;
            }
    }
    private void ExecuteScanRotation()
    {
        scanTimer += Time.deltaTime;
        if(scanTimer < scanWaitTime)
        {
            float angleModifier = Mathf.Sin(Time.time * rotationSpeed) * scanAngle;
            transform.rotation = rotationOnArrival * Quaternion.Euler(0, angleModifier, 0);
        }
        else
        {
            transform.rotation = rotationOnArrival;
            isWaiting = false;
            agent.isStopped = false;
            UpdatePatrolDestination();
        }

    }
    private void UpdatePatrolDestination()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            Vector3 targetDestination = headingToPointB ? pointB : pointA;
            agent.SetDestination(targetDestination);
            headingToPointB = !headingToPointB;
        }
    }
    private void OnDrawGizmos()
    {
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition, visionDistance);

        Vector3 leftLimit = Quaternion.AngleAxis(-visionAngle / 2, transform.up) * transform.forward;
        Vector3 rightLimit = Quaternion.AngleAxis(visionAngle / 2, transform.up) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(eyePosition, leftLimit * visionDistance);
        Gizmos.DrawRay(eyePosition, rightLimit * visionDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(eyePosition, transform.forward * visionDistance);

        Gizmos.color = Color.green;
        Vector3 leftPoint = eyePosition + leftLimit * visionDistance;
        Vector3 rightPoint = eyePosition + rightLimit * visionDistance;
        Gizmos.DrawLine(leftPoint, rightPoint);
    }


}
