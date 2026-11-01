using UnityEngine;
using Unity.Netcode;

//shared bridge state. opens when any player completes/skips the tutorial
public class BridgeController : NetworkBehaviour
{
    [SerializeField] GameObject[] _enableBridgeWhenOpen;
    [SerializeField] GameObject[] _disableBridgeWhenOpen;

    NetworkVariable<bool> _isBridgeOpen = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        _isBridgeOpen.OnValueChanged += OnOpenChanged;
        TutorialEvents.OnTutorialCompleted += HandleTutorialCompleted;

        Apply(_isBridgeOpen.Value);   //also covers late joiners
    }

    public override void OnNetworkDespawn()
    {
        _isBridgeOpen.OnValueChanged -= OnOpenChanged;
        TutorialEvents.OnTutorialCompleted -= HandleTutorialCompleted;
    }

    void HandleTutorialCompleted() => OpenBridgeServerRpc();

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void OpenBridgeServerRpc()
    {
        _isBridgeOpen.Value = true;
    }

    void OnOpenChanged(bool prev, bool open) => Apply(open);

    void Apply(bool open)
    {
        foreach (GameObject obj in _enableBridgeWhenOpen)
            if(obj != null) obj.SetActive(open);

        foreach(GameObject obj in _disableBridgeWhenOpen)
            if(obj != null) obj.SetActive(!open);
    }
}
