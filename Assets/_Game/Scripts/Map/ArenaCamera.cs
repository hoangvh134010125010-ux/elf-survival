using UnityEngine;

[DefaultExecutionOrder(1000)]
public sealed class ArenaCamera : MonoBehaviour
{
    public const float ViewSize = 6.5f;

    public float smoothTime = 0.12f;

    private Camera cam;
    private Vector3 velocity;
    private bool snapped;

    public static void Attach()
    {
        Camera main = Camera.main;
        if (main == null) return;

        Behaviour brain = main.GetComponent("CinemachineBrain") as Behaviour;
        if (brain != null) brain.enabled = false;

        if (main.GetComponent<ArenaCamera>() == null)
        {
            main.gameObject.AddComponent<ArenaCamera>();
        }
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = ViewSize;
    }

    private void LateUpdate()
    {
        if (cam == null || PlayerController.Instance == null) return;

        Vector3 target = PlayerController.Instance.transform.position;
        target = ClampToArena(target);
        target.z = transform.position.z;

        if (!snapped)
        {
            transform.position = target;
            snapped = true;
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }

    private Vector3 ClampToArena(Vector3 point)
    {
        if (ArenaBounds.Instance == null) return point;

        Rect area = ArenaBounds.Instance.ViewArea;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        point.x = area.width <= halfW * 2f ? area.center.x : Mathf.Clamp(point.x, area.xMin + halfW, area.xMax - halfW);
        point.y = area.height <= halfH * 2f ? area.center.y : Mathf.Clamp(point.y, area.yMin + halfH, area.yMax - halfH);
        return point;
    }
}
