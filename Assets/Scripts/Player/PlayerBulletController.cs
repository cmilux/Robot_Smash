using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
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
    [SerializeField] TrailRenderer _trailRenderer;
    bool _shotActive;   // client side: true between NotifySpawn and NotifyReturn

    private void Awake()
    {
        if(_trailRenderer == null) _trailRenderer = GetComponent<TrailRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody>();
        if (!IsServer) StartCoroutine(HideUntilShot());
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
            ObjectPoolManager.instance.ReturnPlayerBullet(this);
        }
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void ResetTrail()
    {
        if(_trailRenderer == null) return;

        _trailRenderer.emitting = false;
        _trailRenderer.Clear();
    }

    [ClientRpc]
    public void NotifySpawnClientRpc(Vector3 pos, Quaternion rot)
    {
        if (IsServer) return;

        ResetTrail();

        //move bullet directly to its real spawn point
        transform.SetPositionAndRotation(pos, rot);
        
        gameObject.SetActive(true);

        //start recording the trail from the correct position
        _trailRenderer.emitting = true;

        _shotActive = true;
    }

    private IEnumerator HideUntilShot()
    {
        yield return null;              // let every behaviour finish spawning
        if (!_shotActive) gameObject.SetActive(false);
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

        _trailRenderer.emitting = false;
        _trailRenderer.Clear();

        gameObject.SetActive(false);
        _shotActive = false;
    }
}