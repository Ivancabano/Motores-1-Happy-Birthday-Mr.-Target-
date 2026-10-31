using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : MonoBehaviour, IAlertable
{
    protected NavMeshAgent agent;
    protected TargetHealth targetHealth;
    protected AlertManager alertManager;protected Transform targetTransform;
    protected IDetectable targetDetectable;

    [Header("Vision Configuration")]
    [SerializeField] protected float visionDistance = 12f;
    [Range(0f, 360f)] protected float visionAngle = 70f;
    [SerializeField] protected LayerMask obstacleLayer;
    protected float visionTolerance = 1.2f;

    protected enum AIState { Patrolling, Chasing }
    protected AIState currentState = AIState.Patrolling;

    public bool IsChasing
    {
        get => currentState == AIState.Chasing;
        internal set => currentState = value ? AIState.Chasing : AIState.Patrolling;
    }

    public float VisionDistance => visionDistance;
    public float VisionAngle => visionAngle;
    public Vector3 EyePosition => transform.position + Vector3.up * 1.5f;

    protected virtual void Start()
    {
       SearchReferences();
    }
    protected void SearchReferences()
    {
        if (targetTransform == null)
        {
            agent = GetComponent<NavMeshAgent>();

            alertManager = Object.FindFirstObjectByType<AlertManager>();

            PlayerDetectorTarget playerTarget = Object.FindFirstObjectByType<PlayerDetectorTarget>();
            if (playerTarget != null)
            {
                targetTransform = playerTarget.transform;
            }
        }

        if (targetTransform != null)
        {
            targetDetectable = targetTransform.GetComponent<IDetectable>();
        }
    }
    
    protected virtual void OnEnable()
    {
        AlertManager.Register(this);
    }

    protected virtual void OnDisable()
    {
        AlertManager.Unregister(this);
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                if (HasDirectVision())
                {
                    if (alertManager != null) alertManager.EnemyOnSight();
                    break;
                }
                HandlePatrol();
                break;

            case AIState.Chasing:
                ExecuteChase();
                break;
        }
    }

    protected abstract void HandlePatrol();

    protected virtual void ExecuteChase()
    {
        if (targetTransform != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(targetTransform.position);
        }
    }

    public virtual void ForceChase()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        currentState = AIState.Chasing;
    }

    protected bool HasDirectVision()
    {
        if (targetTransform == null) return false;

        Vector3 eyePos = EyePosition;
        Vector3 targetCenter = targetDetectable != null ?
            targetDetectable.GetCenterPosition() :
            targetTransform.position + Vector3.up * 1f;

        Vector3 dir = (targetCenter - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, targetCenter);

        if (dist <= visionDistance)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, dir);
            if (angleToPlayer < (visionAngle / 2f) * visionTolerance)
            {
                if (Physics.Raycast(eyePos, dir, out RaycastHit hit, visionDistance + 2f, ~obstacleLayer))
                {
                    if (hit.transform.TryGetComponent<IDetectable>(out _))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}