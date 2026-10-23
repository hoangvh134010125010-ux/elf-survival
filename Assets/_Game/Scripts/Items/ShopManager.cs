using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Shop Setup")]
    public Transform shopContainer;
    public GameObject shopItemPrefab;
    public List<ShopItemSO> shopItems;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private bool shopGenerated;

    private void Start()
    {
        if (PlayerStats.Instance != null && HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateGold(PlayerStats.Instance.gold);
        }
        GenerateShopItems();
    }

    public void GenerateShopItems()
    {
        if (shopContainer == null || shopItemPrefab == null) return;
        if (shopGenerated && shopContainer.childCount > 0) return;

        for (int i = shopContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(shopContainer.GetChild(i).gameObject);
        }

        if (shopItems == null) return;

        foreach (ShopItemSO itemData in shopItems)
        {
            ItemDatabase.Register(itemData);
            GameObject newItemUI = Instantiate(shopItemPrefab, shopContainer);
            ShopItemUI uiScript = newItemUI.GetComponent<ShopItemUI>();

            if (uiScript != null)
            {
                uiScript.SetupItem(itemData);
            }
        }

        shopGenerated = true;
    }

    public bool TryBuyItem(ShopItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int totalCost = item.price * amount;

        if (PlayerStats.Instance == null || !PlayerStats.Instance.SpendGold(totalCost))
        {
            NotificationManager.Instance?.ShowNotification("<color=red>Không đủ Vàng để mua vật phẩm!</color>");
            return false;
        }

        InventoryManager.GetOrCreate().AddItemToBag(item, amount);
        PlayerStats.Instance.SaveCurrentPlayerData(true);
        AudioManager.Instance?.PlayBuySuccess();
        NotificationManager.Instance?.ShowNotification($"<color=green>Mua thành công {amount}x {item.itemName}!</color>");
        return true;
    }
}