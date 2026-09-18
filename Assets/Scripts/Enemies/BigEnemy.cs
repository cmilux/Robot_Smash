using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Unity.Profiling;
using UnityEngine.Profiling;
using Unity.Netcode;

public class BigEnemy : Enemy
{
    [Header("Spawn settings")]
    float maxEnemies = 3;
    int currentEnemies;
    public GameObject kamikazeEnemy;
    public GameObject bulletObj;
    public Transform[] spawnBulletsPoint;
    float spawnRadius = 5f;
    float destroyTimer = 5f;

    [Header("Cooldown time")]
    public float spawnCooldown = 3f;
    public float spawnTime;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;
        if (isDead.Value) return;      //dead enemies don't act

        UpdateTarget();

        DetectPlayer();                 //checks distance to target and sets _playerDetected accordingly

        if (!_playerDetected)
        {
            HandlePatrolState();        //player is out of range — keep wandering patrol points
        }
        else
        {
            MoveTowardTarget();         //player is within detectionRadius — chase them directly (stopDistance can be ~0 so it walks into contact range for the explosion collision)
            SpawnKamikaze();
            Shoot();
        }

        //Checks how many spawned enemies are on scene || chequea cuantos enemigos hay en escena
        currentEnemies = GameObject.FindGameObjectsWithTag("SpawnedEnemies").Length;
    }

    void SpawnKamikaze()
    {
        if (currentEnemies >= maxEnemies) return;

        //Cooldown to spawn enemies
        spawnTime -= Time.deltaTime;
        if (spawnTime > 0) return;
        spawnTime = spawnCooldown;

        //calculates a radius from enemy position || calcula un radio basado en la posicion del enemigo
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = transform.position.y;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
        {
            spawnPos = hit.position;

            //Pool a kamikaze in the big enemy radius || Pool kamikazes en un radio del enemigo
            KamikazeEnemy kam = ObjectPoolManager.instance.GetKamikaze();
            kam.transform.position = spawnPos;
            kam.gameObject.SetActive(true);

            kam.Initialize();
        }
    }

    void Shoot()
    {
        if (currentEnemies < maxEnemies) return;

        agent.isStopped = false;

        //Cooldown to spawn bullets
        spawnTime -= Time.deltaTime;
        if (spawnTime > 0) return;
        spawnTime = spawnCooldown;

        //Pool a bullet in every spawn point
        for (int i = 0; i < spawnBulletsPoint.Length; i++)
        {
            Transform spawnPointIndex = spawnBulletsPoint[i];

            //Pool bullets
            TurretBullet bullet = ObjectPoolManager.instance.GetEnemyBullet();
            bullet.transform.position = spawnPointIndex.position;
            bullet.transform.rotation = spawnPointIndex.rotation;

            //Get the bullet rigidbody
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            //Pool bullets forward
            Vector3 dir = spawnPointIndex.forward;
            //Apply force to the bullets direction || aplica fuerza a la direccion de disparo de la bala
            rb.linearVelocity = dir * 15f;

            bullet.gameObject.SetActive(true);

            ObjectPoolManager.instance.ReturnEnemyBulletAfterDelay(bullet, destroyTimer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        //create a gizmos to show enemy radius || crea una esfera visual para visualizar el radio del enemigo
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}