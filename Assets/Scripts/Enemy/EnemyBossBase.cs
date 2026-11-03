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
        SearchReferences();
        InicializarVida();
    }
    protected void SearchReferences()
    {
        agent = GetComponent<NavMeshAgent>();

        if (targetTransform == null)
        {
            agent = GetComponent<NavMeshAgent>();

            PlayerDetectorTarget playerTarget = Object.FindFirstObjectByType<PlayerDetectorTarget>();
            
            if (playerTarget != null)
            {
                targetTransform = playerTarget.transform;
            }
        }
        if (enemyBaseReference == null)
        {
            enemyBaseReference = GetComponent<EnemyBase>();
        }
    }
    void OnEnable()
    {
        if (enemyBaseReference != null)
        {
            enemyBaseReference.OnChasingChanged += HandleChasingChanged;
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
