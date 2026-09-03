using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo DB", menuName = "Inventory/Data Base")]
public class ItemDataBase : ScriptableObject
{
    // List of items assigned from the inspector
    public List<ItemData> items = new List<ItemData>();
    public List<RecipeData> recipes = new List<RecipeData>();
    // Item dictionary (hidden in the inspector)
    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    public void InitializeDataBase()
    {
        itemDictionary.Clear();

        foreach (ItemData item in items)
        {
            // Extra safety: in case there is an empty slot in the list
            if (item == null) continue;

            // Check if the dictionary does not already contain the ID
            if (!itemDictionary.ContainsKey(item.id.ToString()))
            {
                itemDictionary.Add(item.id.ToString(), item);
            }
            else
            {
                Debug.LogWarning($"El ID {item.id} esta repetido en la base de datos.");
            }
        }

        Debug.Log($"Base de datos inicializada con: {itemDictionary.Count} items");
    }

    public ItemData SearchItem(string id)
    {
        // If the dictionary is empty but the list has objects we initialize
        if (itemDictionary.Count == 0 && items.Count > 0)
        {
            InitializeDataBase();
        }

        // Search the dictionary for the ID
        if (itemDictionary.TryGetValue(id, out ItemData itemData))
        {
            return itemData;
        }
        else
        {
            return null;
        }
    }
}