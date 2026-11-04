using UnityEngine;
using UnityEngine.AI;

public class EnemyBossBase : MonoBehaviour
{
    protected EnemyBase enemyBaseReference;
    protected NavMeshAgent agent;
    protected TargetHealth targetHealth;
    protected Transform targetTransform;
    protected bool isActivated = false;
    [SerializeField] protected int bossHealthNotActivated = 10;
    [SerializeField] protected int bossHealthActivated = 15;
    [SerializeField] protected int bossHealth;

    void Start()
    {
        // 1. Primero buscamos las referencias ANTES de inicializar la vida
        SearchReferences();

        // 2. Luego inicializamos la vida con la referencia ya encontrada
        InicializarVida();

        Debug.Log("Referencias listas");

        if (enemyBaseReference != null)
        {
            Debug.Log("me notifique al aviso");
            enemyBaseReference.OnChasingChanged += HandleChasingChanged;
            Debug.Log($"[VERIFICACIÓN] El Boss ({gameObject.name}) se suscribió al EnemyBase que está en el objeto: {enemyBaseReference.gameObject.name}");
        }
        else
        {
            Debug.LogError("[VERIFICACIÓN] ¡El Boss NO encontró ningún EnemyBase al cual suscribirse!");
        }
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
        }

        // CORREGIDO: Buscamos en la escena al guardia/enemigo base, no en este mismo objeto
        if (enemyBaseReference == null)
        {
            enemyBaseReference = Object.FindFirstObjectByType<EnemyBase>();
        }
    }

    void OnDisable()
    {
        if (enemyBaseReference != null)
        {
            enemyBaseReference.OnChasingChanged -= HandleChasingChanged;
        }
    }

    private void InicializarVida()
    {
        if (enemyBaseReference != null)
        {
            isActivated = enemyBaseReference.IsChasing;
            bossHealth = isActivated ? bossHealthActivated : bossHealthNotActivated;
        }
    }

    private void HandleChasingChanged(bool estaPersiguiendo)
    {
        Debug.Log("Notificado");
        if (!isActivated && estaPersiguiendo)
        {
            isActivated = true;
            bossHealth = bossHealthActivated;
            Debug.Log($"¡El Boss se ha activado! Vida escalada a: {bossHealth}");
        }
    }

    void Update()
    {

    }
}