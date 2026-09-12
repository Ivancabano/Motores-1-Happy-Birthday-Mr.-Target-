using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Aim")]
    [SerializeField] private float aimDistance = 100f;
    [SerializeField] private LayerMask aimMask;

    private Vector3 aimPoint;

    public Vector3 AimPoint => aimPoint;

    private void Update()
    {
        if (mainCamera == null || playerMovement == null)
            return;

        CalculateAimPoint();
    }

    private void CalculateAimPoint()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, aimDistance,aimMask))
        {
            aimPoint = hit.point;
        }
        else
        {
            aimPoint = mainCamera.transform.position + mainCamera.transform.forward * aimDistance;
        }
    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null)
            return;

        Gizmos.DrawLine(mainCamera.transform.position,aimPoint);

        Gizmos.DrawSphere(aimPoint,0.1f);
    }
}