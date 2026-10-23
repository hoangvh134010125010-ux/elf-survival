using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Thông tin nhân vật")]
    public PlayerClass characterClass = PlayerClass.Warrior;
    public string characterName = "Chiến Binh";
    public int gold = 0;

    [Header("Level EXP (Reset mỗi trận)")]
    public float currentLevel = 1f;
    public float currentExp = 0f;
    public float expToNextLevel = 10f;

    [Header("Chỉ số gốc (Base Stats)")]
    public int baseAtk = 10;
    public int baseMaxHp = 100;
    public int currentHp = 100;
    public float baseAtkSpeed = 1f;
    public float baseMoveSpeed = 4f;

    [Header("Chỉ số từ Trang bị (Bonus Stats)")]
    public int bonusAtk = 0;
    public int bonusMaxHp = 0;
    public float bonusAtkSpeed = 0f;
    public float bonusMoveSpeed = 0f;

    private const float SaveBatchSeconds = 1f;

    private int loadedSlot = -1;
    private bool saveDirty;
    private float nextSaveTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadPlayerDataFromSave();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (saveDirty && Time.unscaledTime >= nextSaveTime)
        {
            SaveCurrentPlayerData(false);
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentPlayerData(true);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && saveDirty) SaveCurrentPlayerData(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        if (saveDirty) SaveCurrentPlayerData(true);

        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        if (loadedSlot != currentSlot || scene.name == SceneNames.MapSelect)
        {
            LoadPlayerDataFromSave();
        }

        if (IsGameplayScene(scene.name))
        {
            ResetBattleStats();
        }

        if (AutoWeapon.Instance != null)
        {
            AutoWeapon.Instance.EquipWeaponByClass();
        }

        InventoryManager.GetOrCreate().RecaculateBonusStats();
        UpdateAllUI();
    }

    private void Start()
    {
        InventoryManager.GetOrCreate();
        UpdateAllUI();
    }

    public void LoadPlayerDataFromSave()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        loadedSlot = currentSlot;
        saveDirty = false;
        SaveData data = SaveSystem.GetSaveSlot(currentSlot);

        if (data != null && data.isOccupied)
        {
            EnsureStarterGear(currentSlot, data);
            characterClass = (PlayerClass)data.characterClass;
            characterName = string.IsNullOrEmpty(data.characterName) ? GetDefaultClassName(characterClass) : data.characterName;
            gold = Mathf.Max(0, data.gold);
            PlayerPrefs.SetInt("SelectedClass", data.characterClass);
        }
        else if (PlayerPrefs.HasKey("SelectedClass"))
        {
            characterClass = (PlayerClass)PlayerPrefs.GetInt("SelectedClass");
            characterName = GetDefaultClassName(characterClass);
        }

        ApplyBaseStatsByClass(characterClass);
        ResetBattleStats();

        if (data != null && data.isOccupied)
        {
            InventoryManager.GetOrCreate().LoadInventoryFromData(data);
        }

        currentHp = GetTotalMaxHp();

        if (AutoWeapon.Instance != null)
        {
            AutoWeapon.Instance.EquipWeaponByClass();
        }

        UpdateAllUI();
    }

    public void ResetBattleStats()
    {
        currentLevel = 1f;
        currentExp = 0f;
        expToNextLevel = 14f;
        ApplyBaseStatsByClass(characterClass);
        InventoryManager.Instance?.RecaculateBonusStats();
        currentHp = GetTotalMaxHp();
        UpdateAllUI();
    }

    public void SaveCurrentPlayerData(bool flushNow = false)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        SaveData data = SaveSystem.GetSaveSlot(currentSlot) ?? new SaveData();

        data.characterClass = (int)characterClass;
        data.characterName = characterName;
        data.gold = gold;

        if (InventoryManager.Instance != null && InventoryManager.Instance.IsReady)
        {
            InventoryManager.Instance.SaveInventoryToData(data);
        }

        SaveSystem.SaveSlot(currentSlot, data, flushNow);
        loadedSlot = currentSlot;
        saveDirty = false;
        nextSaveTime = Time.unscaledTime + SaveBatchSeconds;
    }

    public void RequestSave()
    {
        saveDirty = true;
    }

    private static void EnsureStarterGear(int slotIndex, SaveData data)
    {
        if (data == null || data.gearInitialized) return;

        CombatBalance.ClassBase stats = CombatBalance.GetClass((PlayerClass)data.characterClass);
        if (data.equippedWeapon == null || string.IsNullOrEmpty(data.equippedWeapon.itemID))
        {
            data.equippedWeapon = new InventoryItemSaveData(stats.weaponAssetName, 1, 0);
        }

        if (data.equippedArmor == null || string.IsNullOrEmpty(data.equippedArmor.itemID))
        {
            data.equippedArmor = new InventoryItemSaveData(CombatBalance.ArmorAssetName, 1, 0);
        }

        if (data.bagItems == null) data.bagItems = new System.Collections.Generic.List<InventoryItemSaveData>();
        data.gearInitialized = true;
        SaveSystem.SaveSlot(slotIndex, data, true);
    }

    private void ApplyBaseStatsByClass(PlayerClass pClass)
    {
        CombatBalance.ClassBase stats = CombatBalance.GetClass(pClass);
        baseAtk = stats.attack;
        baseMaxHp = stats.hp;
        baseAtkSpeed = stats.attackSpeed;
        baseMoveSpeed = CombatBalance.MoveSpeed;
    }

    private static string GetDefaultClassName(PlayerClass pClass)
    {
        return CombatBalance.GetClass(pClass).displayName;
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName == SceneNames.Gameplay;
    }

    public int GetTotalAtk() => baseAtk + bonusAtk;
    public int GetTotalMaxHp() => baseMaxHp + bonusMaxHp;
    public float GetTotalAtkSpeed() => baseAtkSpeed + bonusAtkSpeed;
    public float GetTotalMoveSpeed() => CombatBalance.MoveSpeed;

    public void SetEquipmentBonus(int atkBonus, int hpBonus, float atkSpeedBonus = 0f, float moveSpeedBonus = 0f)
    {
        bonusAtk = atkBonus;
        bonusMaxHp = hpBonus;
        bonusAtkSpeed = atkSpeedBonus;
        bonusMoveSpeed = 0f;

        int totalMaxHp = GetTotalMaxHp();
        if (currentHp > totalMaxHp) currentHp = totalMaxHp;

        UpdateAllUI();
    }

    public void AddExp(float amount)
    {
        currentExp += amount;

        bool didLevelUp = false;
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            expToNextLevel *= 1.22f;

            baseAtk += 3;
            baseMaxHp += 14;
            currentHp = GetTotalMaxHp();
            didLevelUp = true;
        }

        if (didLevelUp && HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowLevelupPanel();
        }

        UpdateAllUI();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        UpdateAllUI();
        RequestSave();
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;

        gold -= amount;
        UpdateAllUI();
        SaveCurrentPlayerData(true);
        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHp = Mathf.Min(currentHp + amount, GetTotalMaxHp());
        UpdateAllUI();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        currentHp = Mathf.Max(0, currentHp - damage);
        UpdateAllUI();
    }

    public void UpdateAllUI()
    {
        if (HUDManager.Instance == null) return;

        HUDManager.Instance.UpdateGold(gold);
        HUDManager.Instance.UpdateClass(characterClass);
        HUDManager.Instance.UpdateLevel(Mathf.RoundToInt(currentLevel));
        HUDManager.Instance.UpdateExp(currentExp, expToNextLevel);
        HUDManager.Instance.UpdatePlayerStats(GetTotalAtk(), currentHp, GetTotalMaxHp(), GetTotalAtkSpeed());
    }
}
