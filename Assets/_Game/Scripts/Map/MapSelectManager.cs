using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapSelectManager : MonoBehaviour
{
    public static MapSelectManager Instance;

    [Header("Panels")]
    public GameObject mapSelectPanel;

    [Header("UI References")]
    public Button openMapSelectButton;
    public Button closeMapSelectButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (openMapSelectButton != null) openMapSelectButton.onClick.AddListener(ToggleMapSelectPanel);
        if (closeMapSelectButton != null) closeMapSelectButton.onClick.AddListener(CloseMapSelectPanel);

        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
    }

    public void ToggleMapSelectPanel()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (mapSelectPanel != null)
        {
            bool isActive = mapSelectPanel.activeSelf;
            if (HUDManager.Instance != null) HUDManager.Instance.CloseAllPanels();
            mapSelectPanel.SetActive(!isActive);
        }
    }

    public void CloseMapSelectPanel()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
    }
}