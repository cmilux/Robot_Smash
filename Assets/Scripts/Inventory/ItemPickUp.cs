using Unity.Netcode;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;
    public int quantity = 1;

   public void Pickup()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
        else
        {
            PickupServerRpc();
        }
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickupServerRpc()
    {
        if (!IsServer) return;
        NetworkObject.Despawn(true);
    }
}