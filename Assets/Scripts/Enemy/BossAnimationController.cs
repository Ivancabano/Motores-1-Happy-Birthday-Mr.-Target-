
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossEnemy boss;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BossShotgunAttack shotgun;
    [SerializeField] private TargetHealth targetHealth;

    [Header("Animaciones")]
    [SerializeField] private float animationSmooth = 0.1f;

    private Animator animator;
    private bool isDead = false;

    private static readonly int Speed = Animator.StringToHash("Speed");

    private static readonly int Shoot = Animator.StringToHash("Shoot");

    private static readonly int Die = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (boss == null)
            boss = GetComponentInParent<BossEnemy>();

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();

        if (shotgun == null)
            shotgun = GetComponentInParent<BossShotgunAttack>();

        if (targetHealth == null)
            targetHealth = GetComponentInParent<TargetHealth>();
    }

    private void OnEnable()
    {
        if (shotgun != null)
            shotgun.OnShoot += HandleShoot;

        if (targetHealth != null)
            targetHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (shotgun != null)
            shotgun.OnShoot -= HandleShoot;

        if (targetHealth != null)
            targetHealth.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (isDead)
            return;

        float speed = 0f;

        if (boss != null && boss.IsActivated &&
            agent != null && agent.isOnNavMesh)
        {
            speed = agent.velocity.magnitude / Mathf.Max(agent.speed, 0.01f);
        }

        animator.SetFloat(Speed,Mathf.Clamp01(speed),animationSmooth,Time.deltaTime);
    }

    private void HandleShoot()
    {
        if (isDead)
            return;

        animator.SetTrigger(Shoot);
    }

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;

        animator.SetFloat(Speed, 0f);
        animator.ResetTrigger(Shoot);
        animator.SetTrigger(Die);

        Debug.Log("Animacion de muerte del jefe.");
    }
}
