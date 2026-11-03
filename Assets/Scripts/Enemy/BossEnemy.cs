
using UnityEngine;

public class BossEnemy : EnemyBase
{
    private bool bossActivated = false;

    // El jefe no participa de las alertas generales.
    protected override void OnEnable()
    {
    }

    protected override void OnDisable()
    {
    }

    protected override void Update()
    {
        // No hace nada hasta que entramos a la sala.
        if (!bossActivated)
            return;

        base.Update();
    }

    protected override void HandlePatrol()
    {
        // El jefe no patrulla.
    }

    public void ActivateBoss()
    {
        if (bossActivated)
            return;

        bossActivated = true;

        // Entra directamente en persecucion.
        ForceChase();

        Debug.Log("JEFE ACTIVADO");
    }
}
