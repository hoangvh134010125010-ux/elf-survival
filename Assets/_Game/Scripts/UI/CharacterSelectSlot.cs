using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public Text characterNameText;
    public Text characterAtkText;
    public Text characterHpText;
    public Text characterAtkSpeedText;
    public Button selectButton;

    [Header("Character Config")]
    public PlayerClass characterClass;
    public string characterName = "Chiến Binh";
    public Sprite avatarSprite;
    public int baseAtk = 50;
    public int baseHp = 500;
    public float baseAtkSpeed = 1.0f;
    public int startingGold = CombatBalance.StartingGold;

    private void Start()
    {
        UpdateUI();

        if (selectButton == null) selectButton = GetComponent<Button>();
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectCharacter);
        }
    }

    private void UpdateUI()
    {
        CombatBalance.ClassBase stats = CombatBalance.GetClass(characterClass);
        if (characterImage != null && avatarSprite != null) characterImage.sprite = avatarSprite;
        if (characterNameText != null)
        {
            characterNameText.text = string.IsNullOrWhiteSpace(characterName) ? stats.displayName : characterName;
        }
        if (characterAtkText != null) characterAtkText.text = $"ATK: {CombatBalance.StarterAttack(characterClass)}";
        if (characterHpText != null) characterHpText.text = $"HP: {CombatBalance.StarterHp(characterClass)}";
        if (characterAtkSpeedText != null) characterAtkSpeedText.text = $"ATK SPD: {stats.attackSpeed:F2}";
    }

    private void OnSelectCharacter()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        int targetSlot = -1;
        for (int i = 1; i <= 3; i++)
        {
            if (!SaveSystem.GetSaveSlot(i).isOccupied)
            {
                targetSlot = i;
                break;
            }
        }

        if (targetSlot == -1)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("<color=red>Bộ nhớ lưu đã đầy! Hãy vào Load Game xóa bớt Slot cũ.</color>");
            }
            return;
        }

        CombatBalance.ClassBase stats = CombatBalance.GetClass(characterClass);
        SaveData newData = new SaveData
        {
            characterName = string.IsNullOrWhiteSpace(characterName) ? stats.displayName : characterName,
            characterClass = (int)characterClass,
            gold = startingGold,
            gearInitialized = true,
            equippedWeapon = new InventoryItemSaveData(stats.weaponAssetName, 1, 0),
            equippedArmor = new InventoryItemSaveData(CombatBalance.ArmorAssetName, 1, 0)
        };
        newData.bagItems.Add(new InventoryItemSaveData(CombatBalance.SmallPotionAsset, 10, 0));
        newData.bagItems.Add(new InventoryItemSaveData(CombatBalance.MediumPotionAsset, 5, 0));
        newData.bagItems.Add(new InventoryItemSaveData(CombatBalance.LargePotionAsset, 3, 0));

        SaveSystem.SaveSlot(targetSlot, newData, true);

        PlayerPrefs.SetInt("CurrentSlot", targetSlot);
        PlayerPrefs.SetInt("SelectedClass", (int)characterClass);
        PlayerPrefs.Save();

        NotificationManager.Instance?.ShowNotification($"Đã tạo nhân vật vào Slot {targetSlot}!");
        if (MainMenuManager.Instance != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MapSelect);
        }
    }
}