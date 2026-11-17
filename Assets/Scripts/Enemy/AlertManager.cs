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
        // --- ESTO IMPRIMIRÁ EN LA CONSOLA EXACTAMENTE QUIÉN LLAMÓ A LA ALARMA ---
        Debug.LogError("¡ALARMA ACTIVADA DESDE!: " + new System.Diagnostics.StackTrace());
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
