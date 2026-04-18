using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo DB", menuName = "Inventory/Data Base")]
public class ItemDataBase : ScriptableObject
{
    // Lista de items asignados desde el Inspector
    public List<ItemData> items = new List<ItemData>();

    // Diccionario de items (oculto en el inspector)
    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    public void InitializeDataBase()
    {
        itemDictionary.Clear();

        foreach (ItemData item in items)
        {
            if (item == null) continue; // Seguridad extra: por si dejaste un hueco vacío en la lista

            // CORRECCIÓN 1: Usamos "!" para verificar si el diccionario NO contiene el ID
            if (!itemDictionary.ContainsKey(item.id.ToString()))
            {
                itemDictionary.Add(item.id.ToString(), item);
            }
            else
            {
                Debug.LogWarning($"¡Cuidado! El ID {item.id} está repetido en la base de datos.");
            }
        }

        Debug.Log($"Base de datos inicializada con: {itemDictionary.Count} items");
    }

    public ItemData SearchItem(string id)
    {
        // CORRECCIÓN 2: Si el diccionario está vacío pero la LISTA tiene objetos, inicializamos.
        if (itemDictionary.Count == 0 && items.Count > 0)
        {
            InitializeDataBase();
        }

        // Buscamos en el diccionario (id ya es un string, no hace falta usar .ToString() aquí)
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
