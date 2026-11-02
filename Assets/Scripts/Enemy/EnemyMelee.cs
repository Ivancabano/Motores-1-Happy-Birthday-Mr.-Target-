using UnityEngine;

public class EnemyMelee : EnemyBase
{
    [Header("Patrol & Scan Configuration")]
    [SerializeField] private float hallwayDistance = 8f;
    [SerializeField] private float scanWaitTime = 3f;
    [SerializeField] private float scanAngle = 45f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float attackDistance = 1.8f;
    [Header("Combate Melee")]
    private Vector3 pointA;
    private Vector3 pointB;
    private bool headingToPointB = true;
    private float scanTimer = 0f;
    private Quaternion rotationOnArrival;
    private bool isWaiting = false;

    protected override void Start()
    {
        base.Start();
        pointA = transform.position;
        pointB = transform.position + transform.forward * hallwayDistance;
    }

    protected override void HandlePatrol()
    {
        if (isWaiting) ExecuteScanRotation();
        else HandlePatrolMovement();
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
        if (scanTimer < scanWaitTime)
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
protected override void ExecuteChase()
{
    if (targetTransform == null || agent == null || !agent.isOnNavMesh)
        return;

    // Calculamos la distancia horizontal al jugador.
    Vector3 direction =
        targetTransform.position - transform.position;

    direction.y = 0f;

    float distance = direction.magnitude;

    // Si esta suficientemente cerca, se detiene.
    if (distance <= attackDistance)
    {
        agent.isStopped = true;
        agent.ResetPath();

        
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp( transform.rotation,targetRotation,8f * Time.deltaTime);
        }

        return;
    }

    agent.isStopped = false;
    agent.SetDestination(targetTransform.position);
}
    public override void ForceChase()
    {
        isWaiting = false;
        base.ForceChase();
    }
}