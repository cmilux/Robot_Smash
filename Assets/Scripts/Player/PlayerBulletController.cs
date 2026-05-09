using Unity.Netcode;
using UnityEngine;

public class PlayerBulletController : NetworkBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    private Rigidbody rb;
   // PlayerLevelUI pj;

    void Start()
    {   

       // pj = GameObject.FindAnyObjectByType<PlayerLevelUI>();
    }
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (IsServer)
        {
            rb.linearVelocity = transform.forward * speed;

            Invoke(nameof(DestroyBullet), 2f);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        //Get enemy script
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null) 
        {
            enemy.TakeDamage(damage);           //Apply damage to enemy

           // if (enemy.isDead == true)
            
                //Add this amount of experience to player if enemy died
                //pj.AddExp(30);
               // Debug.Log("Adding EXP to: " + pj.gameObject.name);
            
            DestroyBullet();
        }
            
    }
    void DestroyBullet()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}
