using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// This script manages weapons, car paint, hotbar keys, and dropping items over the network
public class InventoryManager : NetworkBehaviour
{
    [Header("UI")]
    public GameObject playerInventoryUI;
    public GameObject craftingUI;

    private PlayerInput playerInput;
    private CarController carController;

    [Header("Visual Weapons")]
    public GameObject visibleItemsContainer;
    private VisibleItem[] visibleItems;

    // Network variables to share the current weapon and paint with all players
    private NetworkVariable<int> equippedWeaponId = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    private NetworkVariable<int> equippedSawsId = new NetworkVariable<int>(
    -1,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner
    );
    private NetworkVariable<int> equippedBumperId = new NetworkVariable<int>(
    -1,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> currentPaintId = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [Header("Hotbar")]
    public GameObject hotbarSlotsContainer;

    [Header("Attack System")]
    public PlayerAttackDistance playerAttack;
    public CarSaws carSaws;
    public CarBumper carBumper;

    [Header("Player Visual")]
    public Renderer playerRenderer;
    public Material defaultMaterial;

    [Header("Drop Settings")]
    public Transform dropPoint;

    private void Awake()
    {
        // Get components automatically when the game starts
        playerInput = GetComponent<PlayerInput>();
        carController = GetComponent<CarController>();
        carSaws = GetComponent<CarSaws>();
        carBumper = GetComponent<CarBumper>();
        visibleItems = visibleItemsContainer.GetComponentsInChildren<VisibleItem>(true);
    }

    public override void OnNetworkSpawn()
    {
        // All clients subscribe to the weapon changes
        equippedWeaponId.OnValueChanged += OnWeaponChanged;
        //Force the initial update for late joiners
        OnWeaponChanged(-1, equippedWeaponId.Value);

        // All clients subscribe to the saws changes
        equippedSawsId.OnValueChanged += OnSawsChanged;
        OnSawsChanged(-1, equippedSawsId.Value);

        equippedBumperId.OnValueChanged += OnBumperChanged;
        OnBumperChanged(-1, equippedBumperId.Value);

        // All clients subscribe to the paint changes
        currentPaintId.OnValueChanged += OnPaintChanged;
        OnPaintChanged(-1, currentPaintId.Value);
        //Local UI only for the owner

        if (!IsOwner) return;
        // Find and link the UI elements for the local player
        playerInventoryUI = GameObject.Find("InventoryUI").transform.Find("Inventory").gameObject;
        craftingUI = GameObject.Find("InventoryUI").transform.Find("Craft").gameObject;
        hotbarSlotsContainer = GameObject.Find("HotBar");

        // Hide the inventory UI at the start
        if (playerInventoryUI != null)
        {
            playerInventoryUI.SetActive(false);
        }
        if (craftingUI != null)
        {
            craftingUI.SetActive(false);
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        // clear the events
        equippedWeaponId.OnValueChanged -= OnWeaponChanged;
        equippedSawsId.OnValueChanged -= OnSawsChanged;
        equippedBumperId.OnValueChanged -= OnBumperChanged;
        currentPaintId.OnValueChanged -= OnPaintChanged;
    }

    // this function runs on all clients when the weapon Id changes
    private void OnWeaponChanged(int oldId, int newId)
    {
        foreach (VisibleItem vItem in visibleItems )
        {
            if (vItem.visibleItem != null && vItem.item.itemType == ItemType.weapon)
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
    // this function runs on all clients when the saws Id changes
    private void OnSawsChanged(int oldId, int newId)
    {
        foreach (VisibleItem vItem in visibleItems)
        {
            if (vItem.visibleItem != null && vItem.item.itemType == ItemType.saws)
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

        if (carSaws != null)
        {
            carSaws.isEquipped = (newId != -1);
            // Force the saws off if they get unequipped 
            if (newId == -1)
            {
                carSaws.sawsOn = false;
            }
        }
    }
    private void OnBumperChanged(int oldId, int newId)
    {
        foreach (VisibleItem vItem in visibleItems)
        {
            if (vItem.visibleItem != null && vItem.item.itemType == ItemType.carBumper)
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

        if (carBumper != null)
        {
            carBumper.isEquipped = (newId != -1);
            // Force the bumper off if they get unequipped 
            if (newId == -1)
            {
                carBumper.isEquipped = (newId != -1);
            }
        }
    }
    // This function runs on all clients when the paint Id changes
    private void OnPaintChanged(int oldId, int newId)
    {
        if (playerRenderer == null) return;

        if (newId == -1)
        {
            //  -1 vuelve al default material
            if (defaultMaterial != null)
            {
                playerRenderer.material = defaultMaterial;
            }
        }
        else
        {
            // Busca el item en la base de datos usando el Id para usar su material
            ItemData paintItem = GameManager.instance.itemDataBase.SearchItem(newId.ToString());
            if (paintItem != null && paintItem.paintMaterial != null)
            {
                playerRenderer.material = paintItem.paintMaterial;
            }
        }
    }

    // Automatically called by the Input System when pressing TAB
    private void OnOpenInventory(InputValue value)
    {
        if (!IsOwner) return;
        if (playerInventoryUI == null) return;

        if (value.isPressed)
        {  
            // Open or close the inventory UI window
            bool isOpening = !playerInventoryUI.activeSelf;
            playerInventoryUI.SetActive(isOpening);
            craftingUI.SetActive(isOpening);
            // Change controls and mouse state depending on open/closed state
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

    // Hotbar shortcuts linked to the Input System
    public void OnHotbar1(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(0); }
    public void OnHotbar2(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(1); }
    public void OnHotbar3(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(2); }
    public void OnHotbar4(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(3); }
    public void OnHotbar5(InputValue value) { if (!IsOwner) return; if (value.isPressed) UseHotbarItem(4); }

    // Logic to read the hotbar slots and equip weapons or paint
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
                else if (slotToUse.itemData.itemType == ItemType.saws)
                {
                    EquipSaws(slotToUse.itemData);
                }
                else if (slotToUse.itemData.itemType == ItemType.carBumper) 
                {
                    EquipBumper(slotToUse.itemData);
                }
                else if (slotToUse.itemData.itemType == ItemType.paint)
                {
                    ApplyPaint(slotToUse.itemData);
                }
            }
            else
            {
                UnequipWeapons();
                UnequipSaws();
                UnequipBumper();
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
    private void EquipSaws(ItemData sawsToEquip)
    {
        if (!IsOwner) return;

        equippedSawsId.Value = sawsToEquip.id;
    }

    private void UnequipSaws()
    {
        if (!IsOwner) return;

        equippedSawsId.Value = -1;
    }

    private void EquipBumper(ItemData bumperToEquipo) 
    {  
        if (!IsOwner) return;
        equippedBumperId.Value = bumperToEquipo.id;
    }
    private void UnequipBumper()
    {
        if (!IsOwner) return;
        equippedBumperId.Value = -1;
    }
    void ApplyPaint(ItemData item)
    {
        if (item.paintMaterial != null)
        {
            if (!IsOwner) return;

            currentPaintId.Value = item.id;
        }
    }

    void ResetPaint()
    {
        if (defaultMaterial != null)
        {
            if (!IsOwner) return;
            currentPaintId.Value = -1;
        }
    }

    // Prepares the item data and asks the server to drop it
    public void DropItem(Slot slotToDrop)
    {
        if (!IsOwner) return;

        if (slotToDrop.itemData != null && slotToDrop.itemData.dropPrefab != null)
        {
            // Call the Server Rpc to handle spawning the item
            DropItemServerRpc(slotToDrop.itemData.id, slotToDrop.quantity);

            if (slotToDrop.itemData.itemType == ItemType.weapon)
            {
                UnequipWeapons();
            }
            else if (slotToDrop.itemData.itemType == ItemType.saws)
            {
                UnequipSaws();
            }
            else if (slotToDrop.itemData.itemType == ItemType.carBumper)
            {
                UnequipBumper();
            }
            else if (slotToDrop.itemData.itemType == ItemType.paint)
            {
                ResetPaint();
            }
        }
    }

    // This code runs only on the Server to instantiate and spawn the object for everyone
    [Rpc(SendTo.Server)]
    private void DropItemServerRpc(int itemId, int quantity)
    {
        ItemData itemToDrop = GameManager.instance.itemDataBase.SearchItem(itemId.ToString());
        if (itemToDrop == null) return;

        // Create the item in the server world
        GameObject droppedItem = Instantiate(itemToDrop.dropPrefab, dropPoint.position, dropPoint.rotation);
        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();

        if (pickup != null)
        {
            pickup.quantity = quantity;
        }
        // Spawn the object so all clients can see it
        droppedItem.GetComponent<NetworkObject>().Spawn();
    }
}