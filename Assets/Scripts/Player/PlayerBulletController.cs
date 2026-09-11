using Unity.Netcode;
using UnityEngine;

public class PlayerBulletController : NetworkBehaviour
{
   [HideInInspector] public float speed; //set by PlayerAttackDistance based on the equipped weapon
   [HideInInspector]public int damage; //set by PlayerAttackDistance when the bullet is created
    //add the car velocity
    public Vector3 extraVelocity;

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
            Vector3 bulletDirection = transform.forward;
            // Only keep the part of the shooter velocity that point the same way as the bullet
            float forwardBoost = Vector3.Dot(extraVelocity, bulletDirection);

            // Ignore it if the shooter was moving backward to the shot
            forwardBoost = Mathf.Max(forwardBoost, 0f);

            // Give the bullet a push forward boosted by the car speed
            rb.linearVelocity = bulletDirection * (speed + forwardBoost);

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

            //if bullet hits enemy, follow player
            enemy.HandleFollowState();

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