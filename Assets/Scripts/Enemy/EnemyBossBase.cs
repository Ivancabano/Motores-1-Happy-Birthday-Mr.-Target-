using UnityEngine;
using UnityEngine.AI;

public class EnemyBossBase : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Transform targetTransform;

    // Lo dejamos activado por defecto para que puedas darle a Play y probarlo de inmediato
    [SerializeField] protected bool isActivated = true;
    public bool IsCombatActive => isActivated;

    [Header("Boss Health Configuration")]
    [SerializeField] protected int bossHealthNotActivated = 10;
    [SerializeField] protected int bossHealthActivated = 15;
    [SerializeField] protected int bossHealth;

    [Header("Movement Settings")]
    [SerializeField] protected float movementSpeed = 3.5f;
    [SerializeField] protected float stoppingDistance = 2.0f; // Distancia para no pegarse demasiado al jugador

    void Start()
    {
        SearchReferences();

        // Asignamos una vida por defecto para la prueba rápida
        bossHealth = bossHealthActivated;

        if (agent != null)
        {
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = !isActivated; // Si isCombatStarted es true, arranca a moverse de una
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
        // Si el combate está activo, ejecuta la persecución
        if (isActivated)
        {
            MoverHaciaJugador();
        }
    }

    protected virtual void MoverHaciaJugador()
    {
        if (agent == null || targetTransform == null) return;

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        // El NavMeshAgent calcula automáticamente la ruta esquivando los 6 pilares
        agent.SetDestination(targetTransform.position);
    }
}