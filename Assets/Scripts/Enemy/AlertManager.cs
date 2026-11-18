using System.Collections.Generic;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    [SerializeField] TargetHealth targetHealth;
    private bool isAlertActive = false;
    private static readonly HashSet<IAlertable> alertableEnemies = new HashSet<IAlertable>();
    private void Start()
    {
        targetHealth = GetComponent<TargetHealth>();
    }
    public static void Register(IAlertable alertable)
    {
        if (alertable != null) alertableEnemies.Add(alertable);
    }

    public static void Unregister(IAlertable alertable)
    {
        if (alertable != null) alertableEnemies.Remove(alertable);
    }
    public void EnemyOnSight(float distance)
    {
        if(isAlertActive) return;
        Debug.Log("enemigo detectado");
        if (DynamicMusicManager.Instance != null)
        {
            DynamicMusicManager.Instance.StartAlert(distance);
        }
        DoorManager[] todasLasPuertas = FindObjectsOfType<DoorManager>();
        foreach (DoorManager puerta in todasLasPuertas)
        {
            puerta.SetAlertMode(true);
        }
        if (!isAlertActive)
        {
            isAlertActive = true;
            foreach (var alertable in alertableEnemies)
            {
                alertable?.ForceChase();

            }
        }
    }
}
