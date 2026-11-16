using UnityEngine;

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerStateRestorer : MonoBehaviour
{
    [SerializeField] private bool continueFromPreviousLevel = false;

    private void Start()
    {
        if (!continueFromPreviousLevel) return;

        if (RunData.HasData)
            GetComponent<PlayerHealth>().SetHealth(RunData.Health);

        if (RunData.HasAmmo)
        {
            PlayerAmmo ammo = GetComponentInChildren<PlayerAmmo>();
            if (ammo == null) ammo = FindFirstObjectByType<PlayerAmmo>();

            if (ammo != null)
                ammo.SetAmmo(RunData.Magazine, RunData.Reserve);
        }
    }
}