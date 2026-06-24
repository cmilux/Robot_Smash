using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Manages player inventory, hotbar slots, and network-synced item pickup mechanics
public class Inventory : NetworkBehaviour
{
    public Transform slotsContainer;

    public Transform hotBarSlotsContainer;

    // // A list to save all the inventory slots
    private List<Slot> slots = new List<Slot>();

    public override void OnNetworkSpawn()
    {
        // Only the local owner of this player object should search and bind the local UI
        if (!IsOwner) return;

        // Find the UI canvas in the scene at runtime
        GameObject inventoryUI = GameObject.Find("InventoryUI");

        slotsContainer = inventoryUI.transform.Find("Inventory");

        hotBarSlotsContainer = inventoryUI.transform.Find("HotBar");

        // Get all Slot components from both containers (including inactive ones)
        slots.AddRange(slotsContainer.GetComponentsInChildren<Slot>(true));

        slots.AddRange(hotBarSlotsContainer.GetComponentsInChildren<Slot>(true));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        ItemPickup itemOnGround = other.GetComponent<ItemPickup>();

        if (itemOnGround != null)
        {
            int itemsLeftOver = AddItem(itemOnGround.itemData, itemOnGround.quantity);

            if (itemsLeftOver == 0)
            {
                itemOnGround.Pickup();
            }
            else if (itemsLeftOver < itemOnGround.quantity)
            {
                itemOnGround.quantity = itemsLeftOver;
            }
        }
    }

    //logic to distribute items into existing stacks or empty slots, returning leftover amounts
    public int AddItem(ItemData itemData, int quantity)
    {
        int quantityToSave = quantity;

        //Primero buscamos los slots que ya tengan ese item y no esten llenos(para completar el stock)
        // Find slots with the same item to fill them up
        for (int i = 0; i < slots.Count; i++)
        {
            //1_si encontramos ese item en el slot y la cantidad que tiene es menor a la cantidad maxima del slot
            if (slots[i].itemData == itemData && slots[i].quantity < itemData.maxStock)
            {
                int availableSpace = itemData.maxStock - slots[i].quantity;

                // ej: Espacio disponible = 20 y queremos sumar 30 cubos, cantidad a almacenar es 20.
                int quantityToStore = Mathf.Min(availableSpace, quantityToSave); //quantityToStore = cantidadaalmacenar, quantytoTosave = cantidad a guardar 

                slots[i].SetItem(itemData, slots[i].quantity + quantityToStore);
                quantityToSave -= quantityToStore;

                // If we saved all items, stop here
                if (quantityToSave <= 0)
                {
                    return 0;
                }
            }
        }
        //2_ Buscar slots vacios
        // If we still have items, look for empty slots
        if (quantityToSave > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].itemData == null)
                {
                    int quantityToStore = Mathf.Min(itemData.maxStock, quantityToSave);

                    slots[i].SetItem(itemData, quantityToStore);
                    quantityToSave -= quantityToStore;

                    //If we saved all items, stop here
                    if (quantityToSave <= 0)
                    {
                        return 0;
                    }
                }
            }
        }
        //3_ Quedaron items por guardar 
        // Inventory is full, return the total amount of leftover items
        return quantityToSave;
    }
}