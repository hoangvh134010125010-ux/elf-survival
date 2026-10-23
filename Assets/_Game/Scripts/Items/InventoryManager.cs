using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{
    public ShopItemSO itemData;
    public int quantity;
    public int upgradeLevel = 0;

    public InventoryItem(ShopItemSO item, int amount = 1, int level = 0)
    {
        itemData = item;
        quantity = amount;
        upgradeLevel = level;
    }

    public int GetCalculatedAtk(int level)
    {
        if (itemData == null) return 0;
        return itemData.atk + (level * CombatBalance.AttackPerUpgrade);
    }

    public int GetCalculatedHp(int level)
    {
        if (itemData == null) return 0;
        return itemData.hp + (level * CombatBalance.HealthPerUpgrade);
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Túi Đồ & Trang Bị")]
    public List<InventoryItem> bagItems = new List<InventoryItem>();
    public InventoryItem equippedWeapon;
    public InventoryItem equippedArmor;

    [Header("Chỉ Số Cộng Thêm Từ Trang Bị")]
    public int bonusAtk = 0;
    public int bonusHp = 0;

    [Header("UI References")]
    public Transform bagContainer;
    public GameObject inventorySlotPrefab;

    [Header("Equip Slot UI")]
    public InventorySlotUI weaponSlotUI;
    public InventorySlotUI armorSlotUI;

    private List<InventorySlotUI> spawnedSlotUIList = new List<InventorySlotUI>();
    private bool inventoryLoaded;

    public bool IsReady => inventoryLoaded;

    public static InventoryManager GetOrCreate()
    {
        if (Instance != null) return Instance;

        GameObject host = new GameObject("InventoryRuntime");
        DontDestroyOnLoad(host);
        InventoryManager runtime = host.AddComponent<InventoryManager>();
        runtime.EnsureLoaded();
        return runtime;
    }

    public void EnsureLoaded()
    {
        if (inventoryLoaded) return;
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        LoadInventoryFromData(SaveSystem.GetSaveSlot(currentSlot));
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.BindSceneUI(this);
            enabled = false;
            return;
        }

        if (transform.parent != null)
        {
            GetOrCreate().BindSceneUI(this);
            enabled = false;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (Instance == this) SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this) return;

        EnsureLoaded();
        BindFromLoadedScenes();
        RecaculateBonusStats();
        RefreshInventoryUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindFromLoadedScenes();
    }

    private void BindFromLoadedScenes()
    {
        InventoryManager[] candidates = FindObjectsByType<InventoryManager>(FindObjectsInactive.Include);
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] != this && candidates[i].bagContainer != null)
            {
                BindSceneUI(candidates[i]);
                return;
            }
        }
    }

    private void BindSceneUI(InventoryManager sceneSource)
    {
        if (sceneSource == null || sceneSource == this) return;

        if (bagContainer != sceneSource.bagContainer)
        {
            spawnedSlotUIList.Clear();
            bagContainer = sceneSource.bagContainer;
        }

        inventorySlotPrefab = sceneSource.inventorySlotPrefab != null ? sceneSource.inventorySlotPrefab : inventorySlotPrefab;
        weaponSlotUI = sceneSource.weaponSlotUI;
        armorSlotUI = sceneSource.armorSlotUI;
        RecaculateBonusStats();
        RefreshInventoryUI();
    }

    public int GetEquippedWeaponLevel()
    {
        return HasEquipped(equippedWeapon) ? equippedWeapon.upgradeLevel : 0;
    }

    public int GetEquippedArmorLevel()
    {
        return HasEquipped(equippedArmor) ? equippedArmor.upgradeLevel : 0;
    }

    private static bool HasEquipped(InventoryItem item)
    {
        return item != null && item.itemData != null;
    }

    public void AddItemToBag(ShopItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return;
        ItemDatabase.Register(item);

        if (item.isStackable)
        {
            InventoryItem existingSlot = bagItems.Find(slot => slot.itemData == item && slot.quantity < item.maxStackSize);
            if (existingSlot != null)
            {
                existingSlot.quantity += amount;
            }
            else
            {
                bagItems.Add(new InventoryItem(item, amount));
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                bagItems.Add(new InventoryItem(item, 1));
            }
        }

        RecaculateBonusStats();
        RefreshInventoryUI();
        PlayerStats.Instance?.RequestSave();
    }

    public InventoryItem FindItemInBag(ShopItemSO item)
    {
        if (item == null) return null;

        InventoryItem exact = bagItems.Find(slot => slot != null && slot.itemData == item && slot.quantity > 0);
        if (exact != null) return exact;

        return bagItems.Find(slot =>
            slot != null &&
            slot.itemData != null &&
            slot.quantity > 0 &&
            (slot.itemData.name == item.name || slot.itemData.itemName == item.itemName));
    }

    public int CountItemInBag(ShopItemSO item)
    {
        if (item == null) return 0;

        int total = 0;
        for (int i = 0; i < bagItems.Count; i++)
        {
            InventoryItem slot = bagItems[i];
            if (slot == null || slot.itemData == null) continue;
            if (slot.itemData == item || slot.itemData.name == item.name || slot.itemData.itemName == item.itemName)
            {
                total += slot.quantity;
            }
        }
        return total;
    }

    public void UseOrEquipItem(InventoryItem slotItem)
    {
        if (slotItem == null || slotItem.itemData == null) return;
        ShopItemSO item = slotItem.itemData;

        if (item.requiredClass != PlayerClass.None && PlayerStats.Instance != null)
        {
            if (item.requiredClass != PlayerStats.Instance.characterClass)
            {
                NotificationManager.Instance?.ShowNotification("<color=red>Trang bị không phù hợp với Lớp Nghề!</color>");
                return;
            }
        }

        switch (item.itemType)
        {
            case ItemType.Consumable:
                if (item.instantHeal > 0 && PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.Heal(item.instantHeal);
                    slotItem.quantity--;
                    if (slotItem.quantity <= 0)
                    {
                        bagItems.Remove(slotItem);
                    }
                }
                break;

            case ItemType.Weapon:
                EquipItem(slotItem, ref equippedWeapon);
                break;

            case ItemType.Armor:
                EquipItem(slotItem, ref equippedArmor);
                break;
        }

        RecaculateBonusStats();
        RefreshInventoryUI();
        PlayerStats.Instance?.SaveCurrentPlayerData(true);
    }

    private void EquipItem(InventoryItem slotItem, ref InventoryItem currentSlot)
    {
        if (currentSlot != null && currentSlot.itemData != null)
        {
            bagItems.Add(new InventoryItem(currentSlot.itemData, 1, currentSlot.upgradeLevel));
        }

        currentSlot = new InventoryItem(slotItem.itemData, 1, slotItem.upgradeLevel);

        slotItem.quantity--;
        if (slotItem.quantity <= 0)
        {
            bagItems.Remove(slotItem);
        }
    }

    public void UnequipWeapon()
    {
        if (equippedWeapon != null && equippedWeapon.itemData != null)
        {
            bagItems.Add(new InventoryItem(equippedWeapon.itemData, 1, equippedWeapon.upgradeLevel));
            equippedWeapon = null;
            RecaculateBonusStats();
            RefreshInventoryUI();
            PlayerStats.Instance?.SaveCurrentPlayerData(true);
        }
    }

    public void UnequipArmor()
    {
        if (equippedArmor != null && equippedArmor.itemData != null)
        {
            bagItems.Add(new InventoryItem(equippedArmor.itemData, 1, equippedArmor.upgradeLevel));
            equippedArmor = null;
            RecaculateBonusStats();
            RefreshInventoryUI();
            PlayerStats.Instance?.SaveCurrentPlayerData(true);
        }
    }

    public void RecaculateBonusStats()
    {
        int totalBonusAtk = 0;
        int totalBonusHp = 0;
        float totalBonusAtkSpeed = 0f;
        float totalBonusMoveSpeed = 0f;

        if (equippedWeapon != null && equippedWeapon.itemData != null)
        {
            totalBonusAtk += equippedWeapon.GetCalculatedAtk(equippedWeapon.upgradeLevel);
            totalBonusAtkSpeed += equippedWeapon.itemData.spd;
        }

        if (equippedArmor != null && equippedArmor.itemData != null)
        {
            totalBonusHp += equippedArmor.GetCalculatedHp(equippedArmor.upgradeLevel);
            totalBonusMoveSpeed += equippedArmor.itemData.spd;
        }

        bonusAtk = totalBonusAtk;
        bonusHp = totalBonusHp;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SetEquipmentBonus(totalBonusAtk, totalBonusHp, totalBonusAtkSpeed, totalBonusMoveSpeed);
        }
    }

    public void RefreshInventoryUI()
    {
        if (weaponSlotUI != null) weaponSlotUI.Setup(HasEquipped(equippedWeapon) ? equippedWeapon : null);
        if (armorSlotUI != null) armorSlotUI.Setup(HasEquipped(equippedArmor) ? equippedArmor : null);

        if (bagContainer == null || inventorySlotPrefab == null)
        {
            QuickPotionUI.Instance?.RefreshQuickPotions();
            return;
        }

        int requiredSlots = bagItems.Count;

        while (spawnedSlotUIList.Count < requiredSlots)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, bagContainer);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI != null) spawnedSlotUIList.Add(slotUI);
        }

        for (int i = 0; i < spawnedSlotUIList.Count; i++)
        {
            if (spawnedSlotUIList[i] == null) continue;

            if (i < requiredSlots)
            {
                spawnedSlotUIList[i].gameObject.SetActive(true);
                spawnedSlotUIList[i].Setup(bagItems[i]);
            }
            else
            {
                spawnedSlotUIList[i].gameObject.SetActive(false);
            }
        }

        QuickPotionUI.Instance?.RefreshQuickPotions();
    }

    public void SaveInventoryToData(SaveData data)
    {
        if (data == null) return;
        if (data.bagItems == null) data.bagItems = new List<InventoryItemSaveData>();
        data.bagItems.Clear();

        foreach (var item in bagItems)
        {
            if (item != null && item.itemData != null)
            {
                data.bagItems.Add(new InventoryItemSaveData(item.itemData.name, item.quantity, item.upgradeLevel, item.itemData.itemName));
            }
        }

        data.equippedWeapon = HasEquipped(equippedWeapon)
            ? new InventoryItemSaveData(equippedWeapon.itemData.name, 1, equippedWeapon.upgradeLevel, equippedWeapon.itemData.itemName)
            : new InventoryItemSaveData(string.Empty, 0, 0);

        data.equippedArmor = HasEquipped(equippedArmor)
            ? new InventoryItemSaveData(equippedArmor.itemData.name, 1, equippedArmor.upgradeLevel, equippedArmor.itemData.itemName)
            : new InventoryItemSaveData(string.Empty, 0, 0);
    }

    public void LoadInventoryFromData(SaveData data)
    {
        bagItems.Clear();
        equippedWeapon = null;
        equippedArmor = null;
        inventoryLoaded = true;

        if (data == null) 
        {
            RecaculateBonusStats();
            RefreshInventoryUI();
            return;
        }

        if (data.bagItems != null)
        {
            foreach (var savedItem in data.bagItems)
            {
                ShopItemSO itemSO = ResolveSavedItem(savedItem);
                if (itemSO != null)
                {
                    bagItems.Add(new InventoryItem(itemSO, Mathf.Max(1, savedItem.quantity), savedItem.upgradeLevel));
                }
            }
        }

        ShopItemSO weaponSO = ResolveSavedItem(data.equippedWeapon);
        if (weaponSO != null)
        {
            equippedWeapon = new InventoryItem(weaponSO, 1, data.equippedWeapon.upgradeLevel);
        }

        ShopItemSO armorSO = ResolveSavedItem(data.equippedArmor);
        if (armorSO != null)
        {
            equippedArmor = new InventoryItem(armorSO, 1, data.equippedArmor.upgradeLevel);
        }

        RecaculateBonusStats();
        RefreshInventoryUI();
    }

    private static ShopItemSO ResolveSavedItem(InventoryItemSaveData savedItem)
    {
        if (savedItem == null) return null;
        if (!string.IsNullOrEmpty(savedItem.itemID))
        {
            ShopItemSO byId = ItemDatabase.Get(savedItem.itemID);
            if (byId != null) return byId;
        }

        if (!string.IsNullOrEmpty(savedItem.itemName))
        {
            return ItemDatabase.Get(savedItem.itemName);
        }

        return null;
    }
}
