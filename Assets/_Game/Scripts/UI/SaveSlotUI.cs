using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot Config")]
    public int slotIndex = 1;

    [Header("UI References")]
    public Image characterImage;
    public Text characterNameText;
    public Text goldText;
    public Text saveTimeText;
    public Button deleteButton;
    public Button slotSelectButton;

    [Header("Avatar")]
    public Sprite warriorAvatar;
    public Sprite archerAvatar;
    public Sprite mageAvatar;

    private SaveData currentData;

    private void Start()
    {
        if (slotSelectButton == null) slotSelectButton = GetComponent<Button>();

        if (slotSelectButton != null)
        {
            slotSelectButton.onClick.RemoveAllListeners();
            slotSelectButton.onClick.AddListener(OnLoadSlotClicked);
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }
        RefreshUI();
    }
    public void RefreshUI()
{
    currentData = SaveSystem.GetSaveSlot(slotIndex);

    if (currentData != null && currentData.isOccupied)
    {
        if (characterNameText != null) characterNameText.text = currentData.characterName;
        if (goldText != null) goldText.text = $"Gold: {currentData.gold}";
        if (saveTimeText != null) saveTimeText.text = $"Time: {currentData.saveTime}";

        if (characterImage != null)
        {
            PlayerClass pClass = (PlayerClass)currentData.characterClass;
            Sprite targetSprite = null;

            switch (pClass)
            {
                case PlayerClass.Warrior:
                    targetSprite = warriorAvatar;
                    break;
                case PlayerClass.Archer:
                    targetSprite = archerAvatar;
                    break;
                case PlayerClass.Mage:
                    targetSprite = mageAvatar;
                    break;
            }

            if (targetSprite != null)
            {
                characterImage.sprite = targetSprite;
                characterImage.enabled = true;
            }
            else
            {
                characterImage.enabled = false; 
            }
        }

        if (deleteButton != null) deleteButton.gameObject.SetActive(true);
    }
    else
    {
        if (characterNameText != null) characterNameText.text = "Slot Trống";
        if (goldText != null) goldText.text = "Gold: 0";
        if (saveTimeText != null) saveTimeText.text = "--/--/----";
        if (characterImage != null) characterImage.enabled = false;
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
    }
}
    private void OnLoadSlotClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (currentData != null && currentData.isOccupied)
        {
            PlayerPrefs.SetInt("CurrentSlot", slotIndex);
            PlayerPrefs.SetInt("SelectedClass", currentData.characterClass);
            PlayerPrefs.Save();
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.LoadPlayerDataFromSave();
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MapSelect);
        }
        else
        {
            NotificationManager.Instance?.ShowNotification("Slot này đang trống! Hãy tạo game mới.");
        }
    }

    private void OnDeleteClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        SaveSystem.DeleteSlot(slotIndex);
        RefreshUI();
        NotificationManager.Instance?.ShowNotification($"Đã xóa dữ liệu Slot {slotIndex}!");
    }
}
