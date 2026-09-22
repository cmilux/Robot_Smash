using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuestArea : MonoBehaviour
{
    [SerializeField] int _questId;   //matches questdata.questid
    [SerializeField] EnemySpawnConfig[] _enemyConfig;
    [SerializeField] float _respawnDelay = 5f;

    List<Enemy> _activeEnemies = new List<Enemy>();
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
        DespawnAllEnemies();
    }

    public void SpawnAllEnemies()
    {
        foreach (EnemySpawnConfig config in _enemyConfig)
        {
            Enemy enemy = ObjectPoolManager.instance.GetEnemy(config.enemyPrefab);

            if (enemy == null) continue;

            Vector3 spawnPos = config.spawnPoint.position;
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }

            // Use Warp to place agent properly on NavMesh
            enemy.transform.position = spawnPos;

            // Initialize with a small delay to let agent settle
            enemy.SetSpawnPoint(spawnPos);
            StartCoroutine(InitializeEnemyDelayed(enemy));

            _activeEnemies.Add(enemy);
        }
    }

    private IEnumerator InitializeEnemyDelayed(Enemy enemy)
    {
        yield return null; // wait one frame
        enemy.Initialize();
        enemy.gameObject.SetActive(true);
    }

    public void DespawnAllEnemies()
    {
        foreach (Enemy enemy in _activeEnemies)
        {
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

        _activeEnemies.Remove(enemy);
        StartCoroutine(RespawnEnemyAfterDelay(enemy, _respawnDelay));
    }

    private IEnumerator RespawnEnemyAfterDelay(Enemy enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_questActive && enemy != null)
        {
            enemy.Initialize();
            enemy.gameObject.SetActive(true);
            _activeEnemies.Add(enemy);
        }
    }
}
