using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAmmo : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int startingReserve = 24;
    [SerializeField] private int maxReserve = 60;

    [Header("Reload")]
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private InputActionReference reloadAction;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip emptyClip;

    public static event Action<int, int> OnAmmoChanged;
    public static event Action<bool> OnReloadStateChanged;

    public int CurrentMagazine { get; private set; }
    public int CurrentReserve { get; private set; }
    public bool IsReloading { get; private set; }
    public bool IsReserveFull => CurrentReserve >= maxReserve;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        CurrentMagazine = magazineSize;
        CurrentReserve = Mathf.Min(startingReserve, maxReserve);
    }

    private void OnEnable()
    {
        if (reloadAction?.action != null)
        {
            reloadAction.action.Enable();
            reloadAction.action.performed += OnReloadPerformed;
        }
    }

    private void OnDisable()
    {
        if (reloadAction?.action != null)
        {
            reloadAction.action.performed -= OnReloadPerformed;
            reloadAction.action.Disable();
        }

        if (IsReloading)
        {
            StopAllCoroutines();
            IsReloading = false;
            OnReloadStateChanged?.Invoke(false);
        }
    }

    private void Start()
    {
        NotifyAmmoChanged();
    }

    public bool TryConsume()
    {
        if (IsReloading)
            return false;

        if (CurrentMagazine <= 0)
        {
            PlayClip(emptyClip);
            TryStartReload();
            return false;
        }

        CurrentMagazine--;
        NotifyAmmoChanged();
        return true;
    }

    public void AddReserve(int amount)
    {
        if (amount <= 0)
            return;

        CurrentReserve = Mathf.Min(CurrentReserve + amount, maxReserve);
        NotifyAmmoChanged();
    }

    public void TryStartReload()
    {
        if (IsReloading)
            return;
        if (CurrentMagazine >= magazineSize)
            return;
        if (CurrentReserve <= 0)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        TryStartReload();
    }

    private IEnumerator ReloadRoutine()
    {
        IsReloading = true;
        OnReloadStateChanged?.Invoke(true);
        PlayClip(reloadClip);

        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - CurrentMagazine;
        int toLoad = Mathf.Min(needed, CurrentReserve);

        CurrentMagazine += toLoad;
        CurrentReserve -= toLoad;

        IsReloading = false;
        OnReloadStateChanged?.Invoke(false);
        NotifyAmmoChanged();
    }

    private void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(CurrentMagazine, CurrentReserve);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}