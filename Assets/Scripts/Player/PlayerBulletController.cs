using Unity.Netcode;
using UnityEngine;

public class PlayerBulletController : NetworkBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    private Rigidbody rb;

    // Saves the ID of the player who shot this bullet
    ulong shooterClientId;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        shooterClientId = OwnerClientId;

        // Only the server moves the bullet 
        if (IsServer)
        {
            // Give the bullet a push forward
            rb.linearVelocity = transform.forward * speed;

            // Delete the bullet after 2 seconds
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

            // Delete the bullet after hitting the enemy
            DestroyBullet();
        }
    }

    // delete the bullet from the game for everyone
    void DestroyBullet()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}