using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class TurretEnemy : Enemy
{
    public GameObject bulletObj;
    public Transform spawnPoint;
    public float shootingCooldown = 3f;
    public float shootingSpeed = 40f;
    public float destroyTimer = 5;
    public float bulletTime;

    protected override void Start()
    {
        //Get the nav mesh
        base.Start();
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;

        UpdateTarget();
        MoveTowardTarget();

        if (target == null) return;
        ShootPlayer();
    }

    void ShootPlayer()
    {
        //Bullet cooldown
        bulletTime -= Time.deltaTime;
        if (bulletTime > 0) return;
        bulletTime = shootingCooldown;

        //Creates a bullet to spawn to the player position
        GameObject bullet = Instantiate(bulletObj, spawnPoint.transform.position, spawnPoint.transform.rotation);
        NetworkObject netObj = bullet.GetComponent<NetworkObject>();
        netObj.Spawn();

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        Vector3 dir = (target.position - spawnPoint.transform.position).normalized;
        rb.AddForce(dir * shootingSpeed, ForceMode.Impulse);

        Destroy(bullet, destroyTimer);
    }
}
