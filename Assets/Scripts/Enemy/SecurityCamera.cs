using UnityEngine;
public class SecurityCamera : MonoBehaviour
{
    private AlertManager alertManager;
    private Transform targetTransform;
    private IDetectable targetDetectable;
    private Light cameraLight;
    [SerializeField] private Transform focoTransform;
    
    [Header("Audio de Alerta (Opcional)")]
    [SerializeField] private AudioSource cameraAudioSource;
    [SerializeField] private AudioClip alertSound;
    private bool alertSoundPlayed = false;

    [SerializeField] private LayerMask obstacleLayer;
    private bool playerDetected = false;

    [Header("Zonification Mechanics")]
    [SerializeField] private bool rotateCamera = true;
    [SerializeField] private float zonificationAngle = 40f;
    [SerializeField] private float rotationSpeed = 1.5f;
    private Quaternion initialRotation;
    [Header("Audio de Alerta (Opcional)")]

    private float visionRange;
    private float visionAngle;
    private float lightEdgeTolerance = 1.2f;

    public bool IsPlayerDetected => playerDetected;
    public Light CameraLight => cameraLight;

    void Start()
    {
        SearchReferences();
    }
    private void SearchReferences()
    {
        cameraLight = GetComponentInChildren<Light>();

        if (cameraAudioSource == null)
        {
            cameraAudioSource = GetComponentInChildren<AudioSource>();
        }

        if (cameraLight == null || cameraLight.type != LightType.Spot)
        {
            enabled = false;
            return;
        }

        focoTransform = cameraLight.transform;
        initialRotation = focoTransform.rotation;

        alertManager = Object.FindFirstObjectByType<AlertManager>();

        if (targetTransform == null)
        {
            PlayerDetectorTarget playerTarget = Object.FindFirstObjectByType<PlayerDetectorTarget>();
            if (playerTarget != null)
            {
                targetTransform = playerTarget.transform;
            }
        }

        if (cameraLight == null || cameraLight.type != LightType.Spot)
        {
            enabled = false;
            return;
        }
        initialRotation = transform.rotation;

        if (targetTransform != null)
        {
            targetDetectable = targetTransform.GetComponent<IDetectable>();
        }
    }
    void Update()
    {
        if (rotateCamera && !playerDetected && focoTransform != null)
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
                if (alertManager != null && targetTransform != null)
                    alertManager.EnemyOnSight(Vector3.Distance(transform.position, targetTransform.position));

                if (alertSoundPlayed == false)
                {
                    cameraAudioSource.PlayOneShot(alertSound);
                    alertSoundPlayed = true;
                }
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
        if (targetTransform == null || cameraLight == null) return false;

        Vector3 originPosition = cameraLight.transform.position;
        Vector3 playerCenterPosition = targetDetectable != null ?
            targetDetectable.GetCenterPosition() :
            targetTransform.position + Vector3.up * 1f;

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
                    if (hit.transform.TryGetComponent<IDetectable>(out _))
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
}