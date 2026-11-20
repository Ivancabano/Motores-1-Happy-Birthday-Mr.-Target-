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
        if (button != null && !registeredButtons.Contains(button))
        {
            registeredButtons.Add(button);
            Debug.Log("boton registrado");
        }

        if (!isAlertMode && registeredButtons.Count > 0)
        {
            if (registeredButtons.Count == 1 || correctStealthButton == null)
            {
                ChooseRandomStealthButton();
            }
        }
    }

    private void ChooseRandomStealthButton()
    {
        if (registeredButtons.Count == 0) return;

        foreach (var btn in registeredButtons)
        {
            if (btn != null) btn.SetAsRequiredStealth(false);
        }

        if (registeredButtons.Count == 1)
        {
            correctStealthButton = registeredButtons[0];
            Debug.Log("boton correcto seleccionado: " + correctStealthButton.name);
        }
        else
        {
            int randomIndex = Random.Range(0, registeredButtons.Count);
            correctStealthButton = registeredButtons[randomIndex];
            Debug.Log("boton correcto seleccionado: " + correctStealthButton.name);
        }
        Debug.Log($"[DoorManager] Lista count: {registeredButtons.Count} | Elegido: {correctStealthButton.name} (InstanceID: {correctStealthButton.GetInstanceID()})");
        if (correctStealthButton != null)
        {
            correctStealthButton.SetAsRequiredStealth(true);
        }
    }

    public void SetAlertMode(bool alertActive)
    {
        if (isDoorOpen) return;

        isAlertMode = alertActive;
        pressedButtonsCount = 0;

        if (isAlertMode)
        {
            totalButtonsRequired = Mathf.Min(3, Mathf.Max(1, registeredButtons.Count));

            foreach (var btn in registeredButtons)
            {
                if (btn != null)
                {
                    btn.SetAsRequiredStealth(false);
                    btn.ResetButton(); // <-- Reactiva botones que fallaron en sigilo
                }
            }
        }
        else
        {
            totalButtonsRequired = 1;

            foreach (var btn in registeredButtons)
            {
                if (btn != null)
                {
                    btn.ResetButton(); // <-- Por si salís de alerta y querés limpiar estado
                }
            }

            ChooseRandomStealthButton();
        }

        UpdateVisuals();
    }

    public bool TryPressButton(ButtonController pressedButton)
    {
        if (isDoorOpen || pressedButton == null) return false;

        if (registeredButtons.Count == 1 && pressedButton == registeredButtons[0])
        {
            correctStealthButton = registeredButtons[0];
            pressedButtonsCount = 1;
            OpenDoor();
            UpdateVisuals();
            return true;
        }

        if (!isAlertMode)
        {
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
                shouldBeOn = (i == 0);
                shouldBeGreen = isDoorOpen;
            }
            else
            {
                shouldBeOn = true;
                shouldBeGreen = (i < pressedButtonsCount) || isDoorOpen;
            }

            if (lightIndicatorRenderers[i] != null)
            {
                lightIndicatorRenderers[i].gameObject.SetActive(shouldBeOn);
                lightIndicatorRenderers[i].material = shouldBeGreen ? greenLightMaterial : redLightMaterial;
            }

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