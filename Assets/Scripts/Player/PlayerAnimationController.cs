


using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CharacterController characterController;

    [Header("Configuracion")]
    [SerializeField] private float animationSmooth = 0.1f;

    private Animator animator;
    private bool isDead = false;

    // Parametros del Animator
    private static readonly int Speed = Animator.StringToHash("Speed");

    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

    private static readonly int IsAiming = Animator.StringToHash("IsAiming");

    private static readonly int Die = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();

        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        // Si murio, no actualizamos otras animaciones.
        if (isDead)
            return;

        if (playerMovement == null ||characterController == null)
            return;

        // Obtenemos la velocidad horizontal.
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

        // Evitamos que el dash active la carrera.
        if (playerMovement.IsDashing)
            speed = 0f;

        // Actualizamos el Blend Tree.
        animator.SetFloat(Speed,speed,animationSmooth,Time.deltaTime);

        // Actualizamos el agachado.
        animator.SetBool(IsCrouching,playerMovement.IsCrouching);

        // Actualizamos el apuntado.
        animator.SetBool( IsAiming,playerMovement.IsAiming);
    }

    private void HandleDeath()
    {
        // Evitamos reproducir la muerte varias veces.
        if (isDead)
            return;

        isDead = true;

        // Activamos la animacion de muerte.
        animator.SetTrigger(Die);

        Debug.Log("Reproduciendo animacion de muerte.");
    }
    public void OnDeathAnimationFinished()
{
    DeathSequence sequence = GetComponentInParent<DeathSequence>();

    if (sequence == null)
    {
        Debug.LogError("No se encontro DeathSequence en el personaje.",this);
        return;
    }

    sequence.FinishDeath();
}

}



