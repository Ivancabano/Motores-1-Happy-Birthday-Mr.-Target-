using UnityEngine;

public static class RunData
{
    public static bool HasData { get; private set; }
    public static float Health { get; private set; }
    public static float RemainingTime { get; private set; }

    public static void Save(float health, float remainingTime)
    {
        Health = health;
        RemainingTime = remainingTime;
        HasData = true;
        Debug.Log($"[RunData] GUARDADO: vida {health} | tiempo {remainingTime:0.0}");
    }

    public static void Clear() => HasData = false;
}