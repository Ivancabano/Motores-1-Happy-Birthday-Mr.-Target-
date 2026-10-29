using UnityEngine;
using UnityEngine.AI;
using static EnemyController;

public class AlertManager : MonoBehaviour
{
    [HideInInspector] public bool isAlertActive = false;
    string[] enemyTags = { "EnemyMelee", "EnemyDistance" };

    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void EnemyOnSight()
    {
      if (!isAlertActive)
        {
          isAlertActive = true;
          RankManager.Instance?.RegisterDetection();
          DynamicMusicManager.Instance?.StartAlert();
          foreach (string currentTag in enemyTags)
            {
                GameObject[] enemiesWithThisTag = GameObject.FindGameObjectsWithTag(currentTag);
                foreach(GameObject enemy in enemiesWithThisTag)

                if (enemy.TryGetComponent<EnemyController>(out EnemyController EnemyController))
                {
                    EnemyController.ForceChase();
                }
            }
        }
    }
}
