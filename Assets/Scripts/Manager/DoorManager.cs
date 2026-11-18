using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [Header("Configuración de las 3 Luces de la Puerta")]
    [SerializeField] private List<Renderer> lightIndicatorRenderers = new List<Renderer>();
    [SerializeField] private List<Light> lightComponents = new List<Light>();
    [SerializeField] private Material redLightMaterial;
    [SerializeField] private Material greenLightMaterial;

    [Header("Animación y Audio de la Puerta")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip lockedDoorClip;
    [SerializeField] private AudioClip openDoorClip;

    [Header("Configuración de Botones")]
    [SerializeField] private List<ButtonController> registeredButtons = new List<ButtonController>();
    private ButtonController correctStealthButton;

    private bool isAlertMode = false;
    private int totalButtonsRequired = 1;
    private int pressedButtonsCount = 0;
    private bool isDoorOpen = false;

    private void Start()
    {
        if (doorAudioSource != null)
        {
            doorAudioSource.playOnAwake = false;
            doorAudioSource.loop = false;
        }

        UpdateVisuals();
    }

    public void RegisterButton(ButtonController button)
    {
        if (!registeredButtons.Contains(button))
        {
            registeredButtons.Add(button);
        }

        if (!isAlertMode && registeredButtons.Count > 0 && correctStealthButton == null)
        {
            ChooseRandomStealthButton();
        }
    }

    private void ChooseRandomStealthButton()
    {
        if (registeredButtons.Count == 0) return;

        foreach (var btn in registeredButtons)
        {
            btn.SetAsRequiredStealth(false);
        }

        int randomIndex = Random.Range(0, registeredButtons.Count);
        correctStealthButton = registeredButtons[randomIndex];
        correctStealthButton.SetAsRequiredStealth(true);
    }

    public void SetAlertMode(bool alertActive)
    {
        if (isDoorOpen) return;

        isAlertMode = alertActive;
        pressedButtonsCount = 0;

        if (isAlertMode)
        {
            totalButtonsRequired = 3;
            foreach (var btn in registeredButtons)
            {
                btn.SetAsRequiredStealth(false);
            }
        }
        else
        {
            totalButtonsRequired = 1;
            ChooseRandomStealthButton();
        }

        UpdateVisuals();
    }

    public bool TryPressButton(ButtonController pressedButton)
    {
        if (isDoorOpen) return false;

        if (!isAlertMode)
        {
            // Modo Sigilo: Si acierta al botón correcto, abre de golpe
            if (pressedButton == correctStealthButton)
            {
                pressedButtonsCount = 1;
                OpenDoor();
                UpdateVisuals();
                return true;
            }
            return false;
        }
        else
        {
            // Modo Alerta: Exige acumular los 3 botones secuenciales
            pressedButtonsCount++;
            UpdateVisuals();

            if (pressedButtonsCount >= totalButtonsRequired)
            {
                OpenDoor();
            }
            return true;
        }
    }

    private void OpenDoor()
    {
        isDoorOpen = true;

        if (doorAudioSource != null && openDoorClip != null)
        {
            doorAudioSource.PlayOneShot(openDoorClip);
        }

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }

        Collider doorCollider = GetComponent<Collider>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < lightIndicatorRenderers.Count; i++)
        {
            bool shouldBeGreen = false;
            bool shouldBeOn = true;

            if (!isAlertMode)
            {
                // MODO SIGILO: Solo la primera luz encendida en rojo (hasta abrirse)
                shouldBeOn = (i == 0);
                shouldBeGreen = isDoorOpen;
            }
            else
            {
                // MODO ALERTA: Las 3 luces encendidas. Se vuelven verdes según los botones pulsados.
                shouldBeOn = true;
                shouldBeGreen = (i < pressedButtonsCount) || isDoorOpen;
            }

            // Aplicar material
            if (lightIndicatorRenderers[i] != null)
            {
                lightIndicatorRenderers[i].gameObject.SetActive(shouldBeOn);
                lightIndicatorRenderers[i].material = shouldBeGreen ? greenLightMaterial : redLightMaterial;
            }

            // Aplicar color a la luz real de Unity
            if (i < lightComponents.Count && lightComponents[i] != null)
            {
                lightComponents[i].enabled = shouldBeOn;
                lightComponents[i].color = shouldBeGreen ? Color.green : Color.red;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDoor>(out IDoor player) && !isDoorOpen)
        {
            if (doorAudioSource != null && lockedDoorClip != null)
            {
                doorAudioSource.PlayOneShot(lockedDoorClip);
            }
        }
    }
}