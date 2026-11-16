using UnityEngine;

public class AmmoRecorder : MonoBehaviour
{
    private void OnEnable() => PlayerAmmo.OnAmmoChanged += HandleAmmoChanged;
    private void OnDisable() => PlayerAmmo.OnAmmoChanged -= HandleAmmoChanged;

    private void HandleAmmoChanged(int magazine, int reserve)
    {
        RunData.SaveAmmo(magazine, reserve);
    }
}