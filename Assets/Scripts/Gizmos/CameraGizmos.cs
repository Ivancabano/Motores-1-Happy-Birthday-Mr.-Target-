using UnityEngine;

public class CameraGizmos : MonoBehaviour
{
    private SecurityCamera securityCamera;

    private void Awake()
    {
        securityCamera = GetComponent<SecurityCamera>();
    }

    private void OnDrawGizmos()
    {
        if (securityCamera == null) securityCamera = GetComponent<SecurityCamera>();
        if (securityCamera == null) return;

        Light camLight = securityCamera.CameraLight;
        if (camLight == null || camLight.type != LightType.Spot) return;

        Gizmos.color = securityCamera.IsPlayerDetected ? Color.red : Color.cyan;
        Vector3 origin = camLight.transform.position;
        Gizmos.DrawRay(origin, camLight.transform.forward * camLight.range);
    }
}