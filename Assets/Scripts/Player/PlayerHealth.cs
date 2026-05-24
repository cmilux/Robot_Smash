using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Player life")]
    NetworkVariable<int> health = new NetworkVariable<int>(50,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public int maxHealth = 50;
    public bool isDead = false;

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChange;

        if (IsServer)
        {
            health.Value = maxHealth;
        }

        if (IsOwner) 
        { 
            OnHealthChange(0, health.Value);
        }
    }

    private void OnHealthChange(int oldValue, int newValue)
    {
        if (!IsOwner) return;

        //Update ui references if value changes || actualiza la ui si los valores cambian
        UIManager.Instance.UpdateHealth(newValue, maxHealth);
    }

    //sends information to server and everyone can call this method || envia la informacion al server y cualquiera puede llamar al metodo
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LoseHealthServerRpc(int damage)
    {
        if (isDead) return;

        health.Value -= damage;

        if (health.Value <= 0)
        {
            health.Value = 0;

            isDead = true;

            NetworkObject.Despawn(true);

        }
    }
    public override void OnNetworkDespawn()
    {
        // Siempre es buena práctica desvincular el evento al salir de la red
        health.OnValueChanged -= OnHealthChange;
    }
}
