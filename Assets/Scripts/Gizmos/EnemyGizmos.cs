using UnityEngine;

public class EnemyGizmos : MonoBehaviour
{
    private EnemyBase enemy;

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    private void OnDrawGizmos()
    {
        if (enemy == null) enemy = GetComponent<EnemyBase>();
        if (enemy == null) return;

        Vector3 eyePosition = enemy.EyePosition;
        float visionDistance = enemy.VisionDistance;
        float visionAngle = enemy.VisionAngle;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition, visionDistance);

        Vector3 leftLimit = Quaternion.AngleAxis(-visionAngle / 2, transform.up) * transform.forward;
        Vector3 rightLimit = Quaternion.AngleAxis(visionAngle / 2, transform.up) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(eyePosition, leftLimit * visionDistance);
        Gizmos.DrawRay(eyePosition, rightLimit * visionDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(eyePosition, transform.forward * visionDistance);

        Gizmos.color = Color.green;
        Vector3 leftPoint = eyePosition + leftLimit * visionDistance;
        Vector3 rightPoint = eyePosition + rightLimit * visionDistance;
        Gizmos.DrawLine(leftPoint, rightPoint);
    }
}