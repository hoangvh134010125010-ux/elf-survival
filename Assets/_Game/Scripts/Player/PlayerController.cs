using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;

    [Header("Settings & Cooldowns")]
    public float damageCooldown = 0.85f;
    private float lastDamageTime;
    private bool isDead;
    private Vector2 moveInput;
    private Coroutine flashRoutine;

    public bool IsDead => isDead;

    public float ContactRadius
    {
        get
        {
            if (bodyCollider == null) return 0.4f;
            Vector3 extents = bodyCollider.bounds.extents;
            return (extents.x + extents.y) * 0.5f;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
        lastDamageTime = -damageCooldown;

        if (ActorSizing.TryFitScale(spriteRenderer, ActorSizing.PlayerSize, out Vector3 fitted))
        {
            transform.localScale = fitted;
            ActorSizing.FitCollider(bodyCollider, spriteRenderer, 0.6f);
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        IgnoreEnemyCollisions();
    }

    private void IgnoreEnemyCollisions()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer < 0 || enemyLayer < 0) return;

        gameObject.layer = playerLayer;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (AutoWeapon.Instance != null)
        {
            AutoWeapon.Instance.EquipWeaponByClass();
        }

        if (PlayerStats.Instance != null)
        {
            if (PlayerStats.Instance.currentHp <= 0)
            {
                PlayerStats.Instance.currentHp = PlayerStats.Instance.GetTotalMaxHp();
            }
            PlayerStats.Instance.UpdateAllUI();
        }
    }

    public void OnMove(InputValue value)
    {
        if (isDead) return;

        Vector2 input = value.Get<Vector2>();
        moveInput = input.sqrMagnitude < 0.01f ? Vector2.zero : input;
    }

    public void OnAttack(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed)
        {
            TriggerAttack();
        }
    }

    private void Update()
    {
        if (isDead) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (spriteRenderer != null)
        {
            if (moveInput.x > 0.01f) spriteRenderer.flipX = false;
            else if (moveInput.x < -0.01f) spriteRenderer.flipX = true;
        }

        if (PlayerAnimationController.Instance != null)
        {
            PlayerAnimationController.Instance.SetMoving(isMoving);
        }
    }

    private void FixedUpdate()
    {
        if (isDead || rb == null) return;

        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float moveSpeed = PlayerStats.Instance != null ? PlayerStats.Instance.GetTotalMoveSpeed() : 5f;
        Vector2 next = rb.position + moveInput.normalized * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(ArenaBounds.Clamp(next, 0.6f));
    }

    public void TriggerAttack()
    {
        if (isDead) return;

        if (PlayerAnimationController.Instance != null)
        {
            PlayerAnimationController.Instance.TriggerAttack();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || PlayerStats.Instance == null) return;
        if (Time.time < lastDamageTime + damageCooldown) return;

        lastDamageTime = Time.time;
        PlayerStats.Instance.TakeDamage(damage);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(DamageFlashRoutine());

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        if (PlayerStats.Instance.currentHp <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return GamePerformance.WaitPlayerFlash;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
        }
        flashRoutine = null;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        moveInput = Vector2.zero;

        if (PlayerAnimationController.Instance != null)
        {
            PlayerAnimationController.Instance.TriggerDie();
        }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.StopWaves();
        }

        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification("<color=red>BẠN ĐÃ TỬ TRẬN!</color>");
        }

        Invoke(nameof(PauseGameOnDeath), 1.5f);
    }

    private void PauseGameOnDeath()
    {
        HUDManager.Instance?.CloseAllPanels();
        DeathScreen.Show();
    }
}
