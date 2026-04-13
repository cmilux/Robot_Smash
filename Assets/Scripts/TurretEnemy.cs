using UnityEngine;
using UnityEngine.AI;

public class TurretEnemy : MonoBehaviour
{
    public GameObject bulletObj;
    public Transform player;
    public Transform spawnPoint;
    public float shootingCooldown = 3f;
    public float shootingSpeed = 40f;
    public float destroyTimer = 5;
    public float bulletTime;
    
    private void Start()
    {
        //bulletTime = shootingCooldown;    
    }
    
    private void Update()
    {
        ShootPlayer();
    }
    
    void ShootPlayer()
    {
        bulletTime -= Time.deltaTime;
        if(bulletTime > 0) return;
        bulletTime = shootingCooldown;
    
        GameObject bullet = Instantiate(bulletObj, spawnPoint.transform.position, spawnPoint.transform.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        Vector3 dir = (player.position - spawnPoint.transform.position).normalized;
        rb.AddForce(dir * shootingSpeed, ForceMode.Impulse);
    
        Destroy(bullet, destroyTimer);
    }
}
