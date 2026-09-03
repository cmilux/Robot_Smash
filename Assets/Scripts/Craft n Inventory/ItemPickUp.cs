using Unity.Netcode;
using UnityEngine;

// This script is on the items on the ground so players can pick them up
public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;

    public int quantity = 1;

    // This is called when a player picks up the item
    public void Pickup()
    {
        if (IsServer)
        {
            // The server can delete the item from the network directly
            NetworkObject.Despawn(false);
        }
        else
        {
            // Clients cannot delete items so they ask the server 
            PickupServerRpc();
        }
    }

    // This code runs only on the Server when a client calls it
    [Rpc(SendTo.Server)]
    private void PickupServerRpc()
    {
        // The server deletes the item from the network
        NetworkObject.Despawn(false);
    }

    // Automatically called on all players when the item is removed from the network
    public override void OnNetworkDespawn()
    {
        // Hide the item in the game world
        gameObject.SetActive(false);

        base.OnNetworkDespawn();
    }
}