using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI References")]
    public Image beforeIcon;
    public Image afterIcon;
    public Image beforeBorder;
    public Image afterBorder;

    public Text beforeNameText;
    public Text afterNameText;
    public Text beforeStatText;
    public Text afterStatText;
    public Text stoneRequirementText;

    public Button upgradeButton;

    [Header("Pick Item Panel")]
    public Button beforeSlotButton;
    public GameObject selectionPanel;
    public Transform selectionContainer;
    public Button closeSelectionButton;

    [Header("Upgrade Settings")]
    public const int MAX_UPGRADE_LEVEL = 15;
    private InventoryItem selectedEquipment;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        ResetUI();
    }

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (beforeSlotButton != null)
        {
            beforeSlotButton.onClick.AddListener(OpenEquipmentSelection);
        }

        if (closeSelectionButton != null)
        {
            closeSelectionButton.onClick.AddListener(CloseEquipmentSelection);
        }

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        ResetUI();
    }

    public void OpenEquipmentSelection()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            PopulateEquipmentList();
        }
    }

    public void CloseEquipmentSelection()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    private void PopulateEquipmentList()
    {
        if (selectionContainer == null || InventoryManager.Instance == null) return;

        foreach (Transform child in selectionContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (InventoryItem item in InventoryManager.Instance.bagItems)
        {
            CreateSelectionSlot(item);
        }

        if (InventoryManager.Instance.equippedWeapon != null)
        {
            CreateSelectionSlot(InventoryManager.Instance.equippedWeapon);
        }

        if (InventoryManager.Instance.equippedArmor != null)
        {
            CreateSelectionSlot(InventoryManager.Instance.equippedArmor);
        }
    }

    private void CreateSelectionSlot(InventoryItem item)
    {
        if (item == null || item.itemData == null) return;
        if (item.itemData.itemType != ItemType.Weapon && item.itemData.itemType != ItemType.Armor) return;

        GameObject slotObj = Instantiate(InventoryManager.Instance.inventorySlotPrefab, selectionContainer);
        InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

        if (slotUI != null)
        {
            slotUI.Setup(item);

            Button targetBtn = slotUI.slotButton != null ? slotUI.slotButton : slotObj.GetComponent<Button>();
            if (targetBtn != null)
            {
                targetBtn.onClick.RemoveAllListeners();
                targetBtn.onClick.AddListener(() => OnEquipmentSelected(item));
            }
        }
    }

    private void OnEquipmentSelected(InventoryItem item)
    {
        SelectEquipmentToUpgrade(item);
        CloseEquipmentSelection();
    }

    public void SelectEquipmentToUpgrade(InventoryItem item)
    {
        if (item == null || item.itemData == null) return;

        if (item.itemData.itemType != ItemType.Weapon && item.itemData.itemType != ItemType.Armor)
        {
            NotificationManager.Instance?.ShowNotification("Chỉ nâng Vũ Khí và Áo Giáp");
            return;
        }

        selectedEquipment = item;
        UpdateUpgradeUI();
    }

    public void UpdateUpgradeUI()
    {
        if (selectedEquipment == null || selectedEquipment.itemData == null)
        {
            ResetUI();
            return;
        }

        ShopItemSO data = selectedEquipment.itemData;
        int currentLvl = selectedEquipment.upgradeLevel;
        int nextLvl = Mathf.Min(currentLvl + 1, MAX_UPGRADE_LEVEL);

        if (beforeIcon != null) { beforeIcon.sprite = data.icon; beforeIcon.enabled = true; }
        if (afterIcon != null) { afterIcon.sprite = data.icon; afterIcon.enabled = true; }

        if (beforeNameText != null) beforeNameText.text = $"{data.itemName} +{currentLvl}";
        if (afterNameText != null)
        {
            afterNameText.text = currentLvl >= MAX_UPGRADE_LEVEL
                ? $"{data.itemName} +{MAX_UPGRADE_LEVEL} (MAX)"
                : $"{data.itemName} +{nextLvl}";
        }

        if (beforeStatText != null)
        {
            beforeStatText.text = data.itemType == ItemType.Weapon
                ? $"ATK: {selectedEquipment.GetCalculatedAtk(currentLvl)}"
                : $"HP: {selectedEquipment.GetCalculatedHp(currentLvl)}";
        }

        if (afterStatText != null)
        {
            afterStatText.text = data.itemType == ItemType.Weapon
                ? $"ATK: {selectedEquipment.GetCalculatedAtk(nextLvl)}"
                : $"HP: {selectedEquipment.GetCalculatedHp(nextLvl)}";
        }

        Color currentBorderColor = ItemVisuals.GetBorderColorByLevel(currentLvl);
        Color nextBorderColor = ItemVisuals.GetBorderColorByLevel(nextLvl);

        if (beforeBorder != null) { beforeBorder.color = currentBorderColor; beforeBorder.enabled = true; }
        if (afterBorder != null) { afterBorder.color = nextBorderColor; afterBorder.enabled = true; }

        int requiredStones = GetRequiredStones(currentLvl);
        int availableStones = GetTotalStonesInInventory();

        if (stoneRequirementText != null)
        {
            if (currentLvl >= MAX_UPGRADE_LEVEL)
            {
                stoneRequirementText.text = "Đã đạt cấp độ tối đa (+15)";
            }
            else
            {
                stoneRequirementText.text = $"Số đá cần dùng: {availableStones}/{requiredStones}";
            }
        }
    }

    private int GetRequiredStones(int currentLevel)
    {
        if (currentLevel >= MAX_UPGRADE_LEVEL) return 0;
        return (currentLevel + 1) * 2;
    }

    private int GetTotalStonesInInventory()
    {
        int total = 0;
        if (InventoryManager.Instance == null) return 0;

        foreach (var item in InventoryManager.Instance.bagItems)
        {
            if (item.itemData != null && item.itemData.isUpgradeStone)
            {
                total += item.quantity;
            }
        }
        return total;
    }

    private void OnUpgradeClicked()
    {
        if (selectedEquipment == null)
        {
            NotificationManager.Instance?.ShowNotification("Chưa chọn trang bị");
            return;
        }

        if (selectedEquipment.upgradeLevel >= MAX_UPGRADE_LEVEL)
        {
            NotificationManager.Instance?.ShowNotification("Trang bị đã đạt cấp tối đa!!!");
            return;
        }

        int requiredStones = GetRequiredStones(selectedEquipment.upgradeLevel);
        int availableStones = GetTotalStonesInInventory();

        if (availableStones < requiredStones)
        {
            NotificationManager.Instance?.ShowNotification("Không đủ đá nâng cấp");
            AudioManager.Instance?.PlayUpgradeFail();
            return;
        }

        ConsumeStonesFromInventory(requiredStones);
        selectedEquipment.upgradeLevel++;

        NotificationManager.Instance?.ShowNotification($"<color=green>Nâng cấp thành công</color> {selectedEquipment.itemData.itemName} +{selectedEquipment.upgradeLevel}");
        AudioManager.Instance?.PlayUpgradeSuccess();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RecaculateBonusStats();
            InventoryManager.Instance.RefreshInventoryUI();
        }

        PlayerStats.Instance?.SaveCurrentPlayerData(true);
        UpdateUpgradeUI();
    }

    private void ConsumeStonesFromInventory(int amountToConsume)
    {
        if (InventoryManager.Instance == null) return;

        for (int i = InventoryManager.Instance.bagItems.Count - 1; i >= 0; i--)
        {
            var slotItem = InventoryManager.Instance.bagItems[i];
            if (slotItem.itemData != null && slotItem.itemData.isUpgradeStone)
            {
                if (slotItem.quantity > amountToConsume)
                {
                    slotItem.quantity -= amountToConsume;
                    amountToConsume = 0;
                    break;
                }
                else
                {
                    amountToConsume -= slotItem.quantity;
                    InventoryManager.Instance.bagItems.RemoveAt(i);
                    if (amountToConsume <= 0) break;
                }
            }
        }
    }

    public void ResetUI()
    {
        selectedEquipment = null;

        if (beforeIcon != null) { beforeIcon.sprite = null; beforeIcon.enabled = false; }
        if (afterIcon != null) { afterIcon.sprite = null; afterIcon.enabled = false; }

        if (beforeBorder != null) { beforeBorder.color = Color.white; }
        if (afterBorder != null) { afterBorder.color = Color.white; }

        if (beforeNameText != null) beforeNameText.text = "Chọn trang bị";
        if (afterNameText != null) afterNameText.text = "Trang bị mới";
        if (beforeStatText != null) beforeStatText.text = "Chỉ số cũ";
        if (afterStatText != null) afterStatText.text = "Chỉ số mới";
        if (stoneRequirementText != null) stoneRequirementText.text = "Số đá cần dùng: 0/0";
    }
}