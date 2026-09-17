using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        CharacterController controller = other.GetComponent<CharacterController>();

        if (controller == null)
            return;

        // Desactivamos temporalmente el CharacterController
        // para cambiar la posición directamente.
        controller.enabled = false;

        other.transform.position = destination.position;
        other.transform.rotation = destination.rotation;

        controller.enabled = true;
    }
}