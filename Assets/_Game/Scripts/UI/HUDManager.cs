using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;
    [Header("Mode Config")]
    public bool isGameplayScene = false;

    [Header("Top Panel UI")]
    public Text playerGoldText;
    public Text playerLevelText;
    public Text playerClassText;
    public Image playerClassIcon;
    public Slider playerExpBar;

    [Header("Class Icons")]
    public Sprite warriorIcon;
    public Sprite mageIcon;
    public Sprite archerIcon;


    [Header("Bot Panel Stats")]
    public Text atkText;
    public Text hpText;
    public Text atkSpeedText;

   [Header("Buttons (Cụm Nút Hành Động)")]
    public Button shopButton;
    public Button inventoryButton;
    public Button upgradeButton;

    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject upgradePanel;
    public GameObject inventoryPanel;
    public GameObject levelUpPanel;
    public GameObject settingsPanel;
    public GameObject musicPanel;

    [Header("Map UI")]
    public Text mapInfoText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CloseAllPanels();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        CloseAllPanels();
      if (isGameplayScene)
        {
            SetButtonInteractable(shopButton, false);
            SetButtonInteractable(inventoryButton, false);
            SetButtonInteractable(upgradeButton, false);
            MakePanelTransparent("Top Panel");
            MakePanelTransparent("Bot Panel");
        }
        else // NẾU LÀ SẢNH (LOBBY): Mở tương tác bình thường
        {
            SetButtonInteractable(shopButton, true);
            SetButtonInteractable(inventoryButton, true);
            SetButtonInteractable(upgradeButton, true);
        }

        RefreshHUDData();
    }
    public void RefreshHUDData()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.UpdateAllUI();
        }
    }
    
    private void MakePanelTransparent(string panelName)
    {
        Transform panel = null;
        foreach (Transform child in transform)
        {
            if (child.name == panelName)
            {
                panel = child;
                break;
            }
        }
        if (panel == null) return;

        Image background = panel.GetComponent<Image>();
        if (background != null)
        {
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
            background.raycastTarget = false;
        }

        foreach (Text text in panel.GetComponentsInChildren<Text>(true))
        {
            if (text.GetComponent<Outline>() != null) continue;
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
        }
    }

    private void SetButtonInteractable(Button btn, bool interactable)
    {
        if (btn != null)
        {
            btn.interactable = interactable;

            CanvasGroup canvasGroup = btn.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = btn.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = interactable ? 1f : 0.3f;
        }
    }
    public void UpdateMapUI(string mapName, int currentWave, int maxWaves = 7, int enemiesLeft = -1)
    {
        if (mapInfoText != null)
        {
            mapInfoText.text = enemiesLeft >= 0
                ? $"{mapName} - Lượt {currentWave}/{maxWaves} - Còn {enemiesLeft} quái"
                : $"{mapName} - Lượt {currentWave}/{maxWaves}";
        }
    }


    public void CloseAllPanels()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (musicPanel != null) musicPanel.SetActive(false);
    }

    public void ToggleShopPanel()
    {
        if (isGameplayScene) return;
        PlayButtonSFX();
        bool isActive = shopPanel != null && shopPanel.activeSelf;
        CloseAllPanels();
        if (shopPanel != null) shopPanel.SetActive(!isActive);
    }

    public void ToggleUpgradePanel()
    {
        if (isGameplayScene) return;
        PlayButtonSFX();
        bool isActive = upgradePanel != null && upgradePanel.activeSelf;
        CloseAllPanels();
        if (upgradePanel != null) upgradePanel.SetActive(!isActive);
    }

    public void ToggleInventoryPanel()
    {
        if (isGameplayScene) return;
        PlayButtonSFX();
        bool isActive = inventoryPanel != null && inventoryPanel.activeSelf;
        CloseAllPanels();
        if (inventoryPanel != null) inventoryPanel.SetActive(!isActive);
    }

    public void ToggleSettingsPanel()
    {
        PlayButtonSFX();
        bool willOpen = settingsPanel != null && !settingsPanel.activeSelf;
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(willOpen);
        }

        if (willOpen)
        {
            Time.timeScale = 0f;
            return;
        }

        bool levelUpOpen = LevelUpManager.Instance != null && LevelUpManager.Instance.IsOpen;
        if (!levelUpOpen)
        {
            Time.timeScale = 1f;
        }
    }

    public void ToggleMusicPanel()
    {
        PlayButtonSFX();
        if (musicPanel != null)
        {
            musicPanel.SetActive(!musicPanel.activeSelf);
        }
    }

    public void ShowLevelupPanel()
    {
        PlayButtonSFX();
        CloseAllPanels();
        if (LevelUpManager.Instance != null)
        {
            LevelUpManager.Instance.OpenLevelUpPanel();
        }
        else if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }
    }

    private void PlayButtonSFX()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }


    public void UpdateClass(PlayerClass pClass)
    {
        string className = "";
        Sprite selectedSprite = null;

        switch (pClass)
        {
            case PlayerClass.Warrior:
                className = "Đấu Sĩ";
                selectedSprite = warriorIcon;
                break;

            case PlayerClass.Mage:
                className = "Pháp Sư";
                selectedSprite = mageIcon;
                break;

            case PlayerClass.Archer:
                className = "Cung Thủ";
                selectedSprite = archerIcon;
                break;

        }

        if (playerClassText != null)
        {
            playerClassText.text = "Class: " + className;
        }

        if (playerClassIcon != null)
        {
            if (selectedSprite != null)
            {
                playerClassIcon.sprite = selectedSprite;
                playerClassIcon.enabled = true;
            }
            else
            {
                playerClassIcon.enabled = false;
            }
        }
    }

    public void UpdateGold(int gold)
    {
        if (playerGoldText != null)
        {
            playerGoldText.text = "Gold: " + gold.ToString("N0"); // Tự động định dạng dấu phẩy (vd: 1,000,000)
        }
    }

    public void UpdateLevel(int level)
    {
        if (playerLevelText != null)
        {
            playerLevelText.text = "Level: " + level.ToString();
        }
    }

    public void UpdateExp(float currentExp, float maxExp)
    {
        if (playerExpBar != null)
        {
            playerExpBar.maxValue = maxExp;
            playerExpBar.value = currentExp;
        }
    }

    public void UpdatePlayerStats(float atk, float currentHp, float maxHp, float atkSpeed)
    {
        if (atkText != null) atkText.text = "ATK: " + Mathf.RoundToInt(atk).ToString();
        if (hpText != null) hpText.text = $"HP: {Mathf.RoundToInt(currentHp)}/{Mathf.RoundToInt(maxHp)}";
        if (atkSpeedText != null) atkSpeedText.text = "ATK SPD: " + atkSpeed.ToString("F2");
    }
}