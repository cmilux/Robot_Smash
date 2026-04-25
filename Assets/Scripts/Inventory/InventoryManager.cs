using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    public GameObject playerInventoryUI;

    private PlayerInput playerInput;
    private CarController carController;

    [Header("Visual Weapons")]
    public GameObject visibleItemsContainer;
    private VisibleItem[] visibleItems;

    [Header("Hotbar")]
    public GameObject hotbarSlotsContainer;

    [Header("Attack System")]
    // NEW: Reference to your shooting script
    public PlayerAttackDistance playerAttack;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }

        playerInput = GetComponent<PlayerInput>();
        carController = GetComponent<CarController>();
        visibleItems = visibleItemsContainer.GetComponentsInChildren<VisibleItem>(true);
    }

    private void Start()
    {
        playerInventoryUI.SetActive(false);

        // Ensure shooting is disabled at the start of the game
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }
    }

    private void OnOpenInventory(InputValue value)
    {
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

    public void OnHotbar1(InputValue value) { if (value.isPressed) UseHotbarItem(0); }
    public void OnHotbar2(InputValue value) { if (value.isPressed) UseHotbarItem(1); }
    public void OnHotbar3(InputValue value) { if (value.isPressed) UseHotbarItem(2); }

    private void UseHotbarItem(int index)
    {
        Slot[] hotbarSlots = hotbarSlotsContainer.GetComponentsInChildren<Slot>(true);

        if (index < hotbarSlots.Length)
        {
            Slot slotToUse = hotbarSlots[index];

            // If the slot has an item and it's a weapon, equip it
            if (slotToUse.itemData != null && slotToUse.itemData.itemType == ItemType.weapon)
            {
                EquipWeapon(slotToUse.itemData);
            }
            // If the slot is empty or isn't a weapon, unequip everything
            else
            {
                UnequipWeapons();
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

        // Disable the shooting script so it doesn't fire
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        Debug.Log("Weapons unequipped");
    }
}