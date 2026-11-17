using UnityEngine;

public class HeadHitbox : MonoBehaviour, IHeadshot
{
    [SerializeField] private TargetHealth targetHealth;

    private void Awake()
    {
        // Si está en un objeto hijo, busca el TargetHealth en el padre
        if (targetHealth == null)
            targetHealth = GetComponentInParent<TargetHealth>();
    }

    public void OnHeadshot()
    {
        Debug.Log("¡Disparo crítico en la CABEZA!");

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(targetHealth.MaxHealth);
        }
    }
}