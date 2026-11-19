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
    [SerializeField] private AudioClip buttonPressClip;     
    [SerializeField] private AudioClip incorrectPressClip; 
    private bool isActivated = false;
    private bool isRequiredInStealth = false;

    private void Start()
    {
        SetVisualState(false);

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
            
                isActivated = true;
                SetVisualState(true);

                if (buttonAudioSource != null && buttonPressClip != null)
                {
                    buttonAudioSource.PlayOneShot(buttonPressClip);
                }

                Debug.Log($"Botón {gameObject.name} activado correctamente.");
            }
            else
            {
             
                isActivated = true; 

              
                if (buttonIndicatorRenderer != null)
                    buttonIndicatorRenderer.gameObject.SetActive(false);

                if (buttonLightComponent != null)
                    buttonLightComponent.enabled = false;

                
                if (buttonAudioSource != null && incorrectPressClip != null)
                {
                    buttonAudioSource.PlayOneShot(incorrectPressClip);
                }

                Debug.Log($"Botón {gameObject.name} incorrecto: se apaga y emite error.");
            }
        }
    }

  
    public void ResetButton()
    {
        isActivated = false;

     
        if (buttonIndicatorRenderer != null)
            buttonIndicatorRenderer.gameObject.SetActive(true);

        if (buttonLightComponent != null)
            buttonLightComponent.enabled = true;

        SetVisualState(false);
    }
}