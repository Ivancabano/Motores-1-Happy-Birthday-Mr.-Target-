using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossReinforcements : MonoBehaviour
{
    [Header("Refuerzos")]
    [Tooltip("Prefab del guardia que aparece (Guardia Melee Final).")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Un punto por puerta. Aparece un guardia en cada uno.")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Porcentaje de vida del jefe (0 a 1) al que aparecen los refuerzos.")]
    [SerializeField, Range(0f, 1f)] private float healthThreshold = 0.5f;

    [Header("Reglas")]
    [Tooltip("Si esta desactivado, matar a los refuerzos no afecta el rango final.")]
    [SerializeField] private bool countForRank = false;

    [Tooltip("Si esta activado, los refuerzos usan la vida de alerta (mas resistentes).")]
    [SerializeField] private bool useAlertHealth = false;

    [Tooltip("Distancia maxima para acomodar el punto de aparicion sobre el NavMesh.")]
    [SerializeField] private float navMeshSearchRadius = 2f;

    private bool spawned;
    private bool stopped;
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    private void OnEnable()
    {
        GameEvents.OnBossHealthChanged += HandleBossHealthChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnBossHealthChanged -= HandleBossHealthChanged;
    }

    private void HandleBossHealthChanged(float current, float max)
    {
        if (max <= 0f)
            return;

        if (current <= 0f)
        {
            StopReinforcements();
            return;
        }

        if (!spawned && current / max <= healthThreshold)
            SpawnReinforcements();
    }

    private void SpawnReinforcements()
    {
        spawned = true;

        if (enemyPrefab == null)
        {
            Debug.LogWarning("[BossReinforcements] No hay prefab de guardia asignado.", this);
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[BossReinforcements] No hay puntos de aparicion asignados.", this);
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;

            Vector3 position = point.position;

            if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
                position = hit.position;
            else
                Debug.LogWarning("[BossReinforcements] " + point.name + " no esta sobre el NavMesh: el guardia no va a poder caminar.", point);

            GameObject enemy = Instantiate(enemyPrefab, position, point.rotation);
            spawnedEnemies.Add(enemy);
            StartCoroutine(Engage(enemy));
        }

        Debug.Log("[BossReinforcements] Aparecieron " + spawnedEnemies.Count + " refuerzos.");
    }

    private IEnumerator Engage(GameObject enemy)
    {
        yield return null;

        if (enemy == null || stopped)
            yield break;

        TargetHealth health = enemy.GetComponentInChildren<TargetHealth>();
        EnemyBase enemyBase = enemy.GetComponentInChildren<EnemyBase>();

        if (enemyBase != null)
            enemyBase.ForceChase();

        if (health != null)
        {
            health.SetCountsForRank(countForRank);

            if (!useAlertHealth)
                health.SetAlertState(false);
        }
    }

    private void StopReinforcements()
    {
        if (stopped)
            return;

        stopped = true;

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy == null)
                continue;

            foreach (EnemyBase enemyBase in enemy.GetComponentsInChildren<EnemyBase>())
                enemyBase.enabled = false;

            foreach (EnemyContactDamage contact in enemy.GetComponentsInChildren<EnemyContactDamage>())
                contact.enabled = false;

            foreach (NavMeshAgent agent in enemy.GetComponentsInChildren<NavMeshAgent>())
            {
                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (spawnPoints == null)
            return;

        Gizmos.color = Color.magenta;
        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawWireSphere(point.position, 0.4f);
            Gizmos.DrawLine(point.position, point.position + point.forward);
        }
    }
}
