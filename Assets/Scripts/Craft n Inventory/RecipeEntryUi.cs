using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeEntryUi : MonoBehaviour
{
    [Header("Ingredient A")]
    [SerializeField] private Image _iconA;
    [SerializeField] private TextMeshProUGUI _amountA;

    [Header("Ingredient B")]
    [SerializeField] private Image _iconB;
    [SerializeField] private TextMeshProUGUI _amountB;

    [Header("Result")]
    [SerializeField] private Image _iconResult;

    [Header("Locked")]
    [SerializeField] private TextMeshProUGUI _lockedText;
    [SerializeField] private GameObject _lockedOverlay;

    public void Configure(RecipeData recipe, bool unlocked)
    {
        if(recipe == null) return;

        if(recipe.ingredients.Count < 2)
        {
            Debug.LogWarning($"La receta {recipe.name} no tiene dos ingredientes", this);
            return;
        }
        RecipeIngredient a = recipe.ingredients[0];
        _iconA.sprite = a.itemData.icon;
        _amountA.text = a.quantity.ToString();

        RecipeIngredient b = recipe.ingredients[1];
        _iconB.sprite = b.itemData.icon;
        _amountB.text = b.quantity.ToString();

        _iconResult.sprite = recipe.result.icon;

        _lockedOverlay.SetActive(!unlocked);
        _lockedText.text = $"Nivel {recipe.requiredLevel}";
    }
}
