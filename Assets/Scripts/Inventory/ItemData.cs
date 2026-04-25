using UnityEngine;
using UnityEngine.UI;
public enum ItemType
{
    none,
    weapon,
    paint
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

}