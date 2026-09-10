using Unity.VisualScripting;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public AlertManager alertManager;
    public Transform Target;
    private Light cameraLight;
    public LayerMask obstacleLayer;
    [HideInInspector] public bool playerDetected = false;
    [Header("Zonification Mechanics")]
    public bool rotateCamera = true;
    public float zonificationAngle = 40f;
    public float rotationSpeed = 1.5f;
    private Quaternion initialRotation;
    [HideInInspector] public bool isAlertActive = false;
    private float visionRange;
    private float visionAngle;
    private float lightEdgeTolerance = 1.2f;
    void Start()
    {
        cameraLight = GetComponentInChildren<Light>();

        if (cameraLight == null || cameraLight.type != LightType.Spot)
        {
            enabled = false;
            return;
        }
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (rotateCamera && !playerDetected)
        {
            float angleModifier = Mathf.Sin(Time.time * rotationSpeed) * zonificationAngle;
            transform.rotation = initialRotation * Quaternion.Euler(0, angleModifier, 0);
        }

        visionRange = cameraLight.range;
        visionAngle = cameraLight.spotAngle;

        if (CanSeePlayer())
        {
            if (!playerDetected)
            {
                playerDetected = true;
                alertManager.EnemyOnSight();
            }
        }
        else
        {
            if (playerDetected)
            {
                playerDetected = false;
            }
        }
    }

    bool CanSeePlayer()
    {
        if (Target == null || cameraLight == null) return false;

        Vector3 originPosition = cameraLight.transform.position;
        Vector3 playerCenterPosition = Target.position + Vector3.up * 1f;

        Vector3 directionToPlayer = (playerCenterPosition - originPosition).normalized;
        float currentDistance = Vector3.Distance(originPosition, playerCenterPosition);

        if (currentDistance <= visionRange)
        {
            float angleToPlayer = Vector3.Angle(cameraLight.transform.forward, directionToPlayer);
            float qualifiedCutoffAngle = (visionAngle / 2f) * lightEdgeTolerance;

            if (angleToPlayer < qualifiedCutoffAngle)
            {
                RaycastHit hit;

                if (Physics.Raycast(originPosition, directionToPlayer, out hit, visionRange + 2f))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        Debug.DrawLine(originPosition, hit.point, Color.red);
                        return true;
                    }
                    else
                    {
                        Debug.DrawLine(originPosition, hit.point, Color.blue);
                        return false;
                    }
                }
            }
        }
        return false;
    }
    private void OnDrawGizmos()
    {
        if (cameraLight == null) cameraLight = GetComponentInChildren<Light>();
        if (cameraLight == null || cameraLight.type != LightType.Spot) return;

        Gizmos.color = playerDetected ? Color.red : Color.cyan;
        Vector3 origin = cameraLight.transform.position;
        Gizmos.DrawRay(origin, cameraLight.transform.forward * cameraLight.range);
    }
}
