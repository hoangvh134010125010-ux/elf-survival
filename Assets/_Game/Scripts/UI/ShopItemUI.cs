using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Text itemNameText;
    public Text itemPriceText; 
    public Image itemIcon;
    public Button buyButton;

    private ShopItemSO currentItem;

    public void SetupItem(ShopItemSO itemData)
    {
        currentItem = itemData;

        if (itemNameText != null) itemNameText.text = currentItem.itemName;
        if (itemPriceText != null) itemPriceText.text = currentItem.price.ToString();
        if (itemIcon != null && currentItem.icon != null) itemIcon.sprite = currentItem.icon;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }
    private void OnBuyClicked()
    {
        if (currentItem == null || ShopManager.Instance == null) return;

        if (currentItem.itemType == ItemType.Consumable || currentItem.isUpgradeStone)
        {
            AudioManager.Instance?.PlayButtonClick();
            PurchaseQuantityPanel.Open(currentItem, transform, itemNameText);
            return;
        }

        ShopManager.Instance.TryBuyItem(currentItem);
    }
}