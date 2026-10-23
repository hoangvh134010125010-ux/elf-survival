using UnityEngine;

public static class GameExit
{
    public static void Quit()
    {
        Time.timeScale = 1f;
        PlayerStats.Instance?.SaveCurrentPlayerData(true);
        SaveSystem.FlushToDisk();
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
