using System.Collections;
using Unity.Netcode;
using UnityEngine;

//server decides when its used, everyone hides/shows it through a net variable || server decide cuando se usa, todos lo ocultan/muestran con una net var
public class PlayerHealer : NetworkBehaviour
{
    public int healing = 20;        //health restored
    
    [SerializeField] float _respawnTime = 30f;

    NetworkVariable<bool> _available = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] GameObject _healerVFX;

    public override void OnNetworkSpawn()
    {
        _available.OnValueChanged += OnAvailableChanged;
        SetVisible(_available.Value);
    }

    public override void OnNetworkDespawn()
    {
        _available.OnValueChanged -= OnAvailableChanged;
    }

    //server only: true if it was available and is now consumed
    //solo server: true si estaba disponible y ahora quedo consumido
    public bool TryConsume()
    {
        if (!IsServer || !_available.Value) return false;

        _available.Value = false;
        StartCoroutine(RespawnRoutine());
        return true;
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_respawnTime);
        _available.Value = true;
    }

    void OnAvailableChanged(bool prev, bool available) => SetVisible(available);

    //renderers + colliders off instead of setactive, so the script and coroutine keep running
    //apagamos renderers + colliders en vez de setactive, asi el script y la corrutina siguen corriendo
    void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
        foreach (Collider c in GetComponentsInChildren<Collider>(true)) c.enabled = visible;

        if(_healerVFX != null) _healerVFX.SetActive(visible);
    }
}
