using UnityEngine;

public static class ItemVisuals
{
    public static Color GetBorderColorByLevel(int level)
    {
        if (level >= 15) return new Color(1f, 0.84f, 0f);
        if (level >= 10) return new Color(0.63f, 0.13f, 0.94f);
        if (level >= 5) return new Color(0f, 0.64f, 1f);
        return Color.white;
    }
}
