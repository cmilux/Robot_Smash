using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig : MonoBehaviour
{
    public Enemy enemyPrefab;
    public Transform spawnPoint;
    public float respawnDelay = 5f;
}
