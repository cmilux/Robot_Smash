using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class BigEnemy : Enemy
{
    [Header("Nav Mesh")]
    NavMeshAgent agent;
    public Transform player;

    [Header("Spawn settings")]
    float maxEnemies = 3;
    int currentEnemies;
    public GameObject kamikazeEnemy;
    public GameObject bulletObj;
    //public Transform[] spawnKamikazePoint;  //Right now enemies are being spawned on a radius instead of spawn points
    public Transform[] spawnBulletsPoint;
    float spawnRadius = 5f;
    
    [Header("Cooldown time")]
    public float spawnCooldown = 3f;
    public float spawnTime;

    private void Start()
    {
        //Gets the nav mesh
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Sets nav mesh destination to player
        agent.SetDestination(player.position);

        //Checks how many spawned enemies are on scene and call method
        currentEnemies = GameObject.FindGameObjectsWithTag("SpawnedEnemies").Length;
        SpawnKamikaze();

        //Shoot bullets
        Shoot();
    }

    void SpawnKamikaze()
    {
        if (currentEnemies < maxEnemies)
        {
            //Cooldown to spawn enemies
            spawnTime -= Time.deltaTime;
            if (spawnTime > 0) return;
            spawnTime = spawnCooldown;

            /*This causes an issue that spawns some of them right in front of the player
            //Save a random position from the array of points
            //int spawnPointIndex = Random.Range(0, spawnKamikazePoint.Length);
            //
            ////Spawn enemies at the random position
            //Instantiate(kamikazeEnemy, spawnKamikazePoint[spawnPointIndex].position, Quaternion.identity);
            */

            //Spawn kamikaze in the big enemy radius
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = transform.position.y;

            /*
            //Leave this section commented in case enemies start spawning in or under floor instead of above
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0f;
            Vector3 spawnPos = transform.position + randomPos;
            */

            Instantiate(kamikazeEnemy, spawnPos, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    void Shoot()
    {
        //If on scene they are the max amount of enemies
        if (currentEnemies == maxEnemies)
        {
            //Cooldown to spawn bullets
            spawnTime -= Time.deltaTime;
            if (spawnTime > 0) return;
            spawnTime = spawnCooldown;

            //Instantiate a bullet in every spawn point
            for (int i = 0; i < spawnBulletsPoint.Length; i++)
            {
                Transform spawnPointIndex = spawnBulletsPoint[i];

                //Spawn bullets
                GameObject bullet = Instantiate(
                    bulletObj,
                    spawnPointIndex.position,
                    spawnPointIndex.rotation);

                //Get the bullet rigidbody
                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                /*
                //Spawn in player direction
                //Vector3 dir = (player.position - spawnPointIndex.position).normalized;
                */

                //Spawn bullets forward
                Vector3 dir = spawnPointIndex.forward;
                //Apply force to the bullets
                rb.AddForce(dir * 15f, ForceMode.Impulse);

            }
        }
    }
}