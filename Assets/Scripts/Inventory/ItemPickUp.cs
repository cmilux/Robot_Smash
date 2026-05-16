using Unity.Netcode;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;

    public int quantity = 1;

    public void Pickup()
    {
        Debug.Log("PICKUP LLAMADO");
        if (IsServer)
        {
            Debug.Log("Soy server");
            NetworkObject.Despawn(false);
        }
        else
        {
            Debug.Log("SOy cliente");
            PickupServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void PickupServerRpc()
    {
        NetworkObject.Despawn(false);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);

        base.OnNetworkDespawn();
    }
}