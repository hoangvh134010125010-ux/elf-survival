using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject musicPanel;
    public GameObject newGamePanel;
    public GameObject loadPanel;
    public GameObject mapSelectPanel;

    [Header("Start Panel Buttons")]
    public Button newGameButton;
    public Button loadButton;
    public Button instructButton;
    public Button musicButton;
    public Button exitButton;

    [Header("Close Buttons")]
    public Button closeMusicButton;
    public Button closeNewGameButton;
    public Button closeLoadButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OpenNewGamePanel);
        if (loadButton != null) loadButton.onClick.AddListener(OpenLoadPanel);
        if (musicButton != null) musicButton.onClick.AddListener(OpenMusicPanel);
        if (exitButton != null) exitButton.onClick.AddListener(QuitGame);

        if (closeMusicButton != null) closeMusicButton.onClick.AddListener(ShowStartPanel);
        if (closeNewGameButton != null) closeNewGameButton.onClick.AddListener(ShowStartPanel);
        if (closeLoadButton != null) closeLoadButton.onClick.AddListener(ShowStartPanel);

        ShowStartPanel();
    }

    public void ShowStartPanel()
    {
        SetPanelActive(startPanel);
    }

    public void OpenNewGamePanel()
    {
        SetPanelActive(newGamePanel);
    }

    public void OpenLoadPanel()
    {
        SetPanelActive(loadPanel);

        
        SaveSlotUI[] slots = loadPanel.GetComponentsInChildren<SaveSlotUI>(true);
        foreach (var slot in slots) slot.RefreshUI();
    }

    public void OpenMusicPanel()
    {
        SetPanelActive(musicPanel);
    }

    private void SetPanelActive(GameObject targetPanel)
    {
        if (startPanel != null) startPanel.SetActive(startPanel == targetPanel);
        if (musicPanel != null) musicPanel.SetActive(musicPanel == targetPanel);
        if (newGamePanel != null) newGamePanel.SetActive(newGamePanel == targetPanel);
        if (loadPanel != null) loadPanel.SetActive(loadPanel == targetPanel);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(mapSelectPanel == targetPanel);
    }
    public void QuitGame()
    {
        GameExit.Quit();
    }
}