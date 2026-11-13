using UnityEngine;

public class HealthPickup : Pickup
{
    [Header("Health")]
    [SerializeField] private float healAmount = 25f;

    protected override bool TryApply(GameObject collector)
    {
        PlayerHealth health = collector.GetComponentInParent<PlayerHealth>();

        if (health == null || health.IsDead)
            return false;

        if (health.CurrentHealth >= health.MaxHealth)
            return false;

        health.Heal(healAmount);
        return true;
    }
}