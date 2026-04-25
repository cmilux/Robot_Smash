using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler,IEndDragHandler
{
    [HideInInspector] public ItemData itemData;
    [HideInInspector] public int quantity;

    public Image icon;

    public TextMeshProUGUI quantityText;
    private void Start()
    {
        quantityText = transform.Find("Quantity")?.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetItem(ItemData itemData, int quantity)
    {
        if (icon == null) Debug.LogError("ICON IS NULL", this);
        if (quantityText == null) Debug.LogError("QUANTITY TEXT IS NULL", this);
        if (itemData == null) Debug.LogError("ITEMDATA IS NULL", this);

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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(itemData == null) return;

        quantityText.text = "";
        icon.enabled = false;

        UIManager.Instance.ghostIcon.enabled = true;
        UIManager.Instance.ghostIcon.sprite = itemData.icon;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(itemData == null) return;

        UIManager.Instance.ghostIcon.transform.position = eventData.position; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemData == null) return;

        icon.enabled = true;

        UIManager.Instance.ghostIcon.enabled = false;
        quantityText.text = quantity.ToString();
        // Lo solto afuera de la interfaz?
        if (eventData.pointerEnter == null)
        {
            InventoryManager.instance.DropItem(this);
            ClearItem();
            return; 
        }

        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("Slot"))
        {
            Slot targetSlot = eventData.pointerEnter.GetComponent<Slot>();

            if(targetSlot != null && targetSlot != this)
            {    
                //If its empty we save it here
                if(targetSlot.itemData == null)
                {
                    targetSlot.SetItem(itemData, quantity);
                    ClearItem();
                    return;
                }
                //If its the same slot, we try to add the quanty at least some.
                else if(targetSlot.itemData == itemData)
                {
                    if(targetSlot.quantity + quantity <= itemData.maxStock)// pude sumar todo
                    {
                        targetSlot.SetItem(itemData, targetSlot.quantity + quantity);
                        ClearItem();
                    }
                    else //sumo lo que se pueda
                    {
                        int quantityToMove = itemData.maxStock - targetSlot.quantity;

                        targetSlot.SetItem(itemData, itemData.maxStock);

                        this.SetItem(itemData, quantity - quantityToMove);
                    }
                }
                // si no esta vacio y tampoco es el mismo item
                else
                {
                    ItemData tempItemData = targetSlot.itemData;

                    int tempQuantity = targetSlot.quantity;

                    targetSlot.SetItem(itemData,quantity);

                    this.SetItem(tempItemData, tempQuantity);
                }
            }
        }
    }
}
