using UnityEngine;

public static class ActorSizing
{
    public const float PlayerSize = 1.2f;
    public const float EnemySize = 0.95f;
    public const float MiniBossScale = 1.5f;
    public const float FinalBossScale = 1.9f;

    public static bool TryFitScale(SpriteRenderer renderer, float targetSize, out Vector3 scale)
    {
        scale = Vector3.one;
        float largest = LocalSpriteSize(renderer);
        if (largest <= 0.0001f) return false;

        float s = targetSize / largest;
        scale = new Vector3(s, s, 1f);
        return true;
    }

    public static void ClampWorldSize(Transform target, SpriteRenderer renderer, float minSize, float maxSize)
    {
        if (target == null) return;

        float largest = LocalSpriteSize(renderer);
        if (largest <= 0.0001f) return;

        float current = largest * Mathf.Abs(target.localScale.x);
        if (current >= minSize && current <= maxSize) return;

        float s = Mathf.Clamp(current, minSize, maxSize) / largest;
        target.localScale = new Vector3(s, s, 1f);
    }

    public static void FitCollider(Collider2D collider, SpriteRenderer renderer, float shrink)
    {
        if (collider == null || renderer == null || renderer.sprite == null) return;
        if (collider.gameObject != renderer.gameObject) return;

        Bounds bounds = renderer.sprite.bounds;
        Vector2 size = new Vector2(bounds.size.x, bounds.size.y) * shrink;
        Vector2 center = bounds.center;

        if (collider is CapsuleCollider2D capsule)
        {
            capsule.size = size;
            capsule.offset = center;
            capsule.direction = size.y >= size.x ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;
        }
        else if (collider is BoxCollider2D box)
        {
            box.size = size;
            box.offset = center;
        }
        else if (collider is CircleCollider2D circle)
        {
            circle.radius = Mathf.Max(size.x, size.y) * 0.5f;
            circle.offset = center;
        }
    }

    private static float LocalSpriteSize(SpriteRenderer renderer)
    {
        if (renderer == null || renderer.sprite == null) return 0f;
        Vector3 size = renderer.sprite.bounds.size;
        return Mathf.Max(size.x, size.y);
    }
}
