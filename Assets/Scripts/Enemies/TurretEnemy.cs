using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class TurretEnemy : Enemy
{
    [Header("Bullet attack")]
    public GameObject bulletObj;
    public Transform[] spawnPoints;

    [Header("Cooldown")]
    public float shootingCooldown = 3f;
    public float shootingSpeed = 40f;
    public float destroyTimer = 5;
    public float bulletTime;

    protected override void Start()
    {
        //Get the nav mesh from enemy class
        base.Start();
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;
        if (isDead.Value) return;      //dead enemies don't act

        UpdateTarget();
        if (target == null) return;

        DetectPlayer();                 //checks distance to target and sets _playerDetected accordingly

        if (!_playerDetected)
        {
            HandlePatrolState();        //player is out of range — keep wandering patrol points
        }
        else
        {
            MoveTowardTarget();         //player is within detectionRadius — chase them directly (stopDistance can be ~0 so it walks into contact range for the explosion collision)
            ShootPlayer();
        }
    }

    void ShootPlayer()
    {
        //Bullet cooldown
        bulletTime -= Time.deltaTime;
        if (bulletTime > 0) return;
        bulletTime = shootingCooldown;

        RotateTowardsTarget();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPointsIndex = spawnPoints[i];

            //Creates a bullet to spawn || spawnea una bala
            GameObject bullet = Instantiate(
                bulletObj,
                spawnPointsIndex.position,
                spawnPointsIndex.rotation
            );
            NetworkObject netObj = bullet.GetComponent<NetworkObject>();
            netObj.Spawn();

            //Adds force and direction to the bullet to shoot player || Agrega fuerza y direccion a la bala del enemigo para atacar al player
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            //Vector3 dir = (target.position - spawnPointsIndex.position).normalized;
            Vector3 dir = spawnPointsIndex.forward;
            rb.AddForce(dir * shootingSpeed, ForceMode.Impulse);

            Destroy(bullet, destroyTimer);
        }
    }
}
