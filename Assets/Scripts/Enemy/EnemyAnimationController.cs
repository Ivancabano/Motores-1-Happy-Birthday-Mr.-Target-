

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
    private bool isDead;

    private static readonly int Speed = Animator.StringToHash("Speed");

    private static readonly int Attack = Animator.StringToHash("Attack");

    private static readonly int Die = Animator.StringToHash("Die");

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

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

        animator.SetFloat(
            Speed,
            speed,
            animationSmooth,
            Time.deltaTime
        );
    }

    private void HandleAttack()
    {
        if (isDead)
            return;

        animator.SetTrigger(Attack);
    }

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;
        animator.SetTrigger(Die);
    }
}

