using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Main")]
    public GameObject settingsPanel;
    public Button closeButton;
    public Button buttonContinue;
    public Button buttonReplay;
    public Button buttonMusic;
    public Button buttonMainMenu;
    public Button buttonExit;

    [Header("Music")]
    public GameObject musicPanel;
    public Button closeMusicButton;

    [Header("Audio Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (buttonContinue != null) buttonContinue.onClick.AddListener(CloseSettings);
        if (buttonReplay != null) buttonReplay.onClick.AddListener(OnReplayClicked);
        if (buttonMusic != null) buttonMusic.onClick.AddListener(OnMusicClicked);
        if (buttonMainMenu != null) buttonMainMenu.onClick.AddListener(OnMainMenuClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);
        if (buttonExit == null) buttonExit = FindButtonByLabel(settingsPanel, "Exit");
        if (buttonExit != null) buttonExit.onClick.AddListener(OnExitClicked);
        if (closeMusicButton != null) closeMusicButton.onClick.AddListener(CloseMusicPanel);

        // Load Volume
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.6f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 1f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        if (voiceSlider != null)
        {
            voiceSlider.value = PlayerPrefs.GetFloat("VoiceVol", 1f);
            voiceSlider.onValueChanged.AddListener(SetVoiceVolume);
        }

        if (musicPanel != null) musicPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (musicPanel != null) musicPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnReplayClicked()
    {
        Time.timeScale = 1f;
        PlayerStats.Instance?.SaveCurrentPlayerData(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMusicClicked()
    {
        if (musicPanel != null) musicPanel.SetActive(true);
    }

    private void CloseMusicPanel()
    {
        if (musicPanel != null) musicPanel.SetActive(false);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        PlayerStats.Instance?.SaveCurrentPlayerData(true);
        SceneManager.LoadScene(IsGameplayScene() ? SceneNames.MapSelect : SceneNames.MainMenu);
    }

    private static Button FindButtonByLabel(GameObject root, string label)
    {
        if (root == null) return null;
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null && text.text.Trim() == label) return button;
        }
        return null;
    }

    private void OnExitClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameExit.Quit();
    }

    private static bool IsGameplayScene()
    {
        return MapManager.Instance != null || (HUDManager.Instance != null && HUDManager.Instance.isGameplayScene);
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
    }

    public void SetVoiceVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetVoiceVolume(value);
    }
}