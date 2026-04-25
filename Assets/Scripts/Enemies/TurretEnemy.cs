using UnityEngine;
using UnityEngine.AI;

public class TurretEnemy : Enemy
{
    NavMeshAgent agent;

    public GameObject bulletObj;
    public Transform player;
    public Transform spawnPoint;
    public float shootingCooldown = 3f;
    public float shootingSpeed = 40f;
    public float destroyTimer = 5;
    public float bulletTime;

    private void Start()
    {
        //Get the nav mesh
        agent = GetComponent<NavMeshAgent>();

        //Find player
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        agent.SetDestination(player.position);          //Set enemy destination to player to follow

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
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        Vector3 dir = (player.position - spawnPoint.transform.position).normalized;
        rb.AddForce(dir * shootingSpeed, ForceMode.Impulse);

        Destroy(bullet, destroyTimer);
    }
}
