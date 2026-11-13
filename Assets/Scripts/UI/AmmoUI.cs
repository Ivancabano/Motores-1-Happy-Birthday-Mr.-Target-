using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text ammoText;

    private void OnEnable()
    {
        PlayerAmmo.OnAmmoChanged += SetAmmo;
    }

    private void OnDisable()
    {
        PlayerAmmo.OnAmmoChanged -= SetAmmo;
    }

    public void SetAmmo(int magazine, int reserve)
    {
        ammoText.text = $"{magazine} / {reserve}";
    }

    [ContextMenu("Probar: 12 / 36")]
    private void TestAmmo() => SetAmmo(12, 36);
}