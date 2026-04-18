using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public Transform slotsContainer; //It can be for the player or another object that needs an inventory

    public Transform hotBarSlotsContainer;//Only for the player
    public ItemData itemDev;

    private List<Slot> slots = new List<Slot>();

    private void Start()
    {
        slots.AddRange(slotsContainer.GetComponentsInChildren<Slot>());

        if(hotBarSlotsContainer != null)
        {
            slots.AddRange(hotBarSlotsContainer.GetComponentsInChildren<Slot>());
        }
        Debug.Log($"Inventario inicializado con: {slots.Count} slots");
    }
    private void OnInteract(InputValue value)
    {
        // Verificamos que la tecla se acaba de presionar
        if (value.isPressed)//Key E HOLD
        {
            Debug.Log("¡Se detectó el botón Interact!"); // Si esto no aparece en la consola, el script no está en el Player
            AddItem(itemDev, 1);
        }
    }

    public int AddItem(ItemData itemData, int quantity)
    {
        int quantityToSave = quantity; 
        
        //Primero buscamos los slots que ya tengan ese item y no esten llenos(para completar el stock)
        for (int i = 0; i < slots.Count; i++)
        {   
            //1_si encontramos ese item en el slot y la cantidad que tiene es menor a la cantidad maxima del slot
            if(slots[i].itemData == itemData && slots[i].quantity < itemData.maxStock)
            {
                int availableSpace = itemData.maxStock - slots[i].quantity;

                // ej: Espacio disponible = 20 y queremos sumar 30 cubos, cantidad a almacenar es 20.
                int quantityToStore = Mathf.Min(availableSpace, quantityToSave); //quantityToStore = cantidadaalmacenar, quantytoTosave = cantidad a guardar 

                slots[i].SetItem(itemData, slots[i].quantity + quantityToStore);
                quantityToSave -= quantityToStore;

                if(quantityToSave <= 0)
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
                if( slots[i].itemData == null)
                {
                    int quantityToStore = Mathf.Min(itemData.maxStock, quantityToSave);

                    slots[i].SetItem(itemData,quantityToStore);
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
