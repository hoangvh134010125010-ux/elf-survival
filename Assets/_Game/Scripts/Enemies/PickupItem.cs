using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum DropType
    {
        Gold,
        Exp,
        HealthPotion,
        UpgradeStone
    }

    [Header("item drop settings")]
    public DropType dropType = DropType.Gold;

    [Header("items drop")]
    public ShopItemSO itemData;

    [Header("Số lượng rơi")]
    public int value = 1;

    [Header("Magnet")]
    public float flySpeed = 8f;
    public float normalMagnetDistance = 2f;
    public float expMagnetDistance = 15f;

    private const float PickupSoundGap = 0.08f;
    private static float lastPickupSoundTime = -1f;

    private Transform playerTransform;
    private bool collected;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = RuntimeSprites.Orb;
            spriteRenderer.color = dropType == DropType.Exp ? new Color(0.35f, 0.9f, 1f) : new Color(1f, 0.85f, 0.3f);
            if (spriteRenderer.sortingOrder < 5) spriteRenderer.sortingOrder = 5;
        }

        ActorSizing.ClampWorldSize(transform, spriteRenderer, 0.35f, 0.7f);
    }

    private void OnEnable()
    {
        collected = false;
        CachePlayer();
    }

    public void Configure(int amount)
    {
        value = Mathf.Max(1, amount);
    }

    private void CachePlayer()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
            return;
        }

        if (PlayerStats.Instance != null)
        {
            playerTransform = PlayerStats.Instance.transform;
        }
    }

    private void Update()
    {
        if (collected) return;

        if (playerTransform == null)
        {
            CachePlayer();
            return;
        }

        float magnetDist = dropType == DropType.Exp ? expMagnetDistance : normalMagnetDistance;
        float magnetSqr = magnetDist * magnetDist;
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;

        if (toPlayer.sqrMagnitude <= magnetSqr)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected && other.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        collected = true;

        switch (dropType)
        {
            case DropType.Gold:
                PlayerStats.Instance?.AddGold(value);
                break;

            case DropType.Exp:
                PlayerStats.Instance?.AddExp(value);
                break;

            case DropType.HealthPotion:
            case DropType.UpgradeStone:
                if (itemData != null)
                {
                    InventoryManager.GetOrCreate().AddItemToBag(itemData, value);
                    NotificationManager.Instance?.ShowNotification($"Nhặt được: <color=green>{itemData.itemName} x{value}</color>");
                }
                break;
        }

        if (Time.unscaledTime - lastPickupSoundTime >= PickupSoundGap)
        {
            lastPickupSoundTime = Time.unscaledTime;
            AudioManager.Instance?.PlayBuySuccess();
        }
        SimpleObjectPool.Despawn(gameObject);
    }
}
