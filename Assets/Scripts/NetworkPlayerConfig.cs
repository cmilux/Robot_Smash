using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerConfig : NetworkBehaviour 
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private GameObject _camera;

    private void Awake()
    {
        //PlayerInput
        _playerInput.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        //enables the active player move || permite que el jugador activo se mueva (unity)
        Debug.Log($"[RED] Spawned Player. IsOwner: {IsOwner}. NetworkId: {NetworkObjectId}");
        _playerInput.enabled = IsOwner;
        _camera.SetActive(IsOwner);
    }

    public override void OnNetworkDespawn()
    {
        //disables the unactive player move || desabilita que el jugador inactivo se mueva (unity)
        _playerInput.enabled = false;
        _camera.SetActive(false);
    }
}
