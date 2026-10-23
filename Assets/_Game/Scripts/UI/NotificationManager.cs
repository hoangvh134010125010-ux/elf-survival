using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI References")]
    public GameObject notificationPanel;
    public Text notificationText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float displayDuration = 2.2f;
    public float fadeSpeed = 4.0f;

    private Coroutine currentNoticeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            bool currentPanelMissing = Instance.notificationPanel == null;
            if (currentPanelMissing)
            {
                Destroy(Instance.gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        Instance = this;
        PrepareBanner();

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void PrepareBanner()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (notificationPanel == null) return;

        Image panelImage = notificationPanel.GetComponent<Image>();
        if (panelImage != null) panelImage.raycastTarget = false;

        RectTransform rect = notificationPanel.GetComponent<RectTransform>();
        if (rect == null) return;

        rect.anchorMin = new Vector2(0.2f, 1f);
        rect.anchorMax = new Vector2(0.8f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, BannerHeight);
        rect.localScale = Vector3.one;
        PlaceBelowTopPanel();

        if (notificationText != null)
        {
            RectTransform textRect = notificationText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 10f);
            textRect.offsetMax = new Vector2(-24f, -10f);
            notificationText.supportRichText = true;
            notificationText.alignment = TextAnchor.MiddleCenter;
            notificationText.horizontalOverflow = HorizontalWrapMode.Wrap;
            notificationText.verticalOverflow = VerticalWrapMode.Truncate;
            notificationText.resizeTextForBestFit = true;
            notificationText.resizeTextMinSize = 24;
            notificationText.resizeTextMaxSize = 46;
            notificationText.raycastTarget = false;
        }
    }

    private const float BannerHeight = 120f;
    private const float GapBelowTopPanel = 12f;
    private RectTransform topPanel;
    private readonly Vector3[] corners = new Vector3[4];

    private void PlaceBelowTopPanel()
    {
        if (notificationPanel == null) return;
        RectTransform rect = notificationPanel.GetComponent<RectTransform>();
        RectTransform parent = rect != null ? rect.parent as RectTransform : null;
        if (parent == null) return;

        if (topPanel == null) topPanel = FindTopPanel(parent);

        if (topPanel == null || !topPanel.gameObject.activeInHierarchy)
        {
            rect.anchoredPosition = new Vector2(0f, -24f);
            return;
        }

        topPanel.GetWorldCorners(corners);
        Vector3 bottomLocal = parent.InverseTransformPoint(corners[0]);
        float offsetFromTop = bottomLocal.y - parent.rect.yMax - GapBelowTopPanel;
        rect.anchoredPosition = new Vector2(0f, Mathf.Min(-8f, offsetFromTop));
    }

    private static RectTransform FindTopPanel(RectTransform start)
    {
        Canvas canvas = start.GetComponentInParent<Canvas>();
        Transform root = canvas != null ? canvas.rootCanvas.transform : start.root;
        foreach (RectTransform candidate in root.GetComponentsInChildren<RectTransform>(true))
        {
            if (candidate.name == "Top Panel") return candidate;
        }
        return null;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;

        if (currentNoticeCoroutine != null)
        {
            StopCoroutine(currentNoticeCoroutine);
        }
        AudioManager.Instance?.PlayNotification();

        currentNoticeCoroutine = StartCoroutine(AnimateNotification(message));
    }

    private IEnumerator AnimateNotification(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        notificationPanel.transform.SetAsLastSibling();
        PlaceBelowTopPanel();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(displayDuration);

        if (canvasGroup != null)
        {
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        notificationPanel.SetActive(false);
        currentNoticeCoroutine = null;
    }
}
