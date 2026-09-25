using System;
using Unity.Netcode;
using UnityEngine;

public class CarBumper : NetworkBehaviour
{
    public bool isEquipped = false;

    //the current equipped bumper base data (damage, cooldown, etc.)
    [SerializeField] private ItemData equippedWeaponData;

    public int currentDurability;

    public event Action OnWeaponBroke;
    public event Action<int, int> OnDurabilityChanged;
    private float nextReadyTime;
    //call by InventoryManager when the bumper change
    public void SetWeaponData(ItemData weaponData)
    {
        equippedWeaponData = weaponData;

        if(equippedWeaponData != null)
        {
            currentDurability = equippedWeaponData.maxDurability;
            OnDurabilityChanged?.Invoke(currentDurability,weaponData.maxDurability);
        }
    }
    public void SetNextDashReadyTime(float time)
    {
        nextReadyTime = time;
    }
    public float GetDashCooldownRemaining()
    {
        return Math.Max(0,nextReadyTime - Time.time);
    }
    public float GetCooldownMax(float backup)
    {
        return equippedWeaponData != null ? equippedWeaponData.cooldownBase : backup;
    }

    //return the bumper dash cooldown. Use backup only if no ItemData was assigned yet.
    public float GetDashCooldown(float backup)
    {
        if (equippedWeaponData == null) return backup;
        return equippedWeaponData.cooldownBase;
    }
    public int GetDamage(int backup)
    {
        if (equippedWeaponData == null) return backup;
        return equippedWeaponData.damageBase;
    }

    public void UseDurability()
    {
        if(equippedWeaponData == null) return;
        if (equippedWeaponData.maxDurability < 0) return;

        currentDurability--;
        OnDurabilityChanged?.Invoke(currentDurability, equippedWeaponData.maxDurability);

        if(currentDurability <= 0)
        {
            BreakWeapon();
        }
    }

    void BreakWeapon()
    {
        equippedWeaponData = null;
        isEquipped = false;

        OnWeaponBroke?.Invoke();
    }
}
