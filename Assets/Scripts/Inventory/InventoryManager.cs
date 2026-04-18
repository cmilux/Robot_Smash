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
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }

        playerInput = GetComponent<PlayerInput>();
        carController = GetComponent<CarController>();
    }
    private void OnOpenInventory(InputValue value)
    {
        if (value.isPressed)
        {
            bool isOpening = !playerInventoryUI.activeSelf;
            playerInventoryUI.SetActive(isOpening);

            if (isOpening)
            {
                // 1. Cambiamos al mapa de "UI". 
                // Esto DESACTIVA automáticamente Move, Attack y todo lo del mapa "Player".
                playerInput.SwitchCurrentActionMap("UI");

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                // Si quieres que el coche se detenga físicamente (frenazo seco)
                GetComponent<CarController>().isFrozen = true;
            }
            else
            {
                // 2. Volvemos al mapa de "Player" para poder movernos y disparar
                playerInput.SwitchCurrentActionMap("Player");

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GetComponent<CarController>().isFrozen = false;
            }
        }
    }
    //FALTA AGREGAR LOGICA PARA QUE EL JUGADOR NO SE PUEDA MOVER
}
