using UnityEngine;
using UnityEngine.UI;
public enum ItemType
{
    none,
    weapon,
    paint,
    saws,
    carBumper,
    carSkin
}

[CreateAssetMenu(fileName ="Nuevo Item", menuName ="Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int id = 0;
    public string nombre = "";
    public Sprite icon;
    public int maxStock = 1;

    [Header("Weapons")]
    public int visibleItemID = -1;
    public ItemType itemType;

    [Header("Paint System")]
    public Material paintMaterial;

    // El objeto 3D fisico que cae al suelo
    [Header("Drop System")]
    public GameObject dropPrefab;

    [Header("Car Variant")]
    public CarVariantData carVariant;

    [Header("Weapon Stats")]

    public int damageBase= 5;
    public float cooldownBase= 2f;
    public int maxDurability = -1;//-1 means it never breaks
    public int maxAmmo = -1; // -1 means the weapon doesn't use ammunition
    public int bulletsPerShot = 1; //how many bullets fire
    public float bulletSpeed = 20;
    public float spreadAngle = 0f;//0 = no spread (direction accurate)
}