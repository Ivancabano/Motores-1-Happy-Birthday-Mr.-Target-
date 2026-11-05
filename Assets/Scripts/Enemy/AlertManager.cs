using System.Collections.Generic;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    private bool isAlertActive = false;
    private static readonly HashSet<IAlertable> alertableEnemies = new HashSet<IAlertable>();

    public static void Register(IAlertable alertable)
    {
        if (alertable != null) alertableEnemies.Add(alertable);
    }

    public static void Unregister(IAlertable alertable)
    {
        if (alertable != null) alertableEnemies.Remove(alertable);
    }

    // distance = distancia entre quien detecta (guardia/camara) y el jugador
    public void EnemyOnSight(float distance)
    {
        // Snapshot de Alerta (el manager ignora llamadas repetidas)
        if (DynamicMusicManager.Instance != null)
        {
            DynamicMusicManager.Instance.StartAlert(distance);
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
