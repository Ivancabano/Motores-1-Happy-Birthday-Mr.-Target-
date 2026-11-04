using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text ammoText;

    [Header("Configuración")]
    [SerializeField] private bool infiniteAmmo = true;
    [SerializeField] private string infiniteSymbol = "∞";

    private void Start()
    {
        if (infiniteAmmo)
            ShowInfinite();
    }

    public void ShowInfinite()
    {
        infiniteAmmo = true;
        ammoText.text = infiniteSymbol;
    }

    public void SetAmmo(int magazine, int reserve)
    {
        infiniteAmmo = false;
        ammoText.text = $"{magazine} / {reserve}";
    }
    [ContextMenu("Probar: 12 / 36")]
    private void ProbarFinito() => SetAmmo(12, 36);

    [ContextMenu("Probar: infinito")]
    private void ProbarInfinito() => ShowInfinite();
}