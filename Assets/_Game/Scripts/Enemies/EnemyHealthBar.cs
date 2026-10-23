using UnityEngine;

public sealed class EnemyHealthBar : MonoBehaviour
{
    private const float BarHeight = 0.11f;
    private const float Gap = 0.12f;

    private Transform fill;
    private SpriteRenderer background;
    private SpriteRenderer fillRenderer;
    private float width = 0.8f;
    private float ratio = 1f;
    private bool alwaysVisible;

    public static EnemyHealthBar Create(Transform owner, Material material)
    {
        GameObject root = new GameObject("HP Bar");
        root.transform.SetParent(owner, false);
        EnemyHealthBar bar = root.AddComponent<EnemyHealthBar>();

        bar.background = CreatePart(root.transform, "Background", new Color(0.08f, 0.05f, 0.05f, 0.85f), 60, material);
        bar.fillRenderer = CreatePart(root.transform, "Fill", new Color(0.9f, 0.18f, 0.15f), 61, material);
        bar.fill = bar.fillRenderer.transform;
        root.SetActive(false);
        return bar;
    }

    private static SpriteRenderer CreatePart(Transform parent, string partName, Color color, int order, Material material)
    {
        GameObject part = new GameObject(partName);
        part.transform.SetParent(parent, false);
        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSprites.White;
        renderer.color = color;
        renderer.sortingOrder = order;
        if (material != null) renderer.sharedMaterial = material;
        return renderer;
    }

    public void Layout(SpriteRenderer body, int bossRank)
    {
        Transform owner = transform.parent;
        if (owner == null) return;

        float parentScale = Mathf.Max(0.0001f, Mathf.Abs(owner.localScale.x));
        width = bossRank == 2 ? 1.8f : bossRank == 1 ? 1.35f : 0.8f;
        alwaysVisible = bossRank > 0;

        float top = 0.5f;
        if (body != null && body.sprite != null) top = body.sprite.bounds.max.y;

        transform.localScale = Vector3.one / parentScale;
        transform.localPosition = new Vector3(0f, top + Gap / parentScale, 0f);

        background.transform.localScale = new Vector3(width + 0.06f, BarHeight + 0.05f, 1f);
        fillRenderer.color = bossRank == 2 ? new Color(1f, 0.35f, 0.1f)
            : bossRank == 1 ? new Color(1f, 0.72f, 0.15f)
            : new Color(0.9f, 0.18f, 0.15f);

        SetRatio(ratio);
    }

    public void ResetBar()
    {
        ratio = 1f;
        SetRatio(1f);
        gameObject.SetActive(alwaysVisible);
    }

    public void SetRatio(float value)
    {
        ratio = Mathf.Clamp01(value);
        if (fill == null) return;

        fill.localScale = new Vector3(width * ratio, BarHeight, 1f);
        fill.localPosition = new Vector3(-width * 0.5f + width * ratio * 0.5f, 0f, 0f);

        bool show = ratio > 0f && (alwaysVisible || ratio < 1f);
        if (gameObject.activeSelf != show) gameObject.SetActive(show);
    }
}
