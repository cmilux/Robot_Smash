using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerConfig : NetworkBehaviour 
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private GameObject _camera;
    private CarController _car;

    private void Awake()
    {
        //car controller script
        _car = GetComponent<CarController>();
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

    public void SetInputEnabled(bool value)
    {
        if (!IsOwner) return;

        _playerInput.enabled = value;

        //a released key never reaches on move while disabled, this clears stuck input
        if (!value && _car != null) _car.ResetInput();
    }

    public override void OnNetworkDespawn()
    {
        //disables the unactive player move || desabilita que el jugador inactivo se mueva (unity)
        _playerInput.enabled = false;
        _camera.SetActive(false);
    }
}
