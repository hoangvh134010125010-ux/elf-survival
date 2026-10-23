using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, ShopItemSO> cache;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cache = null;
    }

    public static ShopItemSO Get(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        EnsureLoaded();

        if (cache.TryGetValue(itemId, out ShopItemSO item) && item != null)
        {
            return item;
        }

        foreach (var pair in cache)
        {
            if (pair.Value != null && pair.Value.itemName == itemId)
            {
                return pair.Value;
            }
        }

        return null;
    }

    public static void Register(ShopItemSO item)
    {
        if (item == null) return;
        EnsureLoaded();
        cache[item.name] = item;
        if (!string.IsNullOrEmpty(item.itemName))
        {
            cache[item.itemName] = item;
        }
    }

    private static void EnsureLoaded()
    {
        if (cache != null) return;

        cache = new Dictionary<string, ShopItemSO>(64);

        ItemCatalog catalog = Resources.Load<ItemCatalog>("ItemCatalog");
        if (catalog != null && catalog.items != null)
        {
            for (int i = 0; i < catalog.items.Length; i++)
            {
                RegisterInternal(catalog.items[i]);
            }
        }

        ShopItemSO[] resourceItems = Resources.LoadAll<ShopItemSO>("");
        for (int i = 0; i < resourceItems.Length; i++)
        {
            RegisterInternal(resourceItems[i]);
        }
    }

    private static void RegisterInternal(ShopItemSO item)
    {
        if (item == null) return;
        cache[item.name] = item;
        if (!string.IsNullOrEmpty(item.itemName))
        {
            cache[item.itemName] = item;
        }
    }
}
