using Unity.Netcode;
using UnityEngine;

public class PlayerBulletController : NetworkBehaviour
{
    public float speed;
    [SerializeField] int damage = 10;
    //add the car velocity
    public Vector3 extraVelocity;
    private Rigidbody rb;
    public ulong shooterClientId;       // Saves the ID of the player who shot this bullet

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) return; 

        // Get enemy script
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Apply damage to enemy
            enemy.TakeDamageServerRpc(damage, shooterClientId);

            //if bullet hits enemy, follow player
            enemy.HandleFollowState();

            NotifyReturnClientRpc();

            // Delete the bullet after hitting the enemy
            ObjectPoolManager.instance.ReturnPlayerBullet(this);
        }
    }

    [ClientRpc]
    public void NotifyReturnClientRpc()
    {
        if(IsServer) return;
        gameObject.SetActive(false);
    }
}