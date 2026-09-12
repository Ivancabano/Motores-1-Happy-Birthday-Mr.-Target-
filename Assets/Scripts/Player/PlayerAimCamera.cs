using UnityEngine;
using Unity.Cinemachine;

public class PlayerAimCamera : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineThirdPersonFollow thirdPersonFollow;

    [Header("Cámara normal")]
    [SerializeField] private float normalDistance = 3.5f;
    [SerializeField] private Vector3 normalShoulderOffset =
        new Vector3(0.4f, 0.2f, 0f);
    [SerializeField] private float normalFOV = 60f;

    [Header("Cámara apuntando")]
    [SerializeField] private float aimDistance = 2.2f;
    [SerializeField] private Vector3 aimShoulderOffset =
        new Vector3(0.65f, 0.25f, 0f);
    [SerializeField] private float aimFOV = 45f;

    [Header("Transición")]
    [SerializeField] private float transitionSpeed = 8f;

    private void Update()
    {
        if (playerMovement == null || cinemachineCamera == null || thirdPersonFollow == null)
        {
            return;
        }

        UpdateAimCamera();
    }

    private void UpdateAimCamera()
    {
        bool isAiming = playerMovement.IsAiming;

        float targetDistance = isAiming ? aimDistance : normalDistance;

        Vector3 targetShoulderOffset = isAiming ? aimShoulderOffset : normalShoulderOffset;

        float targetFOV = isAiming ? aimFOV : normalFOV;

        float t = transitionSpeed * Time.deltaTime;

        // Distancia
        thirdPersonFollow.CameraDistance = Mathf.Lerp(thirdPersonFollow.CameraDistance,targetDistance,t);

        // Hombro
        thirdPersonFollow.ShoulderOffset =Vector3.Lerp(thirdPersonFollow.ShoulderOffset,targetShoulderOffset,t);

        // FOV
        LensSettings lens = cinemachineCamera.Lens;

        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView,targetFOV,t);

        cinemachineCamera.Lens = lens;
    }
}