using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Player life")]
    // The server controls the health, but all players can see it
    NetworkVariable<int> health = new NetworkVariable<int>(50,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public int maxHealth = 50;
    public bool isDead = false;

    public override void OnNetworkSpawn()
    {
        // Listen for health changes to update the UI
        health.OnValueChanged += OnHealthChange;

        // Only the server can set the starting health
        if (IsServer)
        {
            health.Value = maxHealth;
        }

        // If this is my local player, update my health UI 
        if (IsOwner) 
        { 
            OnHealthChange(0, health.Value);
        }
    }

    // Called automatically for all clients when the health number changes
    private void OnHealthChange(int oldValue, int newValue)
    {
        // Only update the screen UI for the person who owns this player
        if (!IsOwner) return;

        //Update ui references if value changes || actualiza la ui si los valores cambian
        UIManager.Instance.UpdateHealth(newValue, maxHealth);
    }

    //sends information to server and everyone can call this method || envia la informacion al server y cualquiera puede llamar al metodo
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LoseHealthServerRpc(int damage)
    {
        // If the player is already dead do nothing
        if (isDead) return;

        // Take damage
        health.Value -= damage;

        // Check if the player has died
        if (health.Value <= 0)
        {
            health.Value = 0;

            isDead = true;

            // The server removes the dead player from the game
            NetworkObject.Despawn(true);

        }
    }
    public override void OnNetworkDespawn()
    {
        // Stop listening for health changes to prevent errors
        // Siempre es buena práctica desvincular el evento al salir de la red
        health.OnValueChanged -= OnHealthChange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Healer"))
        {
            health.Value = health.Value + 5;

            if (health.Value == maxHealth)
            {
                health.Value = maxHealth;
            }

            NetworkObject.Destroy(other.gameObject);
        }
    }
}
