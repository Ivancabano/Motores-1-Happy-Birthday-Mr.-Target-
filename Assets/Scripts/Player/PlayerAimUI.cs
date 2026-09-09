using UnityEngine;

public class PlayerAimUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject crosshair;

    private void Update()
    {
        if (playerMovement == null || crosshair == null)
            return;

        crosshair.SetActive(playerMovement.IsAiming);
    }
}