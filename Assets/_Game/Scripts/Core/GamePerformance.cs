using UnityEngine;

public static class GamePerformance
{
    public const int TargetFrameRate = 60;

    public static readonly WaitForSeconds WaitPlayerFlash = new WaitForSeconds(0.15f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyRuntimeSettings()
    {
        Application.targetFrameRate = TargetFrameRate;
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (QualitySettings.vSyncCount == 0)
        {
            QualitySettings.vSyncCount = 1;
        }

        Physics2D.gravity = Vector2.zero;
        Physics2D.reuseCollisionCallbacks = true;
    }
}
