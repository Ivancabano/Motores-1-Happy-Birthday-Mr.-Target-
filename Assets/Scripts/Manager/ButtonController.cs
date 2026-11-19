using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DoorManager doorManager;

    [Header("Configuración de la Luz (Hijo del Botón)")]
    [SerializeField] private Renderer buttonIndicatorRenderer;
    [SerializeField] private Light buttonLightComponent;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;

    [Header("Audio del Botón")]
    [SerializeField] private AudioSource buttonAudioSource;
    [SerializeField] private AudioClip buttonPressClip;     // Sonido al acertar (correcto)
    [SerializeField] private AudioClip incorrectPressClip;  // Nuevo: Sonido al fallar (incorrecto)

    private bool isActivated = false;
    private bool isRequiredInStealth = false;

    private void Start()
    {
        SetVisualState(false); // Inicia en rojo

        if (doorManager != null)
        {
            doorManager.RegisterButton(this);
        }
    }

    public void SetAsRequiredStealth(bool required)
    {
        isRequiredInStealth = required;
    }

    private void SetVisualState(bool activated)
    {
        if (buttonIndicatorRenderer != null)
        {
            buttonIndicatorRenderer.material = activated ? greenMaterial : redMaterial;
        }

        if (buttonLightComponent != null)
        {
            buttonLightComponent.color = activated ? Color.green : Color.red;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDoor>(out IDoor door) && !isActivated)
        {
            bool success = doorManager.TryPressButton(this);

            if (success)
            {
                // --- ACERTÓ EL BOTÓN ---
                isActivated = true;
                SetVisualState(true); // Cambia a verde

                if (buttonAudioSource != null && buttonPressClip != null)
                {
                    buttonAudioSource.PlayOneShot(buttonPressClip);
                }

                Debug.Log($"Botón {gameObject.name} activado correctamente.");
            }
            else
            {
                // --- FALLÓ (BOTÓN INCORRECTO) ---
                isActivated = true; // Lo bloqueamos para que no se pueda reintentar de inmediato

                // Apagamos la luz visual y el componente Light
                if (buttonIndicatorRenderer != null)
                    buttonIndicatorRenderer.gameObject.SetActive(false);

                if (buttonLightComponent != null)
                    buttonLightComponent.enabled = false;

                // Reproducimos el sonido de error
                if (buttonAudioSource != null && incorrectPressClip != null)
                {
                    buttonAudioSource.PlayOneShot(incorrectPressClip);
                }

                Debug.Log($"Botón {gameObject.name} incorrecto: se apaga y emite error.");
            }
        }
    }

    // Método para resetear el botón (vuelve a encenderlo en rojo si se reinicia el estado)
    public void ResetButton()
    {
        isActivated = false;

        // Volvemos a encender los componentes visuales por si se habían apagado
        if (buttonIndicatorRenderer != null)
            buttonIndicatorRenderer.gameObject.SetActive(true);

        if (buttonLightComponent != null)
            buttonLightComponent.enabled = true;

        SetVisualState(false);
    }
}