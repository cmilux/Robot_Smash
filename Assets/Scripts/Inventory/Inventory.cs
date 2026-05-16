using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : NetworkBehaviour
{
    public Transform slotsContainer;

    public Transform hotBarSlotsContainer;

    [Header("Configuración para Recoger")]
    public float pickupRadius = 4f; 
    public LayerMask itemLayer;   

    private List<Slot> slots = new List<Slot>();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        GameObject inventoryUI = GameObject.Find("InventoryUI");

        slotsContainer = inventoryUI.transform.Find("Inventory");

        hotBarSlotsContainer = inventoryUI.transform.Find("HotBar");

        slots.AddRange( slotsContainer.GetComponentsInChildren<Slot>(true));

        slots.AddRange(hotBarSlotsContainer.GetComponentsInChildren<Slot>(true));
    }
    private void OnInteract(InputValue value)
    {
        if (!IsOwner) return;
        if (value.isPressed)//Key E 
        { 
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius, itemLayer);

            foreach (Collider col in colliders)
            {   
                ItemPickup itemOnGround = col.GetComponent<ItemPickup>();

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

                    break;
                }
            }
        }
    }

    public int AddItem(ItemData itemData, int quantity)
    {
        int quantityToSave = quantity;

        //Primero buscamos los slots que ya tengan ese item y no esten llenos(para completar el stock)
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

                if (quantityToSave <= 0)
                {
                    return 0;
                }
            }
        }
        //2_ Buscar slots vacios
        if (quantityToSave > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].itemData == null)
                {
                    int quantityToStore = Mathf.Min(itemData.maxStock, quantityToSave);

                    slots[i].SetItem(itemData, quantityToStore);
                    quantityToSave -= quantityToStore;

                    if (quantityToSave <= 0)
                    {
                        return 0;
                    }
                }
            }
        }
        //3_ Quedaron items por guardar 
        return quantityToSave;
    }
}