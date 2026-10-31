using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Player life")]
    // The server controls the health, but all players can see it
    NetworkVariable<int> health = new NetworkVariable<int>(50,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public int maxHealth = 50;
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] float _respawnDelay = 5f;

    public override void OnNetworkSpawn()
    {
        // Listen for health changes to update the UI
        health.OnValueChanged += OnHealthChange;

        isDead.OnValueChanged += OnDeadChange;

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
        UIManager.Instance.UpdatePlayerHealth(newValue, maxHealth);
    }

    //sends information to server and everyone can call this method || envia la informacion al server y cualquiera puede llamar al metodo
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LoseHealthServerRpc(int damage)
    {
        // If the player is already dead do nothing
        if (isDead.Value) return;

        // Take damage
        health.Value -= damage;

        // Check if the player has died
        if (health.Value <= 0)
        {
            health.Value = 0;

            isDead.Value = true;

            if (AreAllPlayersDead())
                ShowGameOverClientRpc();        //everyone dead == game over || todos muertos = game over
            else
                StartCoroutine(RespawnRoutine());       //someone alive = respawn || alguien vivo = respawn

        }
    }

    bool AreAllPlayersDead()
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            PlayerHealth ph = client.PlayerObject.GetComponent<PlayerHealth>();
            if (ph != null && !ph.isDead.Value) return false;
        }
        return true;
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_respawnDelay);

        //the other player may have died while we waited || el otro jugador pudo morir mientras esperabamos
        if (AreAllPlayersDead()) yield break;

        Transform point = PlayerSpawner.instance.GetSpawnPoint();

        health.Value = maxHealth;
        isDead.Value = false;

        RespawnOwnerRpc(point.position, point.rotation);
    }

    //runs on the cars owner, its the one w move authority || corre en el dueno del auto, que es quien tiene autoridad del movimiento
    [Rpc(SendTo.Owner)]
    void RespawnOwnerRpc(Vector3 position, Quaternion rotation)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;     //before touching velocity || antes de tocar la velocidad
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        NetworkTransform nt = GetComponent<NetworkTransform>();
        if (nt != null) nt.Teleport(position, rotation, transform.localScale);
        else transform.SetPositionAndRotation(position, rotation);

        //falta mas aca
    }

    void OnDeadChange(bool prev, bool dead)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true)) r.enabled = !dead;
        foreach (Collider c in GetComponentsInChildren<Collider>(true)) c.enabled = !dead;
        
        if (IsOwner)
        {
            GetComponent<Rigidbody>().isKinematic = dead;       //so it doesnt fall through the ground without colliders || para quue no se caiga al vacio sin colliders
            GetComponent<PlayerInput>().enabled = !dead;
        }
    }

    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        //falta aca
    }

    public override void OnNetworkDespawn()
    {
        // Stop listening for health changes to prevent errors
        // Siempre es buena práctica desvincular el evento al salir de la red
        health.OnValueChanged -= OnHealthChange;

        isDead.OnValueChanged -= OnDeadChange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Healer"))
        {
            if (health.Value == maxHealth) return;

            PlayerHealer healer = other.GetComponent<PlayerHealer>();

            health.Value = health.Value + healer.healing;

            if (health.Value >= maxHealth)
            {
                health.Value = maxHealth;
            }

            NetworkObject.Destroy(other.gameObject);
        }
    }
}
