using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarSaws : NetworkBehaviour
{
    // True when the player has the saws equipped from the inventory
    public bool isEquipped = false;
    // True when the player turned the saws on with the button
    public bool sawsOn = false; // deberia ser networkvariable para que el otro jugador vea la rotacion visual

    public float damageRate = 1f; // how often the same enemy can be hit again

    public float onDuration = 5f; // how long the saws stay on

    private bool onCooldown = false;

    private ulong shooterClientId;

    private Dictionary<GameObject, float> nextDamageTime = new Dictionary<GameObject, float>();

    //the current equipped saws  base data (damage, cooldown, etc.)
    [SerializeField] ItemData equippedWeaponData;
    public override void OnNetworkSpawn()
    {
        shooterClientId = OwnerClientId;
    }

    //call by InventoryManager when the saws change
    public void SetWeaponData(ItemData weaponData)
    {
        equippedWeaponData = weaponData;
    }

    // Called by the Input System when pressing the saw power button(barra espaciadora)
    public void OnSawsPower(InputValue value)
    {
        if (!IsOwner) return;
        if (!isEquipped) return;
        if (!value.isPressed) return;

        if (sawsOn) return; // already spinning ignore extra press

        if (onCooldown) return; // still waiting to be usable again

        StartCoroutine(SawsOnRoutine());
    }
    // Turns the saws on, wait, then turns them off automatic
    private IEnumerator SawsOnRoutine()
    {
        sawsOn = true;                          // prende
        yield return new WaitForSeconds(onDuration);  // espera
        sawsOn = false;                         // se apaga
        onCooldown = true;                      // marca que hay que esperar
        if (equippedWeaponData != null)
        {
            yield return new WaitForSeconds(equippedWeaponData.cooldownBase);
        }
        onCooldown = false;                     // ahora se puede volver a usar
    }
    private void OnCollisionStay(Collision collision)
    {
        if (!isEquipped) return;
        if (!sawsOn) return;
        if (!IsOwner) return;
        if (!collision.gameObject.CompareTag("Enemy")) return;
        if (equippedWeaponData == null) return;

        // Skip if this enemy was already hit recently
        if (nextDamageTime.TryGetValue(collision.gameObject, out float time) && Time.time < time)
        {
            return;
        }

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamageServerRpc(equippedWeaponData.damageBase, shooterClientId);
            nextDamageTime[collision.gameObject] = Time.time + damageRate;
        }
    }
}