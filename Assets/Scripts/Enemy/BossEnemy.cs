

using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("Configuracion del jefe")]
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float rotationSpeed = 5f;

    private bool bossActivated = false;
    private bool isDead = false;

    public bool IsActivated => bossActivated && !isDead;

    protected override void OnEnable()
    {
        // No participa de las alertas generales.
    }

    protected override void OnDisable()
    {
    }

    protected override void Start()
    {
        base.Start();

        targetHealth = GetComponent<TargetHealth>();

        if (targetHealth != null)
            targetHealth.OnDeath += HandleDeath;

        // Espera hasta que el jugador entre a la sala.
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnDestroy()
    {
        if (targetHealth != null)
            targetHealth.OnDeath -= HandleDeath;
    }

    protected override void Update()
    {
        if (!bossActivated || isDead)
            return;

        base.Update();
    }

    protected override void HandlePatrol()
    {
        // El jefe nunca patrulla.
    }

    protected override void ExecuteChase()
    {
        if (targetTransform == null || agent == null ||!agent.isOnNavMesh)
            return;

        Vector3 direction =targetTransform.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= shootingRange)
        {
            // Se detiene para disparar.
            agent.isStopped = true;

            if (agent.hasPath)
                agent.ResetPath();

            // Mira hacia el protagonista.
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation,rotationSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Persigue al jugador.
            agent.isStopped = false;
            agent.SetDestination(targetTransform.position);
        }
    }

    public void ActivateBoss()
    {
        if (bossActivated || isDead)
            return;

        bossActivated = true;

        // Vida segun deteccion durante la partida.
        bool wasDetected = RankManager.Instance != null && RankManager.Instance.WasDetected;

        if (targetHealth != null)
        {
            targetHealth.SetAlertState(wasDetected);

            Debug.Log("Vida del jefe: " + targetHealth.CurrentHealth + "/" + targetHealth.MaxHealth);
        }

        ForceChase();

        Debug.Log("JEFE ACTIVADO - Jugador detectado: " + wasDetected);
    }

    private void HandleDeath()
    {
        isDead = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}
