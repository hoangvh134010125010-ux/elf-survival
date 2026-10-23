using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Map Config")]
    public List<MapSO> mapList;
    public int currentMapIndex = 0;

    [Header("Wave Settings")]
    public int currentWave = 1;
    public int maxWaves = CombatBalance.WavesPerMap;
    public float waveBreakTime = 2.2f;
    public float minSpawnDistance = 8f;

    private const float SpawnEdgeMargin = 1.5f;

    private readonly List<Enemy> waveEnemies = new List<Enemy>(160);
    private bool isWaveSpawning;
    private bool mapCleared;
    private Coroutine spawnRoutine;

    public MapSO CurrentMap =>
        mapList != null && currentMapIndex >= 0 && currentMapIndex < mapList.Count
            ? mapList[currentMapIndex]
            : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        maxWaves = CombatBalance.WavesPerMap;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        currentMapIndex = PlayerPrefs.GetInt("SelectedMapIndex", currentMapIndex);
        int mapCount = mapList != null ? mapList.Count : 0;
        currentMapIndex = Mathf.Clamp(currentMapIndex, 0, Mathf.Max(0, mapCount - 1));
        BuildArena();
        StartMap(currentMapIndex);
    }

    private void BuildArena()
    {
        Vector2 center = Vector2.zero;
        if (PlayerController.Instance != null)
        {
            center = PlayerController.Instance.transform.position;
        }

        ArenaBounds.Create(center, CurrentMap, currentMapIndex);
        ArenaCamera.Attach();
    }

    public void StartMap(int mapIndex)
    {
        StopSpawnRoutine();
        DespawnWave();
        mapCleared = false;
        maxWaves = CombatBalance.WavesPerMap;

        if (mapList == null || mapList.Count == 0 || mapIndex < 0 || mapIndex >= mapList.Count)
        {
            Debug.LogWarning("[MapManager] Thiếu cấu hình MapSO hoặc mapIndex không hợp lệ.");
            return;
        }

        currentMapIndex = mapIndex;
        currentWave = 1;
        isWaveSpawning = false;
        mapList[currentMapIndex].EnsureDefaults();
        RefreshMapHud();
        spawnRoutine = StartCoroutine(SpawnWaveRoutine());
    }

    public void StopWaves()
    {
        mapCleared = true;
        StopSpawnRoutine();
    }

    private void StopSpawnRoutine()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        isWaveSpawning = false;
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isWaveSpawning = true;
        RefreshMapHud();

        bool isFinalBoss = currentWave == CombatBalance.FinalBossWave;
        bool isMiniBoss = currentWave == CombatBalance.MiniBossWave;

        NotificationManager.Instance?.ShowNotification(
            $"<color=cyan>LƯỢT {currentWave}/{maxWaves} BẮT ĐẦU SAU {waveBreakTime:0} GIÂY</color>");
        yield return new WaitForSeconds(waveBreakTime);

        if (mapCleared || CurrentMap == null)
        {
            isWaveSpawning = false;
            spawnRoutine = null;
            yield break;
        }

        MapSO currentMap = CurrentMap;
        currentMap.EnsureDefaults();

        float waveHpBonus = 1f + (currentWave - 1) * 0.08f;
        float waveDamageBonus = 1f + (currentWave - 1) * 0.05f;
        float hpMultiplier = currentMap.enemyHpMultiplier * waveHpBonus;
        float damageMultiplier = currentMap.enemyDamageMultiplier * waveDamageBonus;
        float speedMultiplier = currentMap.enemySpeedMultiplier;

        int smallCount = CombatBalance.EnemiesForWave(currentWave);

        if (isFinalBoss)
        {
            NotificationManager.Instance?.ShowNotification("<color=red>BOSS CUỐI XUẤT HIỆN!</color>");
            SpawnEnemy(currentMap.bossPrefab, hpMultiplier * 1.85f, damageMultiplier * 1.4f, speedMultiplier, 2);
        }
        else if (isMiniBoss)
        {
            NotificationManager.Instance?.ShowNotification("<color=yellow>MINI BOSS XUẤT HIỆN!</color>");
            SpawnEnemy(currentMap.bossPrefab, hpMultiplier * 1.5f, damageMultiplier * 1.25f, speedMultiplier, 1);
        }
        else
        {
            NotificationManager.Instance?.ShowNotification($"<color=cyan>LƯỢT {currentWave}/{maxWaves}: {smallCount} QUÁI</color>");
        }

        int spawned = waveEnemies.Count;
        WaitForSeconds spawnGap = new WaitForSeconds(CombatBalance.SpawnInterval);
        for (int i = 0; i < smallCount; i++)
        {
            if (mapCleared) break;

            if (SpawnEnemy(GetRandomSmallEnemyPrefab(currentMap), hpMultiplier, damageMultiplier, speedMultiplier, 0))
            {
                spawned++;
            }
            yield return spawnGap;
        }

        isWaveSpawning = false;
        spawnRoutine = null;
        RefreshMapHud();

        if (mapCleared || waveEnemies.Count > 0) yield break;

        if (spawned == 0)
        {
            Debug.LogWarning("[MapManager] Lượt không spawn được quái. Kiểm tra prefab trong MapSO.");
        }
        EndWave();
    }

    private bool SpawnEnemy(GameObject enemyPrefab, float hpMultiplier, float damageMultiplier, float speedMultiplier, int bossRank)
    {
        if (enemyPrefab == null) return false;
        Enemy spawned = SpawnEnemyInstance(enemyPrefab, hpMultiplier, damageMultiplier, speedMultiplier, bossRank);
        if (spawned == null) return false;

        if (bossRank > 0)
        {
            BossHealthUI.Track(spawned, bossRank == 2 ? "BOSS CUỐI" : "MINI BOSS");
        }
        return true;
    }

    private Enemy SpawnEnemyInstance(GameObject enemyPrefab, float hpMultiplier, float damageMultiplier, float speedMultiplier, int bossRank)
    {
        Vector3 position = GetSpawnPosition();
        GameObject enemyObj = SimpleObjectPool.Spawn(enemyPrefab, position, Quaternion.identity);
        if (enemyObj == null) return null;

        Enemy enemyScript = enemyObj.GetComponent<Enemy>();
        if (enemyScript == null)
        {
            SimpleObjectPool.Despawn(enemyObj);
            return null;
        }

        enemyScript.ApplyStatMultiplier(hpMultiplier, damageMultiplier, speedMultiplier, bossRank);
        if (!waveEnemies.Contains(enemyScript))
        {
            waveEnemies.Add(enemyScript);
        }
        return enemyScript;
    }

    private Vector3 GetSpawnPosition()
    {
        Vector2 player = PlayerController.Instance != null ? (Vector2)PlayerController.Instance.transform.position : Vector2.zero;
        Rect area = ArenaBounds.Instance != null
            ? ArenaBounds.Instance.Area
            : new Rect(player.x - 18f, player.y - 10f, 36f, 20f);

        float minX = area.xMin + SpawnEdgeMargin;
        float maxX = area.xMax - SpawnEdgeMargin;
        float minY = area.yMin + SpawnEdgeMargin;
        float maxY = area.yMax - SpawnEdgeMargin;
        float minDistanceSqr = minSpawnDistance * minSpawnDistance;

        Vector2 best = player;
        float bestDistanceSqr = -1f;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            Vector2 candidate = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            float distanceSqr = (candidate - player).sqrMagnitude;
            if (distanceSqr >= minDistanceSqr) return candidate;
            if (distanceSqr > bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                best = candidate;
            }
        }

        return best;
    }

    private GameObject GetRandomSmallEnemyPrefab(MapSO map)
    {
        GameObject picked = PickValidPrefab(map);
        if (picked != null) return picked;

        if (mapList == null) return null;

        for (int i = 0; i < mapList.Count; i++)
        {
            picked = PickValidPrefab(mapList[i]);
            if (picked != null) return picked;
        }

        return null;
    }

    private static GameObject PickValidPrefab(MapSO map)
    {
        if (map == null || map.smallEnemyPrefabs == null) return null;

        List<GameObject> valid = map.smallEnemyPrefabs.FindAll(prefab => prefab != null);
        if (valid.Count == 0) return null;
        return valid[Random.Range(0, valid.Count)];
    }

    public void OnEnemyDied(Enemy enemy)
    {
        if (mapCleared || enemy == null) return;
        if (!waveEnemies.Remove(enemy)) return;
        RefreshMapHud();

        if (waveEnemies.Count > 0 || isWaveSpawning)
        {
            if (enemy.BossRank == 2)
            {
                NotificationManager.Instance?.ShowNotification("<color=red>ĐÃ HẠ BOSS!</color>");
            }
            else if (enemy.BossRank == 1)
            {
                NotificationManager.Instance?.ShowNotification("<color=yellow>ĐÃ HẠ MINI BOSS!</color>");
            }

            return;
        }

        EndWave();
    }

    private void EndWave()
    {
        if (mapCleared) return;

        if (currentWave >= maxWaves)
        {
            HandleMapCleared();
            return;
        }

        if (currentWave == CombatBalance.MiniBossWave)
        {
            NotificationManager.Instance?.ShowNotification("<color=yellow>HẾT LƯỢT MINI BOSS!</color>");
        }
        else
        {
            NotificationManager.Instance?.ShowNotification($"<color=lime>ĐÃ HẾT LƯỢT {currentWave}!</color>");
        }

        currentWave++;
        RefreshMapHud();
        spawnRoutine = StartCoroutine(SpawnWaveRoutine());
    }

    private void HandleMapCleared()
    {
        if (mapCleared) return;
        mapCleared = true;
        StopSpawnRoutine();
        DespawnWave();

        MapSO currentMap = CurrentMap;
        if (currentMap == null) return;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddGold(currentMap.goldReward);
        }

        if (currentMap.itemDrops != null)
        {
            for (int i = 0; i < currentMap.itemDrops.Count; i++)
            {
                InventoryManager.GetOrCreate().AddItemToBag(currentMap.itemDrops[i], 1);
            }
        }

        PlayerStats.Instance?.SaveCurrentPlayerData(true);
        NotificationManager.Instance?.ShowNotification(
            $"<color=red>HẾT LƯỢT BOSS!</color>\n<color=green>HOÀN THÀNH {currentMap.mapName}!</color>");
        StartCoroutine(ReturnToMapSelect());
    }

    private void DespawnWave()
    {
        for (int i = waveEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = waveEnemies[i];
            if (enemy != null)
            {
                SimpleObjectPool.Despawn(enemy.gameObject);
            }
        }

        waveEnemies.Clear();
    }

    private void RefreshMapHud()
    {
        MapSO map = CurrentMap;
        if (HUDManager.Instance != null && map != null)
        {
            string title = string.IsNullOrEmpty(map.styleName) ? map.mapName : $"{map.mapName} - {map.styleName}";
            int left = isWaveSpawning && waveEnemies.Count == 0 ? -1 : waveEnemies.Count;
            HUDManager.Instance.UpdateMapUI(title, currentWave, maxWaves, left);
        }
    }

    public int RollGold(int bossRank)
    {
        int difficulty = CurrentMap != null ? (int)CurrentMap.difficulty : 0;
        switch (bossRank)
        {
            case 2: return 150 + difficulty * 100;
            case 1: return 80 + difficulty * 50;
            default: return 3 + difficulty * 3;
        }
    }

    public int RollExp(int bossRank)
    {
        return bossRank > 0 ? 15 : 1;
    }

    public int CurrentDifficulty => CurrentMap != null ? (int)CurrentMap.difficulty : 0;

    private IEnumerator ReturnToMapSelect()
    {
        yield return new WaitForSeconds(2.4f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneNames.MapSelect);
    }
}
