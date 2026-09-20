using Unity.Netcode;
using UnityEngine;

public class PlayerBulletController : NetworkBehaviour
{
    public float speed;
    [SerializeField] int _damage;
    [SerializeField] float _destroyTimer;
    //add the car velocity
    public Vector3 extraVelocity;
    private Rigidbody _rb;
    public ulong shooterClientId;       // Saves the ID of the player who shot this bullet

    public void SetDamage(int damage)
    {
        _damage = damage;
    }
    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (IsServer)
        {
            CancelInvoke(nameof(ReturnToPoolTimeout));
            Invoke(nameof(ReturnToPoolTimeout), _destroyTimer);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) return; 

        // Get enemy script
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Apply damage to enemy
            enemy.TakeDamageServerRpc(_damage, shooterClientId);

            //if bullet hits enemy, follow player
            enemy.HandleFollowState();

            CancelInvoke(nameof(ReturnToPoolTimeout));

            NotifyReturnClientRpc();

            // Delete the bullet after hitting the enemy
            ObjectPoolManager.instance.ReturnPlayerBulletAfterDelay(this, 2f);
        }
    }

    private void ReturnToPoolTimeout()
    {
        if (!IsServer) return;
        if(!gameObject.activeSelf) return;

        NotifyReturnClientRpc();
        ObjectPoolManager.instance.ReturnPlayerBullet(this);
    }

    //let the clients know the game obj is off
    [ClientRpc]
    public void NotifyReturnClientRpc()
    {
        if(IsServer) return;
        gameObject.SetActive(false);
    }
}