using System.Collections.Generic;
using UnityEngine;

public static class EnemyRegistry
{
    private static readonly List<Enemy> Enemies = new List<Enemy>(64);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Enemies.Clear();
    }

    public static void Register(Enemy enemy)
    {
        if (enemy != null && !Enemies.Contains(enemy))
        {
            Enemies.Add(enemy);
        }
    }

    public static void Unregister(Enemy enemy)
    {
        Enemies.Remove(enemy);
    }

    public static Transform FindNearest(Vector2 origin, float range)
    {
        float rangeSqr = range * range;
        float minSqr = rangeSqr;
        Transform closest = null;

        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = Enemies[i];
            if (enemy == null)
            {
                Enemies.RemoveAt(i);
                continue;
            }

            if (!enemy.IsAlive) continue;

            float sqr = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
            if (sqr <= minSqr)
            {
                minSqr = sqr;
                closest = enemy.transform;
            }
        }

        return closest;
    }
}
