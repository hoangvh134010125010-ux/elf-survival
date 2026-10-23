using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItemSaveData
{
    public string itemID;
    public string itemName;
    public int quantity;
    public int upgradeLevel;

    public InventoryItemSaveData()
    {
    }

    public InventoryItemSaveData(string id, int qty, int lvl, string name = "")
    {
        itemID = id;
        itemName = name;
        quantity = qty;
        upgradeLevel = lvl;
    }
}

[System.Serializable]
public class SaveData
{
    public bool isOccupied = false;
    public string characterName = "Chiến Binh";
    public int characterClass = 0;
    public int gold = 0;
    public string saveTime = "";

    public bool gearInitialized;
    public InventoryItemSaveData equippedWeapon;
    public InventoryItemSaveData equippedArmor;
    public List<InventoryItemSaveData> bagItems = new List<InventoryItemSaveData>();
}

public static class SaveSystem
{
    private static bool pendingDiskFlush;

    public static SaveData GetSaveSlot(int slotIndex)
    {
        string json = PlayerPrefs.GetString($"SaveSlot_{slotIndex}", "");
        if (string.IsNullOrEmpty(json))
        {
            return new SaveData { isOccupied = false };
        }
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void SaveSlot(int slotIndex, SaveData data, bool flushNow = false)
    {
        data.isOccupied = true;
        data.saveTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        PlayerPrefs.SetString($"SaveSlot_{slotIndex}", JsonUtility.ToJson(data));
        pendingDiskFlush = true;
        SaveFlushHost.Ensure();

        if (flushNow)
        {
            FlushToDisk();
        }
    }

    public static void DeleteSlot(int slotIndex)
    {
        PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}");
        pendingDiskFlush = true;
        SaveFlushHost.Ensure();
        FlushToDisk();
    }

    public static void FlushToDisk()
    {
        if (!pendingDiskFlush) return;
        PlayerPrefs.Save();
        pendingDiskFlush = false;
    }
}

public sealed class SaveFlushHost : MonoBehaviour
{
    private static SaveFlushHost instance;
    private float timer;

    public static void Ensure()
    {
        if (instance != null) return;

        GameObject host = new GameObject(nameof(SaveFlushHost));
        DontDestroyOnLoad(host);
        instance = host.AddComponent<SaveFlushHost>();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < 1.5f) return;
        timer = 0f;
        SaveSystem.FlushToDisk();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.FlushToDisk();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.FlushToDisk();
    }
}
