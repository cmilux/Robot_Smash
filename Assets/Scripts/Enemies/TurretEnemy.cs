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
            animator.SetBool("IsStopped", false);
            animator.SetBool("IsAttacking", true);

            MoveTowardTarget();         //player is within detectionRadius — chase them directly (stopDistance can be ~0 so it walks into contact range for the explosion collision)
            ShootPlayer();
        }

        if (_enemyWaiting)
        {
            animator.SetBool("IsStopped", true);
            animator.SetBool("IsAttacking", false);
        }
        else if(!_enemyWaiting && !_playerDetected)
        {
            animator.SetBool("IsStopped", false);
            animator.SetBool("IsAttacking", false);
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

            TurretBullet bullet = ObjectPoolManager.instance.GetEnemyBullet();
            bullet.transform.position = spawnPointsIndex.transform.position;
            bullet.transform.rotation = spawnPointsIndex.transform.rotation;

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            Vector3 dir = (target.position - spawnPointsIndex.transform.position).normalized;
            rb.linearVelocity = dir * shootingSpeed;

            bullet.gameObject.SetActive(true);

            ObjectPoolManager.instance.ReturnEnemyBulletAfterDelay(bullet, destroyTimer);
        }
            
    }
}
