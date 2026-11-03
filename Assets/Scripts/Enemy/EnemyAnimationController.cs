


using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyContactDamage contactDamage;
    [SerializeField] private TargetHealth targetHealth;

    [Header("Configuracion")]
    [SerializeField] private float animationSmooth = 0.1f;

    private Animator animator;
    private bool isDead = false;

    private static readonly int Speed =
        Animator.StringToHash("Speed");

    private static readonly int Attack =
        Animator.StringToHash("Attack");

    private static readonly int Die =
        Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();

        if (targetHealth == null)
            targetHealth = GetComponentInParent<TargetHealth>();

        if (contactDamage == null)
            contactDamage = GetComponentInParent<EnemyContactDamage>();

        if (contactDamage == null)
            contactDamage = GetComponentInChildren<EnemyContactDamage>();
    }

    private void OnEnable()
    {
        if (contactDamage != null)
            contactDamage.OnAttack += HandleAttack;

        if (targetHealth != null)
            targetHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (contactDamage != null)
            contactDamage.OnAttack -= HandleAttack;

        if (targetHealth != null)
            targetHealth.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (isDead || agent == null || !agent.isOnNavMesh)
            return;

        // Velocidad real del enemigo
        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

        // Actualizamos Idle / Run / Walk
        animator.SetFloat(Speed, speed,animationSmooth,Time.deltaTime);
    }

    private void HandleAttack()
    {
        if (isDead)
            return;

        // Reproducimos la piña
        animator.SetTrigger(Attack);
    }

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;

        animator.ResetTrigger(Attack);
        animator.SetTrigger(Die);
    }
}


