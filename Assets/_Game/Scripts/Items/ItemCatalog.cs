using UnityEngine;

[CreateAssetMenu(fileName = "ItemCatalog", menuName = "Game Data/Item Catalog")]
public class ItemCatalog : ScriptableObject
{
    public ShopItemSO[] items;
}
