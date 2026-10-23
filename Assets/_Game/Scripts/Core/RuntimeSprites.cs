using UnityEngine;
using UnityEngine.UI;

public static class RuntimeSprites
{
    private static Sprite white;
    private static Sprite orb;

    public static Sprite Orb
    {
        get
        {
            if (orb != null) return orb;

            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
                    float alpha = Mathf.Clamp01(1f - Mathf.SmoothStep(0.55f, 1f, distance));
                    float core = Mathf.Clamp01(1f - distance * 1.6f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Max(alpha, core));
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            orb = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return orb;
        }
    }

    public static Sprite White
    {
        get
        {
            if (white != null) return white;

            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            white = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f, 0, SpriteMeshType.FullRect, new Vector4(1, 1, 1, 1));
            return white;
        }
    }

    public static Font UIFont(Text reference = null)
    {
        if (reference != null && reference.font != null) return reference.font;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
