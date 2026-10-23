using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PooledInstance : MonoBehaviour
{
    public GameObject Prefab;
    public bool InPool;
}

public static class SimpleObjectPool
{
    private static readonly Dictionary<GameObject, Stack<GameObject>> Pools = new Dictionary<GameObject, Stack<GameObject>>(32);
    private static bool hooked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Pools.Clear();
        hooked = false;
    }

    private static void EnsureSceneHook()
    {
        if (hooked) return;
        hooked = true;
        SceneManager.sceneUnloaded += _ => Pools.Clear();
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        EnsureSceneHook();

        GameObject instance = null;
        if (Pools.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            while (stack.Count > 0 && instance == null)
            {
                instance = stack.Pop();
            }
        }

        if (instance == null)
        {
            instance = Object.Instantiate(prefab, position, rotation);
            PooledInstance marker = instance.GetComponent<PooledInstance>();
            if (marker == null) marker = instance.AddComponent<PooledInstance>();
            marker.Prefab = prefab;
            marker.InPool = false;
        }
        else
        {
            PooledInstance marker = instance.GetComponent<PooledInstance>();
            if (marker != null) marker.InPool = false;
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
        }

        return instance;
    }

    public static T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject instance = Spawn(prefab, position, rotation);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    public static void Despawn(GameObject instance)
    {
        if (instance == null) return;

        PooledInstance marker = instance.GetComponent<PooledInstance>();
        if (marker == null || marker.Prefab == null)
        {
            Object.Destroy(instance);
            return;
        }

        if (marker.InPool) return;
        marker.InPool = true;
        instance.SetActive(false);

        if (!Pools.TryGetValue(marker.Prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>(16);
            Pools[marker.Prefab] = stack;
        }

        stack.Push(instance);
    }
}
