using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    public Enemy enemyPrefab;
    public Transform spawnPoint;
    public int amount;
    public float respawnDelay = 5f;
}
