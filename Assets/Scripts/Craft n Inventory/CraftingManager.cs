using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class CraftingManager : MonoBehaviour
{
    [Header("Crafting Slots")]
    public List<Slot> craftingSlots;
    public Slot resultSlot;

    [Header("Button")]
    public Button craftButton;

    private RecipeData currentRecipe;
    private void Start()
    {
        //Connect the button by code
        craftButton.onClick.AddListener(OnCraftButton);
    }
    private void Update()
    {
        //Check for a valid recipe
        CheckRecipe();
    }
    private void CheckRecipe()
    {
        //Clear the current recipe if all slots are empty
        if (AllSlotsEmpty())
        {
            currentRecipe = null;
            return;
        }
        // Find a recipe that matches the items in the slots
        currentRecipe = FindMatchingRecipe();

        if (currentRecipe != null)
        {
            Debug.Log($"Recipe found: {currentRecipe.result.nombre}");
        }
    }
    public void OnCraftButton()
    {
        if (currentRecipe == null) return;

        //Show the crafted item in the result slot
        resultSlot.SetItem(currentRecipe.result, currentRecipe.resultQuantity);

        //Remove the required ingredients from the crafting slots
        ConsumeIngredients(currentRecipe);
    }
    private bool AllSlotsEmpty()
    {
        foreach (Slot slot in craftingSlots)
        {
            // Return false if one slot has an item
            if (slot.itemData != null) return false;
        }
        // All slots are empty
        return true;
    }
    private RecipeData FindMatchingRecipe()
    {
        // Store each item and total quantity from the crafting slots
        Dictionary<ItemData, int> craftingItems = new Dictionary<ItemData, int>();

        foreach (Slot slot in craftingSlots)
        {
            // Ignore empty slots
            if (slot.itemData == null) continue;

            // Add the quantity if the item already exist
            if (craftingItems.ContainsKey(slot.itemData))
            {
                craftingItems[slot.itemData] += slot.quantity;
            }
            else
            {
                // Add the item for the first time
                craftingItems[slot.itemData] = slot.quantity;
            }
        }
        // Check all recipes in the database
        foreach (RecipeData recipe in GameManager.instance.itemDataBase.recipes)
        {
            // Return the recipe if the ingredients match
            if (RecipeMatches(recipe, craftingItems))
            {
                return recipe;
            }
        }
        // No matching recipe was found
        return null;
    }

    private bool RecipeMatches(RecipeData recipe, Dictionary<ItemData, int> craftingItems)
    {
        // Check that both have the same number of different ingredients
        if (recipe.ingredients.Count != craftingItems.Count)
            return false;

        foreach (RecipeIngredient ingredient in recipe.ingredients)
        {
            // Check if the required item exist in the crafting slots
            if (!craftingItems.ContainsKey(ingredient.itemData))
                return false;

            // Check if there are enough items
            if (craftingItems[ingredient.itemData] < ingredient.quantity)
                return false;
        }
        // All ingredients match the recipe
        return true;
    }
    // Remove the ingredients needed for the recipe
    public void ConsumeIngredients(RecipeData recipe)
    {
        foreach (RecipeIngredient ingredient in recipe.ingredients)
        {
            // Store how many items still need to be remove
            int quantityToConsume = ingredient.quantity;

            foreach (Slot slot in craftingSlots)
            {
                // Check if the slot contains the required item
                if (slot.itemData == ingredient.itemData)
                {
                    if (slot.quantity <= quantityToConsume)
                    {
                        // Remove all
                        quantityToConsume -= slot.quantity;
                        slot.ClearItem();
                    }
                    else
                    {
                        // Remove only the required amount
                        slot.SetItem(
                            slot.itemData,
                            slot.quantity - quantityToConsume
                        );

                        quantityToConsume = 0;
                    }
                    // Stop when all required items have been remove
                    if (quantityToConsume <= 0)
                        break;
                }
            }
        }
    }
}