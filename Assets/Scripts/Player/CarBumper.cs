using Unity.Netcode;
using UnityEngine;

public class CarBumper : NetworkBehaviour
{
    public bool isEquipped = false;

    //the current equipped bumper base data (damage, cooldown, etc.)
    [SerializeField] private ItemData equippedWeaponData;

    //call by InventoryManager when the bumper change
    public void SetWeaponData(ItemData weaponData)
    {
        equippedWeaponData = weaponData;
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
}
