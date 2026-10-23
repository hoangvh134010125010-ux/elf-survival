using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class UIInputBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Hook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureCurrentScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureCurrentScene();
    }

    private static void ConfigureCurrentScene()
    {
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = Object.FindAnyObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            GameObject esObject = new GameObject("EventSystem");
            eventSystem = esObject.AddComponent<EventSystem>();
            esObject.AddComponent<InputSystemUIInputModule>();
        }

        InputSystemUIInputModule uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (uiModule == null)
        {
            uiModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        uiModule.enabled = true;
        uiModule.AssignDefaultActions();

        PlayerInput[] players = Object.FindObjectsByType<PlayerInput>(FindObjectsInactive.Exclude);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].uiInputModule = uiModule;
        }
    }
}
