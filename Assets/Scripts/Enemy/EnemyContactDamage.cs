
using System;
using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Daño Melee")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageCooldown = 1.5f;

    private float lastDamageTime = -999f;

    // Evento que activa la animacion Punch
    public event Action OnAttack;

    public void TryAttack(Transform target)
    {
        if (target == null)
            return;

        // Comprobamos el cooldown
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        // Buscamos el sistema de vida del jugador
        IDamageable damageable =
            target.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            Debug.LogWarning(
                "[EnemyContactDamage] No se encontro IDamageable."
            );
            return;
        }

        // Guardamos el momento del ataque
        lastDamageTime = Time.time;

        // Activamos la animacion
        OnAttack?.Invoke();

        // Aplicamos daño
        damageable.TakeDamage(damageAmount);

        Debug.Log(
            "[EnemyContactDamage] Golpe melee realizado."
        );
    }
}
