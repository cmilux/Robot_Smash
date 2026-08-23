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
    {   // Connect the button by code
        craftButton.onClick.AddListener(OnCraftButton);
    }
    private void Update()
    {
        CheckRecipe();
    }
    private void CheckRecipe()
    {
        if (AllSlotsEmpty())
        {
            currentRecipe = null;
            return;
        }

        currentRecipe = FindMatchingRecipe();
    }
    public void OnCraftButton()
    {
        if (currentRecipe == null) return;

        // Show the result ONLY when the button is pressed
        resultSlot.SetItem(currentRecipe.result, currentRecipe.resultQuantity);

        // Consume ingredients immediately
        ConsumeIngredients(currentRecipe);
    }
    private bool AllSlotsEmpty()
    {
        foreach (Slot slot in craftingSlots)
        {
            if (slot.itemData != null) return false;
        }
        return true;
    }
    private RecipeData FindMatchingRecipe()
    {
        //Build a dictionary of what it is in the crafting slots
        Dictionary<ItemData, int> craftingItems = new Dictionary<ItemData, int>();

        foreach (Slot slot in craftingSlots)
        {
            if (slot.itemData == null) continue;

            if (craftingItems.ContainsKey(slot.itemData))
            {
                craftingItems[slot.itemData] += slot.quantity;
            }
            else { craftingItems[slot.itemData] = slot.quantity; }
        }

        //Check every recipe in the database
        foreach (RecipeData recipe in GameManager.instance.itemDataBase.recipes)
        {
            if (RecipeMatches(recipe, craftingItems))
            {
                return recipe;
            }
        }
        return null;
    }
    private bool RecipeMatches(RecipeData recipe, Dictionary<ItemData, int> craftingItems)
    {
        //Must have the same number of different ingredients
        if (recipe.ingredients.Count != craftingItems.Count) return false;

        foreach (RecipeIngredient ingredient in recipe.ingredients)
        {
            //Check if the item exists in the crafting slot wiht enough quantity
            if (!craftingItems.ContainsKey(ingredient.itemData)) return false;

            if (craftingItems[ingredient.itemData] < ingredient.quantity) return false;
        }
        return true;
    }
    //Call when the player drag the result out of the result slot 
    public void ConsumeIngredients(RecipeData recipe)
    {
        foreach (RecipeIngredient ingredient in recipe.ingredients)
        {
            foreach (Slot slot in craftingSlots)
            {
                if (slot.itemData == ingredient.itemData)
                {
                    int remaining = slot.quantity - ingredient.quantity;

                    if (remaining <= 0)
                    {
                        slot.ClearItem();
                    }
                    else
                    {
                        slot.SetItem(slot.itemData, remaining);
                    }
                    break;
                }
            }
        }
    }
}
