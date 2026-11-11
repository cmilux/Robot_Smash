using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using System.Collections;

public class BigEnemy : Enemy
{
    [Header("Spawn settings")]
    float maxEnemies = 3;
    int currentEnemies;
    public GameObject kamikazeEnemy;
    public GameObject bulletObj;
    public Transform[] spawnBulletsPoint;
    float spawnRadius = 5f;

    [Header("Cooldown time")]
    public float spawnCooldown = 3f;
    public float spawnTime;
    float _nextCountTime;

    [Header("Spawn animation")]
    [SerializeField] float _spawnAnimDelay = 0.6f;  //seconds from animation start to the throw moment || segundos desde que arranca la animacion hasta el lanzamiento
    [SerializeField] float _spawnAnimDuration = 1.2f;   //how long IsSpawning stays on (clip length) || cuanto queda prendido IsSpawning (largo del clip)   
    bool _spawning;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;
        if (isDead.Value) return;      //dead enemies don't act

        UpdateTarget();
        UpdateAnimator();

        DetectPlayer();                 //checks distance to target and sets _playerDetected accordingly

        if (!_playerDetected)
        {
            HandlePatrolState();        //player is out of range — keep wandering patrol points

            //not attacking anymore, clear attack animation so it doesnt walk w the shooting animation
            animator.SetBool("IsSpawning", false);
            animator.SetBool("IsShooting", false);
        }
        else
        {
            MoveTowardTarget();         //player is within detectionRadius — chase them directly (stopDistance can be ~0 so it walks into contact range for the explosion collision)
            SpawnKamikaze();
            Shoot();

            //face the player every frame while attacking
            RotateTowardsTarget();
        }

        //count spawned kamikazes twice per second instead of every frame
        if (Time.time >= _nextCountTime)
        {
            _nextCountTime = Time.time + 0.5f;
            currentEnemies = GameObject.FindGameObjectsWithTag("SpawnedEnemies").Length;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        _spawning = false;
        spawnTime = 0f;
    }

    void SpawnKamikaze()
    {
        if (currentEnemies >= maxEnemies)
        {
            animator.SetBool("DidSpawn3", true);
            return;
        }

        agent.isStopped = true;

        if (_spawning) return;  //already throwing one || ya esta lanzando uno

        //Cooldown to spawn enemies
        spawnTime -= Time.deltaTime;
        if (spawnTime > 0) return;
        spawnTime = spawnCooldown;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        _spawning = true;



        animator.SetBool("IsSpawning", true);
        animator.SetBool("IsShooting", false);

        yield return new WaitForSeconds(_spawnAnimDelay);

        yield return new WaitForSeconds(Mathf.Max(0f, _spawnAnimDuration - _spawnAnimDelay));

        animator.SetBool("IsSpawning", false);
        _spawning = false;
    }
    // Called by an animation event at the exact frame the kamikaze is thrown
    public void OnSpawnThrow()
    {
        if (!IsServer) return;                          //only the server spawns enemies
        if (isDead.Value || !_playerDetected) return;   //dont throw if dead or the player left

        SpawnOneKamikaze();
    }

    void SpawnOneKamikaze()
    {
        //calculates a radius from enemy position || calcula un radio basado en la posicion del enemigo
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = transform.position.y;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
        {
            spawnPos = hit.position;

            //Pool a kamikaze in the big enemy radius || Pool kamikazes en un radio del enemigo
            KamikazeEnemy kam = ObjectPoolManager.instance.GetKamikaze();

            NetworkTransform netTransform = kam.GetComponent<NetworkTransform>();
            if (netTransform != null)
            {
                netTransform.Teleport(spawnPos, kam.transform.rotation, kam.transform.localScale);
            }
            else
            {
                kam.transform.position = spawnPos;
            }

            kam.gameObject.SetActive(true);

            kam.Initialize();

            kam.SyncActiveState(true);
        }
    }

    void Shoot()
    {
        if (currentEnemies < maxEnemies)
        {
            animator.SetBool("DidSpawn3", false);
            return;
        }

        agent.isStopped = true;

        //Cooldown to spawn bullets
        spawnTime -= Time.deltaTime;
        if (spawnTime > 0) return;
        spawnTime = spawnCooldown;

        //Pool a bullet in every spawn point
        for (int i = 0; i < spawnBulletsPoint.Length; i++)
        {
            Transform spawnPointIndex = spawnBulletsPoint[i];

            animator.SetBool("IsSpawning", false);
            animator.SetBool("IsShooting", true);

            //Pool bullets
            TurretBullet bullet = ObjectPoolManager.instance.GetEnemyBullet();
            bullet.gameObject.SetActive(true);

            //bullet.transform.position = spawnPointIndex.position;
            //bullet.transform.rotation = spawnPointIndex.rotation;

            var nt = bullet.GetComponent<NetworkTransform>();
            if (nt != null)
            {
                nt.Teleport(
                    spawnPointIndex.position,
                    spawnPointIndex.rotation,
                    bullet.transform.localScale
                    );
            }
            else
                bullet.transform.SetPositionAndRotation(
                    spawnPointIndex.position,
                    spawnPointIndex.rotation
                    );

            //Get the bullet rigidbody
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.linearVelocity = (target.position - spawnPointIndex.position).normalized * 15f;

            bullet.NotifySpawnClientRpc(spawnPointIndex.position, spawnPointIndex.rotation);

            //ObjectPoolManager.instance.ReturnEnemyBulletAfterDelay(bullet, destroyTimer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        //create a gizmos to show enemy radius || crea una esfera visual para visualizar el radio del enemigo
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}