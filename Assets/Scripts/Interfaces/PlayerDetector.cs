using UnityEngine;

public class PlayerDetectorTarget : MonoBehaviour, IDetectable
{
    public Transform GetTransform()
    {
        return transform;
    }

    public Vector3 GetCenterPosition()
    {
        return transform.position + Vector3.up * 1f;
    }
}