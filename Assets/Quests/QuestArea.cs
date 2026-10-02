using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class QuestArea : MonoBehaviour
{
    [SerializeField] int _questId;   //matches questdata.questid
    [SerializeField] EnemySpawnConfig[] _enemyConfig;
    //[SerializeField] float _respawnDelay = 5f;        unncesesary since enemyspawnconfig has it

    private class SpawnedEnemy
    {
        public Enemy enemy;
        public EnemySpawnConfig config;
    }

    List<SpawnedEnemy> _activeEnemies = new List<SpawnedEnemy>();
    bool _questActive = false;
    bool _subscribed = false;

    private void OnEnable()
    {
        _subscribed = false; // reset flag when enabled
    }

    private void Update()
    {
        // Try to subscribe once QuestManager is available
        if (!_subscribed && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged += HandleQuestChanged;
            _subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (_subscribed && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged -= HandleQuestChanged;
            _subscribed = false;
        }
    }

    private void HandleQuestChanged(int newQuestId)
    {
        if (newQuestId == _questId)
        {
            StartQuest();
        }
        else if (_questActive)
        {
            EndQuest();
        }
    }

    private void StartQuest()
    {
        _questActive = true;
        SpawnAllEnemies();
    }

    private void EndQuest()
    {
        _questActive = false;
        //DespawnAllEnemies();
    }

    public void SpawnAllEnemies()
    {
        foreach (EnemySpawnConfig config in _enemyConfig)
        {
            Enemy enemy = ObjectPoolManager.instance.GetEnemy(config.enemyPrefab);

            for (int i = 0; i < config.amount; i++)
            {
                SpawnEnemy(config);
            }
        }
    }

    private void SpawnEnemy(EnemySpawnConfig config)
    {
        Enemy enemy = ObjectPoolManager.instance.GetEnemy(config.enemyPrefab);

        if (enemy == null) return;

        Vector3 spawnPos = config.spawnPoint.position;
        
        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
        }

        // Use Warp to place agent properly on NavMesh
        enemy.transform.position = spawnPos;

        // Initialize with a small delay to let agent settle
        enemy.SetSpawnPoint(spawnPos);
        enemy.SetQuestArea(this);
        
        StartCoroutine(InitializeEnemyDelayed(enemy));

        _activeEnemies.Add(new SpawnedEnemy
        {
            enemy = enemy,
            config = config
        });
    }

    private IEnumerator InitializeEnemyDelayed(Enemy enemy)
    {
        yield return null; // wait one frame
        enemy.Initialize();
        enemy.gameObject.SetActive(true);
        enemy.SyncActiveState(true);
    }

    public void DespawnAllEnemies()
    {
        foreach (SpawnedEnemy spawnedEnemy in _activeEnemies)
        {
            Enemy enemy = spawnedEnemy.enemy;

            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemy.gameObject.SetActive(false);
            }
        }
        _activeEnemies.Clear();
    }

    public void OnEnemyDied(Enemy enemy)
    {
        if(!_questActive) return;

        SpawnedEnemy spawnedEnemy = _activeEnemies.Find(
            x => x.enemy == enemy
            );

        if(spawnedEnemy == null) return;

        EnemySpawnConfig config = spawnedEnemy.config;

        _activeEnemies.Remove(spawnedEnemy);

        StartCoroutine(RespawnEnemyAfterDelay(config));
    }

    private IEnumerator RespawnEnemyAfterDelay(EnemySpawnConfig config)
    {
        yield return new WaitForSeconds(config.respawnDelay);

        if (!_questActive) yield break;

        SpawnEnemy(config);
    }
}
