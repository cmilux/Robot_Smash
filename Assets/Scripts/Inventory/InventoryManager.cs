using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : NetworkBehaviour
{
    [Header("UI")]
    public GameObject playerInventoryUI;

    private PlayerInput playerInput;
    private CarController carController;

    [Header("Visual Weapons")]
    public GameObject visibleItemsContainer;
    private VisibleItem[] visibleItems;

    [Header("Hotbar")]
    public GameObject hotbarSlotsContainer;

    [Header("Attack System")]
    public PlayerAttackDistance playerAttack;

    [Header("Player Visual")]
    public Renderer playerRenderer;
    public Material defaultMaterial;

    [Header("Drop Settings")]
    public Transform dropPoint;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        carController = GetComponent<CarController>();

        visibleItems = visibleItemsContainer.GetComponentsInChildren<VisibleItem>(true);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        playerInventoryUI = GameObject.Find("InventoryUI").transform.Find("Inventory").gameObject;

        hotbarSlotsContainer = GameObject.Find("HotBar");

        if (playerInventoryUI != null)
        {
            playerInventoryUI.SetActive(false);
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }
    }

    private void OnOpenInventory(InputValue value)
    {
        if (!IsOwner) return;

        if (playerInventoryUI == null) return;

        if (value.isPressed)
        {
            bool isOpening = !playerInventoryUI.activeSelf;

            playerInventoryUI.SetActive(isOpening);

            if (isOpening)
            {
                playerInput.SwitchCurrentActionMap("UI");

                Cursor.visible = true;

                Cursor.lockState = CursorLockMode.None;

                carController.isFrozen = true;
            }
            else
            {
                playerInput.SwitchCurrentActionMap("Player");

                Cursor.visible = false;

                Cursor.lockState = CursorLockMode.Locked;

                carController.isFrozen = false;
            }
        }
    }

    public void OnHotbar1(InputValue value)
    {
        if (!IsOwner) return;

        if (value.isPressed)
        {
            UseHotbarItem(0);
        }
            
    }

    public void OnHotbar2(InputValue value)
    {
        if (!IsOwner) return;

        if (value.isPressed)
        {
            UseHotbarItem(1);
        }
            
    }

    public void OnHotbar3(InputValue value)
    {
        if (!IsOwner) return;

        if (value.isPressed)
        {
            UseHotbarItem(2);
        }
           
    }

    public void OnHotbar4(InputValue value)
    {
        if (!IsOwner) return;

        if (value.isPressed)
        {
            UseHotbarItem(3);
        }
            
    }

    public void OnHotbar5(InputValue value)
    {
        if (!IsOwner) return;

        if (value.isPressed)
        {
            UseHotbarItem(4);
        }
           
    }

    private void UseHotbarItem(int index)
    {
        if (hotbarSlotsContainer == null) return;

        Slot[] hotbarSlots = hotbarSlotsContainer.GetComponentsInChildren<Slot>(true);

        if (index < hotbarSlots.Length)
        {
            Slot slotToUse = hotbarSlots[index];

            if (slotToUse.itemData != null)
            {
                // Es un arma? La equipa
                if (slotToUse.itemData.itemType == ItemType.weapon)
                {
                    EquipWeapon(slotToUse.itemData);
                }
                else if (slotToUse.itemData.itemType == ItemType.paint)
                {
                    ApplyPaint(slotToUse.itemData);
                }
            }
            else
            {
                UnequipWeapons();

                ResetPaint();
            }
        }
    }

    private void EquipWeapon(ItemData weaponToEquip)
    {
        foreach (VisibleItem vItem in visibleItems)
        {
            vItem.visibleItem.SetActive(false);

            if (vItem.item == weaponToEquip)
            {
                vItem.visibleItem.SetActive(true);

                Debug.Log("Equipping weapon: " + weaponToEquip.nombre);
            }
        }

        // Enable the shooting script
        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }
    }

    private void UnequipWeapons()
    {
        // Turn off all 3D models
        foreach (VisibleItem vItem in visibleItems)
        {
            vItem.visibleItem.SetActive(false);
        }

        // Disable the shooting script
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }
    }

    void ApplyPaint(ItemData item)
    {
        Debug.Log(System.Environment.StackTrace);
        if (item.paintMaterial != null)
        {
            playerRenderer.material = item.paintMaterial;

            Debug.Log("Paint applied");
        }
    }

    void ResetPaint()
    {
        if (defaultMaterial != null)
        {
            playerRenderer.material = defaultMaterial;
        }
    }

    public void DropItem(Slot slotToDrop)
    {
        if (slotToDrop.itemData != null &&
            slotToDrop.itemData.dropPrefab != null)
        {
            DropItemServerRpc(slotToDrop.itemData.id, slotToDrop.quantity);

            if (slotToDrop.itemData.itemType == ItemType.weapon)
            {
                UnequipWeapons();
            }
            else if (slotToDrop.itemData.itemType == ItemType.paint)
            {
                ResetPaint();
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void DropItemServerRpc(int itemId, int quantity)
    {
        ItemData itemToDrop = GameManager.instance.itemDataBase.SearchItem(itemId.ToString());

        if (itemToDrop == null)
            return;

        GameObject droppedItem = Instantiate(itemToDrop.dropPrefab, dropPoint.position, dropPoint.rotation);

        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();

        if (pickup != null)
        {
            pickup.quantity = quantity;
        }

        droppedItem.GetComponent<NetworkObject>().Spawn();
    }
}