using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TurretBullet : NetworkBehaviour
{
    public int damage = 1;
    [SerializeField] TrailRenderer _trailRenderer;
    [SerializeField] float _lifeTime = 5f;

    private void Awake()
    {
        if(_trailRenderer == null) _trailRenderer = GetComponent<TrailRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsServer) gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!IsServer) return;
        CancelInvoke(nameof(ReturnToPoolTimeout));
        Invoke(nameof(ReturnToPoolTimeout), _lifeTime);
    }

    void ReturnToPoolTimeout()
    {
        if (!IsServer || !gameObject.activeSelf) return;
        NotifyReturnClientRpc();
        ObjectPoolManager.instance.ReturnEnemyBullet(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            //Gets the playerHealth script from player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                //Takes damage from player
                playerHealth.LoseHealthServerRpc(damage);
            }

            NotifyReturnClientRpc();
            ObjectPoolManager.instance.ReturnEnemyBullet(this);
        }
    }

    public void ResetTrail()
    {
        if (_trailRenderer == null) return;

        _trailRenderer.emitting = false;
        _trailRenderer.Clear();
    }

    [ClientRpc]
    public void NotifySpawnClientRpc(Vector3 pos, Quaternion rot)
    {
        if (IsServer) return;

        _trailRenderer.emitting = false;

        transform.SetPositionAndRotation(pos, rot);

        _trailRenderer.Clear();

        gameObject.SetActive(true);
        
        _trailRenderer.emitting = true;
    }

    //let the clients know the game obj is off
    [ClientRpc]
    public void NotifyReturnClientRpc()
    {
        if (IsServer) return;
        gameObject.SetActive(false);
    }
}
