using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName ="Nuevo Item", menuName ="Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int id = 0;
    public string nombre = "";
    public Sprite icon;
    public int maxStock = 1;
}
