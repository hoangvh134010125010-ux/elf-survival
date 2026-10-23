using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public bool isBoss = false;
    public int BossRank { get; private set; }

    [Header("Chỉ Số Cơ Bản (Lưu chỉ số gốc)")]
    public float speed = 2.2f;
    public int baseMaxHealth = 45;
    public int baseDamage = 7;

    [Header("Chỉ Số Hiện Tại")]
    [HideInInspector] public int maxHealth;
    [HideInInspector] public int currentHealth;
    [HideInInspector] public int damage;

    [Header("Phát hiện người chơi")]
    [Tooltip("Khoảng cách player phải đến gần thì quái mới bắt đầu đuổi.")]
    public float aggroRange = 7f;

    [Header("Tỉ Lệ Rơi Đồ (%)")]
    [Range(0, 100)] public float goldDropChance = 50f;
    [Range(0, 100)] public float potionDropChance = 20f;
    [Range(0, 100)] public float upgradeStoneDropChance = 10f;

    [Header("Item Drop Prefabs")]
    public GameObject expPrefab;
    public GameObject goldPrefab;
    public GameObject potionPrefab;
    public GameObject upgradeStonePrefab;

    private const float BodyMass = 0.4f;
    private const float BossAggroMultiplier = 1.5f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Animator animator;
    private bool isChasing;
    private Transform playerTransform;
    private bool isDead;
    private Coroutine flashRoutine;
    private Vector3 defaultScale;
    private bool sized;
    private Collider2D bodyCollider;
    private Color baseTint = Color.white;
    private bool hasMultiplier;
    private float hpMultiplier = 1f;
    private float damageMultiplier = 1f;
    private float speedMultiplier = 1f;
    private EnemyHealthBar healthBar;

    public bool IsAlive => !isDead && isActiveAndEnabled;
    public bool IsChasing => isChasing;
    public float CurrentSpeed => speed * speedMultiplier;
    private float AggroRange => BossRank > 0 ? aggroRange * BossAggroMultiplier : aggroRange;
    public float HealthRatio => maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.mass = BodyMass;
            body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            body.interpolation = RigidbodyInterpolation2D.None;
        }
        defaultScale = transform.localScale;
        healthBar = EnemyHealthBar.Create(transform, spriteRenderer != null ? spriteRenderer.sharedMaterial : null);
        TryFitSize();
    }

    private void LayoutHealthBar()
    {
        if (healthBar == null) return;
        healthBar.Layout(spriteRenderer, BossRank);
        healthBar.ResetBar();
    }

    private void TryFitSize()
    {
        if (sized) return;
        if (!ActorSizing.TryFitScale(spriteRenderer, ActorSizing.EnemySize, out Vector3 fitted)) return;

        defaultScale = fitted;
        ActorSizing.FitCollider(bodyCollider, spriteRenderer, 0.75f);
        sized = true;
        transform.localScale = defaultScale * BossScale();
        if (healthBar != null) healthBar.Layout(spriteRenderer, BossRank);
    }

    private float BossScale()
    {
        if (BossRank == 2) return ActorSizing.FinalBossScale;
        if (BossRank == 1) return ActorSizing.MiniBossScale;
        return 1f;
    }

    private void OnEnable()
    {
        isDead = false;
        if (body != null) body.linearVelocity = Vector2.zero;
        SetChasing(false);
        ApplyCurrentStats();

        if (spriteRenderer != null) spriteRenderer.color = baseTint;

        FindPlayerTarget();
        EnemyRegistry.Register(this);
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        hasMultiplier = false;
        BossRank = 0;
        isBoss = false;
        hpMultiplier = 1f;
        damageMultiplier = 1f;
        speedMultiplier = 1f;
        baseTint = Color.white;
        transform.localScale = defaultScale;
        EnemyRegistry.Unregister(this);
    }

    public void ApplyStatMultiplier(float hpScale, float damageScale, float speedScale, int bossRank = 0)
    {
        hasMultiplier = true;
        hpMultiplier = Mathf.Max(0.1f, hpScale);
        damageMultiplier = Mathf.Max(0.1f, damageScale);
        speedMultiplier = Mathf.Max(0.1f, speedScale);
        BossRank = bossRank;
        isBoss = bossRank > 0;
        ApplyCurrentStats();
    }

    private void ApplyCurrentStats()
    {
        float hpScale = hasMultiplier ? hpMultiplier : 1f;
        float damageScale = hasMultiplier ? damageMultiplier : 1f;
        maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseMaxHealth * hpScale));
        currentHealth = maxHealth;
        damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageScale));

        transform.localScale = defaultScale * BossScale();
        if (BossRank == 2) baseTint = new Color(1f, 0.55f, 0.45f);
        else if (BossRank == 1) baseTint = new Color(1f, 0.86f, 0.45f);
        else baseTint = Color.white;

        if (spriteRenderer != null) spriteRenderer.color = baseTint;
        LayoutHealthBar();
    }

    private void FindPlayerTarget()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    private void Update()
    {
        if (isDead) return;
        if (!sized) TryFitSize();

        if (playerTransform == null)
        {
            FindPlayerTarget();
            return;
        }

        if (!isChasing)
        {
            float range = AggroRange;
            if (((Vector2)playerTransform.position - (Vector2)transform.position).sqrMagnitude > range * range) return;
            SetChasing(true);
        }

        if (spriteRenderer != null)
        {
            float dx = playerTransform.position.x - transform.position.x;
            if (dx > 0.01f) spriteRenderer.flipX = false;
            else if (dx < -0.01f) spriteRenderer.flipX = true;
        }

        if (body == null)
        {
            Vector2 step = Vector2.MoveTowards(transform.position, playerTransform.position, CurrentSpeed * Time.deltaTime);
            transform.position = ArenaBounds.Clamp(step, 0.4f);
        }
    }

    private void SetChasing(bool chasing)
    {
        isChasing = chasing;
        if (animator != null) animator.speed = chasing ? 1f : 0f;
        if (!chasing && body != null) body.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (isDead || body == null || playerTransform == null) return;

        if (!isChasing)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 self = body.position;
        Vector2 target = playerTransform.position;
        Vector2 away = self - target;
        float reach = ContactReach();

        if (away.sqrMagnitude <= reach * reach)
        {
            body.linearVelocity = Vector2.zero;
            if (away.sqrMagnitude < reach * reach * 0.7f && away.sqrMagnitude > 0.0001f)
            {
                body.MovePosition(ArenaBounds.Clamp(target + away.normalized * reach, 0.4f));
            }

            if (PlayerController.Instance != null) PlayerController.Instance.TakeDamage(damage);
            return;
        }

        Vector2 next = Vector2.MoveTowards(self, target, CurrentSpeed * Time.fixedDeltaTime);
        if ((next - target).sqrMagnitude < reach * reach)
        {
            next = target + away.normalized * reach;
        }
        body.MovePosition(ArenaBounds.Clamp(next, 0.4f));
    }

    private float ContactReach()
    {
        float own = 0.35f;
        if (bodyCollider != null)
        {
            Vector3 extents = bodyCollider.bounds.extents;
            own = (extents.x + extents.y) * 0.5f;
        }

        float player = PlayerController.Instance != null ? PlayerController.Instance.ContactRadius : 0.4f;
        return own + player;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        if (healthBar != null) healthBar.SetRatio(HealthRatio);
        if (!isChasing) SetChasing(true);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(DamageFlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) spriteRenderer.color = baseTint;
        }
        flashRoutine = null;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        int expAmount = MapManager.Instance != null ? MapManager.Instance.RollExp(BossRank) : 3;
        SpawnDropItem(expPrefab, expAmount);

        if (Random.Range(0f, 100f) <= goldDropChance)
        {
            int goldAmount = MapManager.Instance != null ? MapManager.Instance.RollGold(BossRank) : 12;
            SpawnDropItem(goldPrefab, goldAmount);
        }

        if (Random.Range(0f, 100f) <= potionDropChance)
        {
            SpawnDropItem(potionPrefab, 1);
        }

        if (Random.Range(0f, 100f) <= upgradeStoneDropChance)
        {
            SpawnDropItem(upgradeStonePrefab, 1);
        }

        if (BossRank > 0)
        {
            int difficulty = MapManager.Instance != null ? MapManager.Instance.CurrentDifficulty : 0;
            SpawnDropItem(upgradeStonePrefab, BossRank * 3 + difficulty * 2);
            SpawnDropItem(potionPrefab, BossRank);
        }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnEnemyDied(this);
        }

        SimpleObjectPool.Despawn(gameObject);
    }

    private void SpawnDropItem(GameObject prefab, int amount)
    {
        if (prefab == null) return;

        Vector3 randomOffset = new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(-0.35f, 0.35f), 0f);
        GameObject drop = SimpleObjectPool.Spawn(prefab, transform.position + randomOffset, Quaternion.identity);
        if (drop == null) return;

        PickupItem pickup = drop.GetComponent<PickupItem>();
        if (pickup != null)
        {
            pickup.Configure(Mathf.Max(1, amount));
        }
    }
}
