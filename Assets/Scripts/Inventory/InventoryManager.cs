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

    private NetworkVariable<int> equippedWeaponId = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

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
        // All clients subscribe to the weapon changes
        equippedWeaponId.OnValueChanged += OnWeaponChanged;

        //Force the initial update for late joiners
        OnWeaponChanged(-1, equippedWeaponId.Value);

        //Local UI only for the owner
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

    public override void OnNetworkDespawn()
    {
        // clear the event 
        equippedWeaponId.OnValueChanged -= OnWeaponChanged;
    }

    // this function runs on all clients when the weapon Id changes
    private void OnWeaponChanged(int oldId, int newId)
    {
        foreach (VisibleItem vItem in visibleItems)
        {
            if (vItem.visibleItem != null)
            {
                // turn off the visual
                vItem.visibleItem.SetActive(false);

                // turn on the one that matches the new Id
                if (vItem.item.id == newId)
                {
                    vItem.visibleItem.SetActive(true);
                }
            }
        }

        // Enable or disable the attack script only for the owner
        if (IsOwner && playerAttack != null)
        {
            playerAttack.enabled = (newId != -1);
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

    public void OnHotbar1(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(0); }
    public void OnHotbar2(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(1); }
    public void OnHotbar3(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(2); }
    public void OnHotbar4(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(3); }
    public void OnHotbar5(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(4); }

    private void UseHotbarItem(int index)
    {
        if (hotbarSlotsContainer == null) return;
        Slot[] hotbarSlots = hotbarSlotsContainer.GetComponentsInChildren<Slot>(true);

        if (index < hotbarSlots.Length)
        {
            Slot slotToUse = hotbarSlots[index];

            if (slotToUse.itemData != null)
            {
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

    // only update the networkVariable 
    private void EquipWeapon(ItemData weaponToEquip)
    {
        if (!IsOwner) return;

        // cuando este valor cambia OnWeaponChanged se ejecuta en todos los clientes automaticamente
        equippedWeaponId.Value = weaponToEquip.id;
    }

    private void UnequipWeapons()
    {
        if (!IsOwner) return;

        equippedWeaponId.Value = -1;
    }

    void ApplyPaint(ItemData item)
    {
        // esto sigue siendo local si quiero que todos vean el cambio tengo que hacer una networkvariable para el Id de la pintura
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
        if (!IsOwner) return;
        if (slotToDrop.itemData != null && slotToDrop.itemData.dropPrefab != null)
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
        if (itemToDrop == null) return;

        GameObject droppedItem = Instantiate(itemToDrop.dropPrefab, dropPoint.position, dropPoint.rotation);
        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();

        if (pickup != null)
        {
            pickup.quantity = quantity;
        }

        droppedItem.GetComponent<NetworkObject>().Spawn();
    }
}