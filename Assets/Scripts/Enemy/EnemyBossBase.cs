using UnityEngine;
using UnityEngine.AI;

public class EnemyBossBase : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Transform targetTransform;

    [SerializeField] protected bool isActivated = true;
    public bool IsCombatActive => isActivated;

    [Header("Boss Health Configuration")]
    [SerializeField] protected int bossHealthNotActivated = 10;
    [SerializeField] protected int bossHealthActivated = 15;
    [SerializeField] protected int bossHealth;

    [Header("Movement Settings")]
    [SerializeField] protected float movementSpeed = 3.5f;
    [SerializeField] protected float stoppingDistance = 2.0f; // Distancia para no pegarse demasiado al jugador
    [SerializeField] private float rotationSpeed = 15f;       // Añadido para que pivote rápido a tu espalda

    void Start()
    {
        SearchReferences();

        bossHealth = bossHealthActivated;

        if (agent != null)
        {
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = !isActivated;
        }

        Debug.Log("[Boss] Listo y configurado para pruebas de movimiento.");
    }

    protected void SearchReferences()
    {
        agent = GetComponent<NavMeshAgent>();

        if (targetTransform == null)
        {
            PlayerDetectorTarget playerTarget = Object.FindFirstObjectByType<PlayerDetectorTarget>();
            if (playerTarget != null)
            {
                targetTransform = playerTarget.transform;
            }
            else
            {
                Debug.LogError("[Boss] ¡No se encontró el PlayerDetectorTarget en la escena!");
            }
        }
    }

    void Update()
    {
        if (isActivated)
        {
            MoverHaciaJugador();
        }
    }

    protected virtual void MoverHaciaJugador()
    {
        if (agent == null || targetTransform == null) return;

        float distance = Vector3.Distance(transform.position, targetTransform.position);

        // Si está en rango de disparo, frenamos de golpe la velocidad física
        if (distance <= stoppingDistance)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            // FORZAMOS LA VELOCIDAD A CERO para matar la inercia del NavMeshAgent al instante
            agent.velocity = Vector3.zero;

            // Desactivamos la rotación automática para poder girar hacia ti libremente
            agent.updateRotation = false;

            Vector3 direction = (targetTransform.position - transform.position);
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Fuera de rango: reactivamos el movimiento normal
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }

            agent.updateRotation = true;
            agent.SetDestination(targetTransform.position);
        }
    }
}