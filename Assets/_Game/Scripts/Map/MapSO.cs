using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Text;

public enum MapDifficulty { Easy, Normal, Hard, Hell }

[CreateAssetMenu(fileName = "New Map", menuName = "Game Data/Map Config")]
public class MapSO : ScriptableObject
{
    public string mapID;
    public string sceneName = SceneNames.Gameplay;
    public string mapName;
    public MapDifficulty difficulty;

    [Header("Enemy Settings")]
    public List<GameObject> smallEnemyPrefabs;
    public GameObject bossPrefab;

    [Header("Độ khó combat")]
    public float enemyHpMultiplier = 1f;
    public float enemyDamageMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;
    public int baseEnemyCount = CombatBalance.FirstWaveEnemies;
    [FormerlySerializedAs("extraEnemiesPerWave")]
    [Tooltip("Phần trăm số quái tăng thêm sau mỗi lượt.")]
    public int waveGrowthPercent = CombatBalance.WaveGrowthPercent;

    [Header("Giao diện map")]
    public string styleName;
    public List<TileBase> groundTiles = new List<TileBase>();
    [Tooltip("Bật khi groundTiles là một khối 3x3 lặp lại (xếp từ hàng trên, trái sang phải).")]
    public bool groundIsPattern3x3;
    public List<TileBase> accentTiles = new List<TileBase>();
    [Range(0f, 1f)] public float accentChance;
    public Color groundTint = Color.white;
    [Tooltip("Khối 3x3 lặp lại để vẽ tường bao quanh map (xếp từ hàng trên, trái sang phải).")]
    public List<TileBase> wallTiles = new List<TileBase>();
    public Color wallTint = new Color(0.62f, 0.6f, 0.55f);
    public Color wallTrimTint = new Color(0.9f, 0.86f, 0.75f);
    public List<GameObject> decorPrefabs = new List<GameObject>();
    public int decorCount = 26;
    public Color decorTint = Color.white;

    [Header("Điều kiện vào map")]
    public int requiredWeaponUpgrade;
    public int requiredArmorUpgrade;
    public int requiredAtk;
    public int requiredHp;

    [Header("Rewards (Boss Drop)")]
    public int goldReward = 800;
    public List<ShopItemSO> itemDrops;

    public void EnsureDefaults()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = SceneNames.Gameplay;
        }

        CombatBalance.MapRule rule = CombatBalance.GetMap(difficulty);
        if (string.IsNullOrEmpty(mapName)) mapName = rule.defaultName;

        enemyHpMultiplier = rule.enemyHpMultiplier;
        enemyDamageMultiplier = rule.enemyDamageMultiplier;
        enemySpeedMultiplier = rule.enemySpeedMultiplier;
        baseEnemyCount = CombatBalance.FirstWaveEnemies;
        waveGrowthPercent = CombatBalance.WaveGrowthPercent;
        requiredWeaponUpgrade = rule.requiredWeaponUpgrade;
        requiredArmorUpgrade = rule.requiredArmorUpgrade;
        requiredAtk = rule.requiredAttack;
        requiredHp = rule.requiredHp;
        goldReward = rule.goldReward;
    }

    public string RequirementText()
    {
        EnsureDefaults();
        if (requiredWeaponUpgrade <= 0 && requiredArmorUpgrade <= 0 && requiredAtk <= 0 && requiredHp <= 0)
        {
            return "Không yêu cầu";
        }

        StringBuilder builder = new StringBuilder();
        if (requiredWeaponUpgrade > 0) builder.Append($"Vũ khí +{requiredWeaponUpgrade}  ");
        if (requiredArmorUpgrade > 0) builder.Append($"Giáp +{requiredArmorUpgrade}  ");
        if (requiredAtk > 0) builder.Append($"ATK {requiredAtk}  ");
        if (requiredHp > 0) builder.Append($"HP {requiredHp}");
        return builder.ToString().Trim();
    }

    public bool CanEnter(out string failMessage)
    {
        EnsureDefaults();

        int weaponLevel = InventoryManager.GetOrCreate().GetEquippedWeaponLevel();
        int armorLevel = InventoryManager.GetOrCreate().GetEquippedArmorLevel();
        int atk = PlayerStats.Instance != null ? PlayerStats.Instance.GetTotalAtk() : 0;
        int hp = PlayerStats.Instance != null ? PlayerStats.Instance.GetTotalMaxHp() : 0;

        List<string> missing = new List<string>(4);
        if (requiredWeaponUpgrade > 0 && weaponLevel < requiredWeaponUpgrade)
        {
            missing.Add($"vũ khí +{requiredWeaponUpgrade} trở lên (đang +{weaponLevel})");
        }

        if (requiredArmorUpgrade > 0 && armorLevel < requiredArmorUpgrade)
        {
            missing.Add($"giáp +{requiredArmorUpgrade} trở lên (đang +{armorLevel})");
        }

        if (requiredAtk > 0 && atk < requiredAtk)
        {
            missing.Add($"tối thiểu {requiredAtk} ATK (đang {atk})");
        }

        if (requiredHp > 0 && hp < requiredHp)
        {
            missing.Add($"tối thiểu {requiredHp} HP (đang {hp})");
        }

        if (missing.Count == 0)
        {
            failMessage = string.Empty;
            return true;
        }

        failMessage = $"{mapName} yêu cầu " + string.Join(", ", missing) + ".";
        return false;
    }
}
