using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    public GameObject playerInventoryUI;

    private PlayerInput playerInput;
    private CarController carController;

    private void Awake()
    {
        // Singleton pattern implementation
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }

        playerInput = GetComponent<PlayerInput>();
        carController = GetComponent<CarController>();
    }

    private void Start()
    {
        playerInventoryUI.SetActive(false);
    }

    private void OnOpenInventory(InputValue value)
    {
        if (value.isPressed)
        {
            // Open or close the menu based on its current active state
            bool isOpening = !playerInventoryUI.activeSelf;
            playerInventoryUI.SetActive(isOpening);

            if (isOpening)
            {
                //  Switch to the "UI" Action Map. 
                // This automatically disables Move, Attack and other Player map actions.
                playerInput.SwitchCurrentActionMap("UI");

                // Show and unlock the cursor for menu navigation
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                // Stop the car
                carController.isFrozen = true;
            }
            else
            {
                // Return to the "Player" Action Map to restore movement and combat
                playerInput.SwitchCurrentActionMap("Player");

                // Hide and lock the cursor for gameplay
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                carController.isFrozen = false;
            }
        }
    }
}
