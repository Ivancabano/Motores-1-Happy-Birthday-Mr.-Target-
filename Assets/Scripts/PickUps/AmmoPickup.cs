using UnityEngine;

public class AmmoPickup : Pickup
{
    [Header("Ammo")]
    [SerializeField] private int ammoAmount = 12;

    protected override bool TryApply(GameObject collector)
    {
        PlayerAmmo ammo = collector.GetComponentInParent<PlayerAmmo>();

        if (ammo == null || ammo.IsReserveFull)
            return false;

        ammo.AddReserve(ammoAmount);
        return true;
    }
}