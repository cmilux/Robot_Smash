using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TurretBullet : NetworkBehaviour
{
    public int damage = 1;
    [SerializeField] float _lifeTime = 5f;

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

    [ClientRpc]
    public void NotifySpawnClientRpc(Vector3 pos, Quaternion rot)
    {
        if (IsServer) return;

        transform.SetPositionAndRotation(pos, rot);

        gameObject.SetActive(true);
    }

    //let the clients know the game obj is off
    [ClientRpc]
    public void NotifyReturnClientRpc()
    {
        if (IsServer) return;
        gameObject.SetActive(false);
    }
}
