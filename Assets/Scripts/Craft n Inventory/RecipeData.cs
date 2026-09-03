using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nueva Receta", menuName = "Inventory/Recipe")]
public class RecipeData : ScriptableObject
{
    public List<RecipeIngredient> ingredients;
    public ItemData result;
    public int resultQuantity = 1;
}

[System.Serializable]
public class RecipeIngredient
{
    public ItemData itemData;
    public int quantity;
}