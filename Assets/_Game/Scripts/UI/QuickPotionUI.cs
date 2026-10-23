using UnityEngine;
using UnityEngine.UI;

public class QuickPotionUI : MonoBehaviour
{
    public static QuickPotionUI Instance;

    [System.Serializable]
    public class PotionSlot
    {
        public string slotName = "Bình Máu";
        public ShopItemSO potionSO;
        public Button useButton;
        public Image potionIcon;
        public Text countText;
        public string hotkey = "7";
    }

    [Header("3 Loại Bình Máu")]
    public PotionSlot smallPotionSlot;
    public PotionSlot mediumPotionSlot;
    public PotionSlot largePotionSlot;

    private void Awake()
    {
        Instance = this;
        if (smallPotionSlot != null && string.IsNullOrEmpty(smallPotionSlot.hotkey)) smallPotionSlot.hotkey = "7";
        if (mediumPotionSlot != null && string.IsNullOrEmpty(mediumPotionSlot.hotkey)) mediumPotionSlot.hotkey = "8";
        if (largePotionSlot != null && string.IsNullOrEmpty(largePotionSlot.hotkey)) largePotionSlot.hotkey = "9";
        PlaceOnScreen();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        ResolvePotions();
        smallPotionSlot.hotkey = "7";
        mediumPotionSlot.hotkey = "8";
        largePotionSlot.hotkey = "9";
        SetupSlotButton(smallPotionSlot, 0);
        SetupSlotButton(mediumPotionSlot, 1);
        SetupSlotButton(largePotionSlot, 2);
        RefreshQuickPotions();
    }

    private void OnEnable()
    {
        ResolvePotions();
        RefreshQuickPotions();
    }

    private void PlaceOnScreen()
    {
        RectTransform rect = transform as RectTransform;
        if (rect == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && rect.parent != canvas.transform)
        {
            rect.SetParent(canvas.transform, false);
        }

        rect.SetAsLastSibling();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-40f, 20f);
        rect.sizeDelta = new Vector2(170f, 500f);
        rect.localScale = Vector3.one;

        VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
    }

    private void ResolvePotions()
    {
        if (smallPotionSlot != null && smallPotionSlot.potionSO == null)
        {
            smallPotionSlot.potionSO = ItemDatabase.Get(CombatBalance.SmallPotionAsset);
        }

        if (mediumPotionSlot != null && mediumPotionSlot.potionSO == null)
        {
            mediumPotionSlot.potionSO = ItemDatabase.Get(CombatBalance.MediumPotionAsset);
        }

        if (largePotionSlot != null && largePotionSlot.potionSO == null)
        {
            largePotionSlot.potionSO = ItemDatabase.Get(CombatBalance.LargePotionAsset);
        }
    }

    private void SetupSlotButton(PotionSlot slot, int slotIndex)
    {
        if (slot == null || slot.useButton == null) return;

        slot.useButton.onClick.RemoveAllListeners();
        slot.useButton.onClick.AddListener(() => UsePotionBySlot(slotIndex));
        slot.useButton.interactable = true;

        Image buttonImage = slot.useButton.targetGraphic as Image;
        if (buttonImage == null) buttonImage = slot.useButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.raycastTarget = true;
            slot.useButton.targetGraphic = buttonImage;
        }

        if (slot.potionIcon != null) slot.potionIcon.raycastTarget = false;
        if (slot.countText != null) slot.countText.raycastTarget = false;

        LayoutElement element = slot.useButton.GetComponent<LayoutElement>();
        if (element == null) element = slot.useButton.gameObject.AddComponent<LayoutElement>();
        element.minWidth = 140f;
        element.minHeight = 140f;
        element.preferredWidth = 150f;
        element.preferredHeight = 150f;

        EnsureHotkeyLabel(slot);
    }

    private static void EnsureHotkeyLabel(PotionSlot slot)
    {
        if (slot == null || slot.useButton == null) return;

        Transform existing = slot.useButton.transform.Find("Hotkey");
        Text label;
        if (existing == null)
        {
            GameObject labelObject = new GameObject("Hotkey", typeof(RectTransform));
            labelObject.transform.SetParent(slot.useButton.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 1f);
            labelRect.anchoredPosition = new Vector2(8f, -6f);
            labelRect.sizeDelta = new Vector2(52f, 42f);

            label = labelObject.AddComponent<Text>();
            label.font = BuiltinFont();
            label.fontSize = 32;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.UpperLeft;
            label.color = new Color(1f, 0.95f, 0.4f);
            label.raycastTarget = false;
        }
        else
        {
            label = existing.GetComponent<Text>();
        }

        if (label != null) label.text = slot.hotkey;
    }

    private static Font BuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    public void RefreshQuickPotions()
    {
        ResolvePotions();
        UpdateSlotUI(smallPotionSlot);
        UpdateSlotUI(mediumPotionSlot);
        UpdateSlotUI(largePotionSlot);
    }

    private void UpdateSlotUI(PotionSlot slot)
    {
        if (slot == null) return;

        int count = slot.potionSO != null ? InventoryManager.GetOrCreate().CountItemInBag(slot.potionSO) : 0;

        if (slot.potionIcon != null)
        {
            if (slot.potionSO != null && slot.potionSO.icon != null)
            {
                slot.potionIcon.sprite = slot.potionSO.icon;
            }
            slot.potionIcon.enabled = slot.potionIcon.sprite != null;
            slot.potionIcon.raycastTarget = false;
            slot.potionIcon.color = count > 0 ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }

        if (slot.countText != null)
        {
            slot.countText.text = $"x{count}";
            slot.countText.color = count > 0 ? Color.white : new Color(1f, 0.45f, 0.4f);
            slot.countText.raycastTarget = false;
        }

        if (slot.useButton != null)
        {
            slot.useButton.interactable = true;
        }
    }

    public void UsePotionBySlot(int slotIndex)
    {
        ResolvePotions();
        PotionSlot slot = slotIndex switch
        {
            0 => smallPotionSlot,
            1 => mediumPotionSlot,
            2 => largePotionSlot,
            _ => null
        };

        if (slot != null)
        {
            UsePotion(slot.potionSO);
        }
    }

    private void UsePotion(ShopItemSO potionSO)
    {
        if (PlayerController.Instance != null && PlayerController.Instance.IsDead) return;

        if (potionSO == null)
        {
            NotificationManager.Instance?.ShowNotification("<color=red>Chưa gán loại bình máu!</color>");
            return;
        }

        InventoryManager inventory = InventoryManager.GetOrCreate();

        if (PlayerStats.Instance != null && PlayerStats.Instance.currentHp >= PlayerStats.Instance.GetTotalMaxHp())
        {
            NotificationManager.Instance?.ShowNotification("<color=yellow>Máu đã đầy, không cần dùng bình máu!</color>");
            return;
        }

        InventoryItem bagSlot = inventory.FindItemInBag(potionSO);
        if (bagSlot != null && bagSlot.quantity > 0)
        {
            int heal = potionSO.instantHeal > 0 ? potionSO.instantHeal : FallbackHeal(potionSO);
            if (PlayerStats.Instance != null && heal > 0 && potionSO.instantHeal <= 0)
            {
                PlayerStats.Instance.Heal(heal);
                bagSlot.quantity--;
                if (bagSlot.quantity <= 0)
                {
                    inventory.bagItems.Remove(bagSlot);
                }
                inventory.RefreshInventoryUI();
                PlayerStats.Instance.SaveCurrentPlayerData(true);
            }
            else
            {
                inventory.UseOrEquipItem(bagSlot);
            }

            AudioManager.Instance?.PlayButtonClick();
            NotificationManager.Instance?.ShowNotification($"Đã dùng <color=green>{potionSO.itemName}</color> (+{Mathf.Max(potionSO.instantHeal, heal)} HP)");
            RefreshQuickPotions();
            return;
        }

        NotificationManager.Instance?.ShowNotification("<color=red>Đã hết bình máu loại này!</color>");
        RefreshQuickPotions();
    }

    private static int FallbackHeal(ShopItemSO potionSO)
    {
        if (potionSO == null) return 0;
        if (potionSO.name == CombatBalance.LargePotionAsset || potionSO.itemName == "Bình Máu To") return CombatBalance.LargePotionHeal;
        if (potionSO.name == CombatBalance.MediumPotionAsset || potionSO.itemName == "Bình Máu Vừa") return CombatBalance.MediumPotionHeal;
        return CombatBalance.SmallPotionHeal;
    }
}
