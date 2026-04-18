using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class BigEnemy : Enemy
{
    NavMeshAgent agent;
    public GameObject kamikazeEnemy;
    public Transform player;
    public Transform[] spawnPoint;
    public KamikazeEnemy scriptEnemy;
    
    public float shootingCooldown = 3f;
    public float bulletTime;

    float maxEnemies = 3;

    private void Start()
    {
        //Gets the nav mesh
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Sets nav mesh destination to player
        agent.SetDestination(player.position);

        ShootPlayer();
    }

    void ShootPlayer()
    {
        //Checks how many spawned enemies are on scene
        int currentEnemies = GameObject.FindGameObjectsWithTag("SpawnedEnemies").Length;

        if (currentEnemies < maxEnemies)
        {
            //Cooldown to spawn enemies
            bulletTime -= Time.deltaTime;
            if (bulletTime > 0) return;
            bulletTime = shootingCooldown;

            //Save a random position from the array of points
            int spawnPointIndex = Random.Range(0, spawnPoint.Length);

            //Spawn enemies at the random position
            Instantiate(kamikazeEnemy, spawnPoint[spawnPointIndex].position, Quaternion.identity);
        }
    }
}
