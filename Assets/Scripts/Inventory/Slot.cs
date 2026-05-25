using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This script controls one single slot in the UI and handles Drag and Drop
public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler,IEndDragHandler
{
    [HideInInspector] public ItemData itemData;
    [HideInInspector] public int quantity;

    public Image icon;

    public TextMeshProUGUI quantityText;
    private void Start()
    {
        // Find the text object that shows the item count
        quantityText = transform.Find("Quantity")?.GetComponentInChildren<TextMeshProUGUI>();
    }

    //Put an item into this slot and update the UI
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

    // Empty this slot and clean the UI
    public void ClearItem()
    {
        itemData = null;
        quantity = 0; 
        icon.sprite = null;
        quantityText.text = "";
    }

    // Called exactly when the player starts dragging the item
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(itemData == null) return;

        // Hide the real item slot text and icon while dragging
        quantityText.text = "";
        icon.enabled = false;

        // Show the moving icon (ghost icon) with the item picture
        UIManager.Instance.ghostIcon.enabled = true;
        UIManager.Instance.ghostIcon.sprite = itemData.icon;
    }

    // Called constantly while the player moves the mouse
    public void OnDrag(PointerEventData eventData)
    {
        if(itemData == null) return;

        // Move the ghost icon to the mouse position
        UIManager.Instance.ghostIcon.transform.position = eventData.position; 
    }

    // Called when the player releases the mouse click
    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemData == null) return;

        // Show the normal slot UI again
        icon.enabled = true;
        UIManager.Instance.ghostIcon.enabled = false;
        quantityText.text = quantity.ToString();

        // Lo solto afuera de la interfaz?
        if (eventData.pointerEnter == null)
        {
            // Buscar el InventoryManager del JUGADOR LOCAL en lugar de usar FindFirstObjectByType
            InventoryManager inventoryManager = null;

            // Get the local player object using Unity Netcode
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
            {
                var localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (localPlayerObject != null)
                {
                    inventoryManager = localPlayerObject.GetComponent<InventoryManager>();
                }
            }
            // If we found the local player, drop the item on the floor
            if (inventoryManager != null)
            {
                inventoryManager.DropItem(this);
                ClearItem();
            }
            return;
        }
        // Check if the item was dropped on another Slot
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
                    // Swap the two different items using a temporary variable
                    ItemData tempItemData = targetSlot.itemData;

                    int tempQuantity = targetSlot.quantity;

                    targetSlot.SetItem(itemData,quantity);

                    this.SetItem(tempItemData, tempQuantity);
                }
            }
        }
    }
}
