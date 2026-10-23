using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapButtonUI : MonoBehaviour
{
    [Header("Cấu Hình Scene Màn Chơi")]
    public string targetSceneName = SceneNames.Gameplay;
    public int mapIndex;
    public MapSO mapConfig;
    public Button mapButton;

    private void Start()
    {
        if (mapButton == null) mapButton = GetComponent<Button>();

        if (mapButton != null)
        {
            mapButton.onClick.RemoveAllListeners();
            mapButton.onClick.AddListener(OnMapButtonClicked);
        }

        ShowRequirement();
    }

    private void ShowRequirement()
    {
        MapSO map = ResolveMap();
        if (map == null) return;

        Transform existing = transform.Find("RequirementLabel");
        Text label;
        if (existing == null)
        {
            GameObject labelObject = new GameObject("RequirementLabel", typeof(RectTransform));
            labelObject.transform.SetParent(transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.08f, 0.04f);
            labelRect.anchorMax = new Vector2(0.92f, 0.34f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label = labelObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (label.font == null) label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 30;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.92f, 0.55f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
        }
        else
        {
            label = existing.GetComponent<Text>();
        }

        if (label != null)
        {
            label.text = string.IsNullOrEmpty(map.styleName)
                ? map.RequirementText()
                : $"{map.styleName}  •  {map.RequirementText()}";
        }
    }

    private void OnMapButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        MapSO map = ResolveMap();
        if (map != null)
        {
            map.EnsureDefaults();
            if (!map.CanEnter(out string failMessage))
            {
                NotificationManager.Instance?.ShowNotification($"<color=red>{failMessage}</color>");
                return;
            }

            targetSceneName = string.IsNullOrEmpty(map.sceneName) ? targetSceneName : map.sceneName;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            targetSceneName = SceneNames.Gameplay;
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.CloseAllPanels();
        }

        PlayerPrefs.SetInt("SelectedMapIndex", mapIndex);
        PlayerPrefs.SetString("CurrentMap", targetSceneName);
        PlayerStats.Instance?.SaveCurrentPlayerData(true);

        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"[MapButtonUI] Không tìm thấy Scene '{targetSceneName}' trong Build Settings.");
            NotificationManager.Instance?.ShowNotification($"<color=red>Lỗi: Chưa thêm Scene '{targetSceneName}' vào Build Settings!</color>");
        }
    }

    private MapSO ResolveMap()
    {
        if (mapConfig != null) return mapConfig;

        if (MapManager.Instance != null && MapManager.Instance.mapList != null &&
            mapIndex >= 0 && mapIndex < MapManager.Instance.mapList.Count)
        {
            return MapManager.Instance.mapList[mapIndex];
        }

        return null;
    }
}
