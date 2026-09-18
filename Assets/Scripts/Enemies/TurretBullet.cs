using Unity.Netcode;
using UnityEngine;

public class TurretBullet : NetworkBehaviour
{
    public int damage = 1;

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
    public void NotifyReturnClientRpc()
    {
        if (IsServer) return;
        gameObject.SetActive(false);
    }
}
