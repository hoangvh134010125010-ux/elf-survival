using UnityEngine;
using UnityEngine.UI;

public sealed class PurchaseQuantityPanel : MonoBehaviour
{
    private const int MaxQuantity = 9999;

    private static PurchaseQuantityPanel instance;

    private ShopItemSO item;
    private int quantity = 1;
    private bool updatingField;

    private Text titleText;
    private Image iconImage;
    private Text unitPriceText;
    private InputField quantityField;
    private Text totalText;

    public static void Open(ShopItemSO shopItem, Transform anchor, Text fontSource)
    {
        if (shopItem == null || anchor == null) return;

        Canvas canvas = anchor.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;

        if (instance == null || instance.transform.parent != canvas.transform)
        {
            if (instance != null) Destroy(instance.gameObject);
            Build(canvas, RuntimeSprites.UIFont(fontSource));
        }

        instance.item = shopItem;
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
        instance.titleText.text = $"Mua {shopItem.itemName}";
        instance.unitPriceText.text = $"Đơn giá: {shopItem.price:N0} vàng";
        instance.iconImage.sprite = shopItem.icon;
        instance.iconImage.enabled = shopItem.icon != null;
        instance.SetQuantity(1);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private int MaxAffordable()
    {
        if (item == null || item.price <= 0) return MaxQuantity;
        int gold = PlayerStats.Instance != null ? PlayerStats.Instance.gold : 0;
        return Mathf.Clamp(gold / item.price, 1, MaxQuantity);
    }

    private void SetQuantity(int value)
    {
        quantity = Mathf.Clamp(value, 1, MaxQuantity);

        updatingField = true;
        quantityField.text = quantity.ToString();
        updatingField = false;

        RefreshTotal();
    }

    private void RefreshTotal()
    {
        if (item == null) return;

        int gold = PlayerStats.Instance != null ? PlayerStats.Instance.gold : 0;
        long total = (long)item.price * quantity;
        string color = total <= gold ? "#FFE08A" : "#FF6B5B";
        totalText.text = $"Tổng: <color={color}>{total:N0}</color> vàng   (Đang có: {gold:N0})";
    }

    private void OnFieldChanged(string value)
    {
        if (updatingField) return;
        if (int.TryParse(value, out int parsed))
        {
            quantity = Mathf.Clamp(parsed, 1, MaxQuantity);
            RefreshTotal();
        }
    }

    private void OnFieldEndEdit(string value)
    {
        SetQuantity(int.TryParse(value, out int parsed) ? parsed : 1);
    }

    private void Confirm()
    {
        if (item == null || ShopManager.Instance == null) return;
        if (ShopManager.Instance.TryBuyItem(item, quantity))
        {
            gameObject.SetActive(false);
        }
        else
        {
            RefreshTotal();
        }
    }

    private void Close()
    {
        AudioManager.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
    }

    private static void Build(Canvas canvas, Font font)
    {
        GameObject overlay = new GameObject("Purchase Quantity Panel", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        Stretch(overlay.GetComponent<RectTransform>());
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
        instance = overlay.AddComponent<PurchaseQuantityPanel>();

        Image frame = NewImage(overlay.transform, "Frame", new Color(0.13f, 0.1f, 0.09f, 0.97f));
        Place(frame.rectTransform, Vector2.zero, new Vector2(960f, 860f));
        frame.raycastTarget = true;

        instance.titleText = NewText(frame.transform, "Title", font, 60, new Color(1f, 0.86f, 0.5f));
        Place(instance.titleText.rectTransform, new Vector2(0f, 340f), new Vector2(900f, 100f));

        instance.iconImage = NewImage(frame.transform, "Icon", Color.white);
        instance.iconImage.preserveAspect = true;
        Place(instance.iconImage.rectTransform, new Vector2(0f, 190f), new Vector2(170f, 170f));

        instance.unitPriceText = NewText(frame.transform, "Unit Price", font, 40, Color.white);
        Place(instance.unitPriceText.rectTransform, new Vector2(0f, 65f), new Vector2(900f, 60f));

        AddStepButton(frame.transform, font, "-10", new Vector2(-385f, -50f), -10);
        AddStepButton(frame.transform, font, "-1", new Vector2(-225f, -50f), -1);
        AddStepButton(frame.transform, font, "+1", new Vector2(225f, -50f), 1);
        AddStepButton(frame.transform, font, "+10", new Vector2(385f, -50f), 10);
        instance.quantityField = NewInputField(frame.transform, font);
        Place((RectTransform)instance.quantityField.transform, new Vector2(0f, -50f), new Vector2(270f, 120f));

        Button maxButton = NewButton(frame.transform, font, "TỐI ĐA", new Color(0.62f, 0.46f, 0.16f), 44);
        Place((RectTransform)maxButton.transform, new Vector2(0f, -180f), new Vector2(320f, 96f));
        maxButton.onClick.AddListener(() => instance.SetQuantity(instance.MaxAffordable()));

        instance.totalText = NewText(frame.transform, "Total", font, 40, Color.white);
        Place(instance.totalText.rectTransform, new Vector2(0f, -270f), new Vector2(920f, 60f));

        Button cancelButton = NewButton(frame.transform, font, "HỦY", new Color(0.4f, 0.18f, 0.16f), 46);
        Place((RectTransform)cancelButton.transform, new Vector2(-210f, -370f), new Vector2(360f, 110f));
        cancelButton.onClick.AddListener(instance.Close);

        Button buyButton = NewButton(frame.transform, font, "MUA", new Color(0.22f, 0.48f, 0.26f), 46);
        Place((RectTransform)buyButton.transform, new Vector2(210f, -370f), new Vector2(360f, 110f));
        buyButton.onClick.AddListener(instance.Confirm);
    }

    private static void AddStepButton(Transform parent, Font font, string label, Vector2 position, int step)
    {
        Button button = NewButton(parent, font, label, new Color(0.52f, 0.38f, 0.2f), 54);
        Place((RectTransform)button.transform, position, new Vector2(150f, 120f));
        button.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
            instance.SetQuantity(instance.quantity + step);
        });
    }

    private static InputField NewInputField(Transform parent, Font font)
    {
        GameObject go = new GameObject("Quantity", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image background = go.GetComponent<Image>();
        background.color = new Color(0.97f, 0.94f, 0.86f);

        Text text = NewText(go.transform, "Text", font, 56, new Color(0.12f, 0.08f, 0.05f), false);
        Stretch(text.rectTransform);
        text.supportRichText = false;

        Text placeholder = NewText(go.transform, "Placeholder", font, 48, new Color(0.45f, 0.4f, 0.35f, 0.8f), false);
        Stretch(placeholder.rectTransform);
        placeholder.text = "1";
        placeholder.fontStyle = FontStyle.Italic;

        go.SetActive(false);
        InputField field = go.AddComponent<InputField>();
        field.targetGraphic = background;
        field.textComponent = text;
        field.placeholder = placeholder;
        field.contentType = InputField.ContentType.IntegerNumber;
        field.lineType = InputField.LineType.SingleLine;
        field.characterLimit = 4;
        field.caretWidth = 3;
        field.customCaretColor = true;
        field.caretColor = new Color(0.12f, 0.08f, 0.05f);
        field.onValueChanged.AddListener(value => instance.OnFieldChanged(value));
        field.onEndEdit.AddListener(value => instance.OnFieldEndEdit(value));
        go.SetActive(true);
        return field;
    }

    private static Button NewButton(Transform parent, Font font, string label, Color color, int fontSize)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;

        Text text = NewText(go.transform, "Label", font, fontSize, Color.white, false);
        Stretch(text.rectTransform);
        text.text = label;
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);
        return button;
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

    private static Text NewText(Transform parent, string objectName, Font font, int size, Color color, bool wrap = true)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = font != null ? font : RuntimeSprites.UIFont();
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.supportRichText = true;
        text.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
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
