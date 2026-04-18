using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Transform slotsContainer; //It can be for the player or another object that needs an inventory

    public Transform hotBarSlotsContainer;//Only for the player

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
}
