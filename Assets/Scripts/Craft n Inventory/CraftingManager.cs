using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
public class CraftingManager : MonoBehaviour
{
    [Header("Crafting Slots")]
    public List<Slot> craftingSlots;
    public Slot resultSlot;

    [Header("Button")]
    public Button craftButton;

    private RecipeData currentRecipe;
    private bool _resultAlreadyOwned;

    private float _nextCheckTime;
    private void Start()
    {
        //Connect the button by code
        craftButton.onClick.AddListener(OnCraftButton);
    }
    private void Update()
    {
        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + 0.2f;
        //Check for a valid recipe
        CheckRecipe();
    }
    private void CheckRecipe()
    {
        //Clear the current recipe if all slots are empty
        if (AllSlotsEmpty())
        {
            currentRecipe = null;
            _resultAlreadyOwned = false;
            UpdateCraftButton();
            return;
        }
        // Find a recipe that matches the items in the slots
        currentRecipe = FindMatchingRecipe();
        _resultAlreadyOwned = AlreadyOwnsResult(currentRecipe);
        UpdateCraftButton();
    }
    private void UpdateCraftButton()
    {
        if (craftButton == null) return;

        craftButton.interactable = (currentRecipe != null && !_resultAlreadyOwned);
    }
    public void OnCraftButton()
    {
        if (currentRecipe == null) return;
        if (_resultAlreadyOwned) return;

        Inventory playerInventory = GetLocalPlayerInventory();

        if (playerInventory != null)
        {
            int leftover = playerInventory.AddItem(currentRecipe.result, currentRecipe.resultQuantity);

            if(leftover < currentRecipe.resultQuantity)
            {
                //Remove the required ingredients from the crafting slots
                ConsumeIngredients(currentRecipe);

                //tutorial step to teach to craft
                TutorialEvents.Raise(TutorialStep.Craft);
            }
        }


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
    private bool AlreadyOwnsResult(RecipeData recipe)
    {
        if(recipe == null) return false;
        if(recipe.result.itemType == ItemType.none) return false;

        Inventory inventory = GetLocalPlayerInventory();
        if (inventory == null) return false;

        return inventory.HasItemInInventory(recipe.result);

    }
    private bool RecipeMatches(RecipeData recipe, Dictionary<ItemData, int> craftingItems)
    {
        if (PlayerLevelUI.Instance != null && PlayerLevelUI.Instance.CurrentLevel < recipe.requiredLevel) return false;
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

    private Inventory GetLocalPlayerInventory()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
        {
            var localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

            if (localPlayerObject != null)
            {
                return localPlayerObject.GetComponent<Inventory>();
            }
        }
        return null;
    }
}