using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ServerController : MonoBehaviour
{
    InputSystem_Actions _controls;

    private void Awake()
    {
        _controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _controls.Server.Enable();
        _controls.Server.StartHost.performed += OnStartHost;
        _controls.Server.StartClient.performed += OnStartClient;
    }

    private void OnDisable()
    {
        _controls.Server.StartHost.performed -= OnStartHost;
        _controls.Server.StartClient.performed -= OnStartClient;
        _controls.Server.Disable();
    }

    private void OnStartHost(InputAction.CallbackContext context)
    {
        NetworkManager.Singleton.StartHost(); //Starts hosts || Iniciamos el Host
    }

    private void OnStartClient(InputAction.CallbackContext context)
    {
        NetworkManager.Singleton.StartClient(); //Starts client || Iniciamos como client
    }
}