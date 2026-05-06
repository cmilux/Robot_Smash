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
        Debug.Log($"[RED] Spawned Player. IsOwner: {IsOwner}. NetworkId: {NetworkObjectId}");
        _playerInput.enabled = IsOwner;
        _camera.SetActive(IsOwner);
    }

    public override void OnNetworkDespawn()
    {
        _playerInput.enabled = false;
        _camera.SetActive(false);
    }
}
