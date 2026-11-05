using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    [Header("Referencias (Autogestionadas)")]
    private EnemyBossBase bossBase;
    private NavMeshAgent agent;
    private BossShotgunAttack shotgun;
    private TargetHealth targetHealth;

    [Header("Configuración de Animación")]
    [SerializeField] private float animationSmooth = 0.1f;

    private Animator animator;
    private bool isDead = false;

    // Hashes del Animator para optimizar rendimiento
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Die = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        SearchReferences();
    }

    private void SearchReferences()
    {
        // Busca en el mismo GameObject o en los padres (por si el script está en el objeto de la malla 3D)
        if (bossBase == null)
            bossBase = GetComponentInParent<EnemyBossBase>();

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

        // Validamos utilizando la propiedad IsCombatActive de EnemyBossBase
        if (bossBase != null && bossBase.IsCombatActive && agent != null && agent.isOnNavMesh)
        {
            speed = agent.velocity.magnitude / Mathf.Max(agent.speed, 0.01f);
        }

        animator.SetFloat(Speed, Mathf.Clamp01(speed), animationSmooth, Time.deltaTime);
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

        Debug.Log("Animación de muerte del jefe activada.");
    }
}