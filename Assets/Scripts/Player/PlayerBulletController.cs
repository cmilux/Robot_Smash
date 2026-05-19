using Unity.Netcode;
using UnityEngine;

public class PlayerBulletController : NetworkBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    private Rigidbody rb;
    ulong shooterClientId;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        shooterClientId = OwnerClientId;

        if (IsServer)
        {
            rb.linearVelocity = transform.forward * speed;

            Invoke(nameof(DestroyBullet), 2f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Get enemy script
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Apply damage to enemy
            enemy.TakeDamageServerRpc(damage, shooterClientId);

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