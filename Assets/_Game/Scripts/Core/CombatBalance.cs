using UnityEngine;

public static class CombatBalance
{
    public const int WavesPerMap = 7;
    public const int MiniBossWave = 5;
    public const int FinalBossWave = 7;
    public const int FirstWaveEnemies = 50;
    public const int WaveGrowthPercent = 20;
    public const float SpawnInterval = 0.04f;
    public const int StartingGold = 0;
    public const float MoveSpeed = 5f;

    public const int AttackPerUpgrade = 4;
    public const int HealthPerUpgrade = 12;

    public const int SwordAttack = 14;
    public const int StaffAttack = 16;
    public const int BowAttack = 11;
    public const int ArmorTier1Hp = 70;

    public const int SmallPotionHeal = 40;
    public const int MediumPotionHeal = 90;
    public const int LargePotionHeal = 180;

    public const string ArmorAssetName = "4 Giáp 1";
    public const string SmallPotionAsset = "7 Bình Máu Nhỏ";
    public const string MediumPotionAsset = "8 Bình Máu Vừa";
    public const string LargePotionAsset = "9 Bình Máu Lớn";

    public struct ClassBase
    {
        public int attack;
        public int hp;
        public float attackSpeed;
        public string displayName;
        public string weaponAssetName;
        public int weaponAttack;
    }

    public struct MapRule
    {
        public float enemyHpMultiplier;
        public float enemyDamageMultiplier;
        public float enemySpeedMultiplier;
        public int requiredWeaponUpgrade;
        public int requiredArmorUpgrade;
        public int requiredAttack;
        public int requiredHp;
        public int goldReward;
        public string defaultName;
    }

    public static int EnemiesForWave(int wave)
    {
        float growth = 1f + WaveGrowthPercent / 100f;
        return Mathf.RoundToInt(FirstWaveEnemies * Mathf.Pow(growth, Mathf.Max(0, wave - 1)));
    }

    public static ClassBase GetClass(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Archer:
                return new ClassBase
                {
                    attack = 10,
                    hp = 130,
                    attackSpeed = 1.6f,
                    displayName = "Cung Thủ",
                    weaponAssetName = "3 Vũ Khí 3",
                    weaponAttack = BowAttack
                };
            case PlayerClass.Mage:
                return new ClassBase
                {
                    attack = 20,
                    hp = 100,
                    attackSpeed = 0.75f,
                    displayName = "Pháp Sư",
                    weaponAssetName = "2 Vũ Khí 2",
                    weaponAttack = StaffAttack
                };
            default:
                return new ClassBase
                {
                    attack = 12,
                    hp = 220,
                    attackSpeed = 0.9f,
                    displayName = "Đấu Sĩ",
                    weaponAssetName = "1 Vũ Khí 1",
                    weaponAttack = SwordAttack
                };
        }
    }

    public static MapRule GetMap(MapDifficulty difficulty)
    {
        switch (difficulty)
        {
            case MapDifficulty.Normal:
                return new MapRule
                {
                    enemyHpMultiplier = 1.5f,
                    enemyDamageMultiplier = 1.3f,
                    enemySpeedMultiplier = 1.06f,
                    requiredWeaponUpgrade = 3,
                    requiredArmorUpgrade = 3,
                    requiredAttack = 32,
                    requiredHp = 200,
                    goldReward = 1600,
                    defaultName = "Map Thường"
                };
            case MapDifficulty.Hard:
                return new MapRule
                {
                    enemyHpMultiplier = 2.1f,
                    enemyDamageMultiplier = 1.6f,
                    enemySpeedMultiplier = 1.12f,
                    requiredWeaponUpgrade = 7,
                    requiredArmorUpgrade = 7,
                    requiredAttack = 48,
                    requiredHp = 250,
                    goldReward = 2800,
                    defaultName = "Map Khó"
                };
            case MapDifficulty.Hell:
                return new MapRule
                {
                    enemyHpMultiplier = 2.8f,
                    enemyDamageMultiplier = 1.9f,
                    enemySpeedMultiplier = 1.2f,
                    requiredWeaponUpgrade = 9,
                    requiredArmorUpgrade = 9,
                    requiredAttack = 56,
                    requiredHp = 275,
                    goldReward = 4500,
                    defaultName = "Map Địa Ngục"
                };
            default:
                return new MapRule
                {
                    enemyHpMultiplier = 1f,
                    enemyDamageMultiplier = 1f,
                    enemySpeedMultiplier = 1f,
                    requiredWeaponUpgrade = 0,
                    requiredArmorUpgrade = 0,
                    requiredAttack = 0,
                    requiredHp = 0,
                    goldReward = 800,
                    defaultName = "Map Dễ"
                };
        }
    }

    public static int StarterAttack(PlayerClass playerClass)
    {
        ClassBase stats = GetClass(playerClass);
        return stats.attack + stats.weaponAttack;
    }

    public static int StarterHp(PlayerClass playerClass)
    {
        return GetClass(playerClass).hp + ArmorTier1Hp;
    }
}
