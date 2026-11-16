using UnityEngine;

public static class RunData
{
    public static bool HasData { get; private set; }
    public static float Health { get; private set; }
    public static float RemainingTime { get; private set; }

    public static bool HasAmmo { get; private set; }
    public static int Magazine { get; private set; }
    public static int Reserve { get; private set; }

    public static void Save(float health, float remainingTime)
    {
        Health = health;
        RemainingTime = remainingTime;
        HasData = true;
    }

    public static void SaveAmmo(int magazine, int reserve)
    {
        Magazine = magazine;
        Reserve = reserve;
        HasAmmo = true;
    }

    public static void Clear()
    {
        HasData = false;
        HasAmmo = false;
    }
}