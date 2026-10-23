using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Cấu Hình Đạn")]
    public float speed = 12f;
    public float lifetime = 5f;
    private int damage = 10;

    private Transform target;
    private Vector2 moveDirection;
    private bool isInitialized;
    private float despawnAt;

    private void Awake()
    {
        ActorSizing.ClampWorldSize(transform, GetComponent<SpriteRenderer>(), 0.4f, 0.9f);
    }

    public void Setup(Transform targetTransform, int totalDamage)
    {
        target = targetTransform;
        damage = totalDamage;
        isInitialized = true;
        despawnAt = Time.time + lifetime;

        if (target != null)
        {
            moveDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            RotateTowards(target.position);
        }
        else
        {
            moveDirection = transform.right;
        }
    }

    private void OnDisable()
    {
        isInitialized = false;
        target = null;
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (Time.time >= despawnAt)
        {
            SimpleObjectPool.Despawn(gameObject);
            return;
        }

        if (target != null && !target.gameObject.activeInHierarchy)
        {
            target = null;
        }

        if (target != null)
        {
            Vector2 destination = target.position;
            moveDirection = (destination - (Vector2)transform.position).normalized;
            RotateTowards(destination);
        }

        transform.Translate(moveDirection * (speed * Time.deltaTime), Space.World);
    }

    private void RotateTowards(Vector2 destination)
    {
        Vector2 direction = destination - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized || !other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null || !enemy.IsAlive) return;

        isInitialized = false;
        enemy.TakeDamage(damage);
        SimpleObjectPool.Despawn(gameObject);
    }
}
