using UnityEngine;
using UnityEngine.UI;

public sealed class BossHealthUI : MonoBehaviour
{
    private static BossHealthUI instance;

    private Enemy boss;
    private int shownHealth = int.MinValue;
    private Image fill;
    private Text title;
    private Text value;
    private RectTransform rect;
    private readonly Vector3[] corners = new Vector3[4];

    public static void Track(Enemy enemy, string bossName)
    {
        if (enemy == null) return;
        if (instance == null && !Build()) return;

        instance.boss = enemy;
        instance.shownHealth = int.MinValue;
        instance.title.text = bossName;
        instance.gameObject.SetActive(true);
        instance.PlaceAboveBotPanel();
        instance.Refresh();
    }

    private static bool Build()
    {
        if (HUDManager.Instance == null) return false;
        Canvas canvas = HUDManager.Instance.GetComponentInParent<Canvas>();
        if (canvas == null) return false;
        canvas = canvas.rootCanvas;
        Font font = RuntimeSprites.UIFont(HUDManager.Instance.atkText);

        GameObject root = new GameObject("Boss Health", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        instance = root.AddComponent<BossHealthUI>();
        instance.rect = root.GetComponent<RectTransform>();
        instance.rect.anchorMin = instance.rect.anchorMax = new Vector2(0.5f, 0f);
        instance.rect.pivot = new Vector2(0.5f, 0f);
        instance.rect.sizeDelta = new Vector2(1200f, 120f);

        Image frame = NewImage(root.transform, "Frame", new Color(0f, 0f, 0f, 0.65f));
        RectTransform frameRect = frame.rectTransform;
        frameRect.anchorMin = new Vector2(0f, 0f);
        frameRect.anchorMax = new Vector2(1f, 0f);
        frameRect.pivot = new Vector2(0.5f, 0f);
        frameRect.sizeDelta = new Vector2(0f, 56f);
        frameRect.anchoredPosition = Vector2.zero;

        instance.fill = NewImage(frame.transform, "Fill", new Color(0.92f, 0.22f, 0.12f));
        instance.fill.sprite = RuntimeSprites.White;
        instance.fill.type = Image.Type.Filled;
        instance.fill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = instance.fill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(6f, 6f);
        fillRect.offsetMax = new Vector2(-6f, -6f);

        instance.value = NewText(frame.transform, "Value", font, 32);
        Stretch(instance.value.rectTransform);

        instance.title = NewText(root.transform, "Title", font, 40);
        RectTransform titleRect = instance.title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0f);
        titleRect.anchorMax = new Vector2(1f, 0f);
        titleRect.pivot = new Vector2(0.5f, 0f);
        titleRect.sizeDelta = new Vector2(0f, 56f);
        titleRect.anchoredPosition = new Vector2(0f, 60f);
        instance.title.color = new Color(1f, 0.85f, 0.4f);

        root.SetActive(false);
        return true;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (boss == null || !boss.IsAlive)
        {
            boss = null;
            gameObject.SetActive(false);
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (boss == null || boss.currentHealth == shownHealth) return;
        shownHealth = boss.currentHealth;
        fill.fillAmount = boss.HealthRatio;
        value.text = $"{Mathf.Max(0, boss.currentHealth):N0} / {boss.maxHealth:N0}";
    }

    private void PlaceAboveBotPanel()
    {
        RectTransform parent = rect.parent as RectTransform;
        RectTransform botPanel = FindNamed(parent, "Bot Panel");
        float y = 40f;
        if (botPanel != null && botPanel.gameObject.activeInHierarchy)
        {
            botPanel.GetWorldCorners(corners);
            Vector3 topLocal = parent.InverseTransformPoint(corners[1]);
            y = topLocal.y - parent.rect.yMin + 24f;
        }

        rect.anchoredPosition = new Vector2(0f, y);
        rect.SetAsLastSibling();
    }

    private static RectTransform FindNamed(RectTransform root, string objectName)
    {
        if (root == null) return null;
        foreach (RectTransform candidate in root.GetComponentsInChildren<RectTransform>(true))
        {
            if (candidate.name == objectName) return candidate;
        }
        return null;
    }

    private static Image NewImage(Transform parent, string objectName, Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text NewText(Transform parent, string objectName, Font font, int size)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);
        return text;
    }

    private static void Stretch(RectTransform target)
    {
        target.anchorMin = Vector2.zero;
        target.anchorMax = Vector2.one;
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }
}
