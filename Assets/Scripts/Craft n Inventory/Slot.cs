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

    public bool isHotbarSlot = false;
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

        //si este slot es del hotbar y ya tenia algo distinto,desequipar eso primero
        if (isHotbarSlot && this.itemData != null && this.itemData != itemData)
        {
            InventoryManager inventoryManager = GetLocalInventoryManager();

            if (inventoryManager != null)
            {
                inventoryManager.UnequipFromSlot(this.itemData);
            }
        }

        this.itemData = itemData;
        this.quantity = quantity;

        icon.sprite = itemData.icon;
        quantityText.text = quantity.ToString();

        if (isHotbarSlot)
        {
            InventoryManager inventoryManager = GetLocalInventoryManager();

            if (inventoryManager != null)
            {
                inventoryManager.EquipFromSlot(itemData);
            }
        }
    }

    // Empty this slot and clean the UI
    public void ClearItem()
    {   
        if(isHotbarSlot && itemData != null)
        {
            InventoryManager inventoryManager = GetLocalInventoryManager();
            if (inventoryManager != null)
            {
                inventoryManager.UnequipFromSlot(itemData);
            }
        }
        itemData = null;
        quantity = 0; 
        icon.sprite = null;
        quantityText.text = "";
    }

    //encontrar el InventoryManager del jugador local
    private InventoryManager GetLocalInventoryManager()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
        {
            var localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (localPlayerObject != null)
            {
                return localPlayerObject.GetComponent<InventoryManager>();
            }
        }
        return null;
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
            InventoryManager inventoryManager = GetLocalInventoryManager();
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
                if (targetSlot.itemData == null)
                {
                    // Guardar los datos antes de limpiar porque ClearItem los borra
                    ItemData movingItem = itemData;
                    int movingQuantity = quantity;

                    ClearItem();                                  
                    targetSlot.SetItem(movingItem, movingQuantity);

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

                    bool bothAreHotbar = this.isHotbarSlot && targetSlot.isHotbarSlot;

                    if (bothAreHotbar)
                    { //actualiza los datos visuales y reequipa directamente lo que corresponde a cada uno
                        targetSlot.itemData = itemData;
                        targetSlot.quantity = quantity;
                        targetSlot.icon.sprite = itemData.icon;
                        targetSlot.quantityText.text = quantity.ToString();

                        this.itemData = tempItemData;
                        this.quantity = tempQuantity;
                        this.icon.sprite = tempItemData.icon;
                        this.quantityText.text = tempQuantity.ToString();

                        InventoryManager inventoryManager = GetLocalInventoryManager();
                        if (inventoryManager != null)
                        {
                            inventoryManager.EquipFromSlot(itemData);       // equipa lo que quedo en targetSlot
                            inventoryManager.EquipFromSlot(tempItemData);    // equipa lo que quedo en this
                        }
                    }
                    else
                    {
                        targetSlot.SetItem(itemData, quantity);
                        this.SetItem(tempItemData, tempQuantity);
                    }
                }
            }
        }
    }
}
