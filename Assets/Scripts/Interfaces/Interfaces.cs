using UnityEngine;

public interface IAlertable
{
    void ForceChase();
    bool IsChasing { get; }
}

public interface IDetectable
{
    Transform GetTransform();
    Vector3 GetCenterPosition();
}