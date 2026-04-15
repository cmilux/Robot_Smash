using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class BigEnemy : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject kamikazeEnemy;
    public Transform player;
    public Transform[] spawnPoint;
    public KamikazeEnemy scriptEnemy;
    
    public float shootingCooldown = 3f;
    public float bulletTime;

    float maxEnemies = 3;

    private void Update()
    {
        agent.SetDestination(player.position);

        ShootPlayer();
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    void ShootPlayer()
    {
        int currentEnemies = GameObject.FindGameObjectsWithTag("SpawnedEnemies").Length;

        while (currentEnemies < maxEnemies)
        {
            bulletTime -= Time.deltaTime;
            if (bulletTime > 0) return;
            bulletTime = shootingCooldown;

            int spawnPointIndex = Random.Range(0, spawnPoint.Length);

            Instantiate(kamikazeEnemy, spawnPoint[spawnPointIndex].position, Quaternion.identity);
        }
    }
}
