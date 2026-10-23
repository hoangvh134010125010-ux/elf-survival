using UnityEngine;

public enum ItemType
{
    Consumable,
    Weapon,
    Armor
}
public enum PlayerClass
{
    None,
    Warrior,
    Mage,
    Archer

}

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Item")]
public class ShopItemSO : ScriptableObject
{
    public PlayerClass requiredClass = PlayerClass.None;
    public string itemName;
    public int price;
    public Sprite icon;
    public ItemType itemType;
    public int atk;
    public int hp;
    public float spd;
    public int instantHeal;
    public bool isStackable = false;
    public int maxStackSize = 9999;
    public bool isUpgradeStone = false;
}
