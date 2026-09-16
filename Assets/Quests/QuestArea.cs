using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestArea : MonoBehaviour
{
    [SerializeField] int _questId;   //matches questdata.questid
    [SerializeField] EnemySpawnConfig[] _enemyConfig;
    [SerializeField] float _respawnDelay = 5f;

    List<Enemy> _activeEnemies = new List<Enemy>();
    bool _questActive = false;

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged += HandleQuestChanged;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged -= HandleQuestChanged;
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
        Debug.Log($"[QuestArea] Quest {_questId} started");
        _questActive = true;
        SpawnAllEnemies();
    }

    private void EndQuest()
    {
        Debug.Log($"[QuestArea] Quest {_questId} ended");
        _questActive = false;
        DespawnAllEnemies();
    }

    public void SpawnAllEnemies()
    {
        foreach (EnemySpawnConfig config in _enemyConfig)
        {
            Debug.Log($"trying to get enemie: {config.enemyPrefab.name}");

            Enemy enemy = ObjectPoolManager.instance.GetEnemy(config.enemyPrefab);

            if (enemy == null)
            {
                Debug.Log($"enemies null for {config.enemyPrefab.name}");
            }

            enemy.transform.position = config.spawnPoint.position;
            enemy.SetSpawnPoint(config.spawnPoint.position);
            enemy.gameObject.SetActive(true);

            _activeEnemies.Add(enemy);
            Debug.Log($"spawned {config.enemyPrefab.name} at {config.spawnPoint.name}");
        }
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
