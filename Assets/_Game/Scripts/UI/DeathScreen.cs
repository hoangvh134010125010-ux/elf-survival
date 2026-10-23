using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance.gameObject.activeSelf;

    private Text detailText;

    public static void Show()
    {
        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            ReturnToMapSelect();
            return;
        }

        if (Instance == null) Build(canvas);

        Instance.gameObject.SetActive(true);
        Instance.transform.SetAsLastSibling();
        Instance.RefreshDetail();
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void RefreshDetail()
    {
        if (detailText == null) return;

        string mapName = "";
        int wave = 1;
        int maxWaves = CombatBalance.WavesPerMap;
        if (MapManager.Instance != null)
        {
            if (MapManager.Instance.CurrentMap != null) mapName = MapManager.Instance.CurrentMap.mapName;
            wave = MapManager.Instance.currentWave;
            maxWaves = MapManager.Instance.maxWaves;
        }

        int gold = PlayerStats.Instance != null ? PlayerStats.Instance.gold : 0;
        detailText.text = $"{mapName}\nGục ngã ở lượt {wave}/{maxWaves}\nVàng hiện có: {gold:N0}";
    }

    private static Canvas FindHudCanvas()
    {
        if (HUDManager.Instance != null)
        {
            Canvas hudCanvas = HUDManager.Instance.GetComponentInParent<Canvas>();
            if (hudCanvas != null) return hudCanvas.rootCanvas;
        }

        return Object.FindAnyObjectByType<Canvas>();
    }

    private static void Build(Canvas canvas)
    {
        Font font = PickFont();
        Sprite frameSprite = PickFrameSprite();

        GameObject overlay = new GameObject("Death Screen", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        Stretch(overlayRect);
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        Instance = overlay.AddComponent<DeathScreen>();

        GameObject frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
        frame.transform.SetParent(overlay.transform, false);
        RectTransform frameRect = frame.GetComponent<RectTransform>();
        frameRect.anchorMin = frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.sizeDelta = new Vector2(980f, 760f);
        Image frameImage = frame.GetComponent<Image>();
        if (frameSprite != null)
        {
            frameImage.sprite = frameSprite;
            frameImage.type = Image.Type.Sliced;
        }
        else
        {
            frameImage.color = new Color(0.16f, 0.1f, 0.1f, 0.96f);
        }

        Text title = CreateText(frame.transform, "Title", font, "BẠN ĐÃ TỬ TRẬN", 84, new Color(1f, 0.32f, 0.28f));
        Place(title.rectTransform, new Vector2(0f, 230f), new Vector2(900f, 140f));

        Instance.detailText = CreateText(frame.transform, "Detail", font, "", 44, Color.white);
        Place(Instance.detailText.rectTransform, new Vector2(0f, 40f), new Vector2(880f, 220f));

        Button mapButton = CreateButton(frame.transform, "Map Select Button", font, "VỀ CHỌN MAP", new Color(0.75f, 0.2f, 0.18f));
        Place((RectTransform)mapButton.transform, new Vector2(-220f, -230f), new Vector2(400f, 130f));
        mapButton.onClick.AddListener(ReturnToMapSelect);

        Button retryButton = CreateButton(frame.transform, "Retry Button", font, "CHƠI LẠI", new Color(0.25f, 0.45f, 0.3f));
        Place((RectTransform)retryButton.transform, new Vector2(220f, -230f), new Vector2(400f, 130f));
        retryButton.onClick.AddListener(Retry);
    }

    private static void ReturnToMapSelect()
    {
        AudioManager.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        PlayerStats.Instance?.SaveCurrentPlayerData(true);
        SceneManager.LoadScene(SceneNames.MapSelect);
    }

    private static void Retry()
    {
        AudioManager.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        PlayerStats.Instance?.SaveCurrentPlayerData(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static Font PickFont()
    {
        if (HUDManager.Instance != null && HUDManager.Instance.atkText != null && HUDManager.Instance.atkText.font != null)
        {
            return HUDManager.Instance.atkText.font;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static Sprite PickFrameSprite()
    {
        if (HUDManager.Instance == null || HUDManager.Instance.settingsPanel == null) return null;
        Image panelImage = HUDManager.Instance.settingsPanel.GetComponent<Image>();
        return panelImage != null ? panelImage.sprite : null;
    }

    private static Text CreateText(Transform parent, string objectName, Font font, string content, int size, Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string objectName, Font font, string label, Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText(go.transform, "Label", font, label, 44, Color.white);
        Stretch(text.rectTransform);
        return button;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
