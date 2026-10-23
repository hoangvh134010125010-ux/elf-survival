using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Slot Mode")]
    public bool isEquippedSlot = false;
    public ItemType slotType;

    [Header("UI References")]
    public Image itemIcon;
    public Image borderImage;
    public Text itemCount;
    public Text itemLevelText;
    public Button slotButton;

    private InventoryItem currentSlotItem;

    public void Setup(InventoryItem itemSlot)
    {
        currentSlotItem = itemSlot;

        if (currentSlotItem == null || currentSlotItem.itemData == null)
        {
            if (itemIcon != null) itemIcon.enabled = false;
            if (borderImage != null) borderImage.color = Color.white;
            if (itemCount != null) itemCount.gameObject.SetActive(false);
            if (itemLevelText != null) itemLevelText.gameObject.SetActive(false);
            return;
        }

        ShopItemSO data = currentSlotItem.itemData;

        if (itemIcon != null && data.icon != null)
        {
            itemIcon.sprite = data.icon;
            itemIcon.enabled = true;
        }

        if (data.itemType == ItemType.Weapon || data.itemType == ItemType.Armor)
        {
            if (borderImage != null)
            {
                borderImage.color = ItemVisuals.GetBorderColorByLevel(currentSlotItem.upgradeLevel);
                borderImage.enabled = true;
            }
        }
        else
        {
            if (borderImage != null) borderImage.color = Color.white;
        }

        if (itemLevelText != null)
        {
            if (currentSlotItem.upgradeLevel > 0)
            {
                itemLevelText.text = $"+{currentSlotItem.upgradeLevel}";
                itemLevelText.gameObject.SetActive(true);
            }
            else
            {
                itemLevelText.gameObject.SetActive(false);
            }
        }

        if (itemCount != null)
        {
            if (data.isStackable && currentSlotItem.quantity > 1)
            {
                itemCount.text = currentSlotItem.quantity.ToString();
                itemCount.gameObject.SetActive(true);
            }
            else
            {
                itemCount.gameObject.SetActive(false);
            }
        }

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    private void OnSlotClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (InventoryManager.Instance == null) return;

        if (UpgradeManager.Instance != null && UpgradeManager.Instance.gameObject.activeInHierarchy)
        {
            if (currentSlotItem != null)
            {
                UpgradeManager.Instance.SelectEquipmentToUpgrade(currentSlotItem);
            }
            return;
        }

        if (isEquippedSlot)
        {
            if (slotType == ItemType.Weapon)
            {
                InventoryManager.Instance.UnequipWeapon();
            }
            else if (slotType == ItemType.Armor)
            {
                InventoryManager.Instance.UnequipArmor();
            }
            return;
        }

        if (currentSlotItem != null)
        {
            InventoryManager.Instance.UseOrEquipItem(currentSlotItem);
        }
    }
}