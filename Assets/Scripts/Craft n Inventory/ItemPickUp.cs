using Unity.Netcode;
using UnityEngine;

// This script is on the items on the ground so players can pick them up
public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;

    public NetworkVariable<int> quantity = new NetworkVariable<int>(1);

    public NetworkVariable<int> durability = new NetworkVariable<int>(-1);

    public void Initialize(int newQuantity, int newDurability)
    {
        if (!IsServer) return;
        quantity.Value = newQuantity;
        durability.Value = newDurability;  
    }
    [Rpc(SendTo.Server)]
    public void SetQuantityServerRpc(int newQuantity)// el cliente pide reducir la cantidad cuando el inventario estaba casi lleno
    {
        quantity.Value = newQuantity;   
    }
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