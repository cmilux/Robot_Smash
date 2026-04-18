using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [HideInInspector] public ItemData itemData;
    [HideInInspector] public int quantity;

    public Image icon;

    private TextMeshProUGUI quantityText;
    private void Start()
    {
        quantityText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetItem(ItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;

        icon.sprite = itemData.icon;
        quantityText.text = quantity.ToString();
    }

    public void ClearItem()
    {
        itemData = null;
        quantity = 0; 
        icon.sprite = null;
        quantityText.text = "";
    }
}
