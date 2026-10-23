using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GameplayHotkeys : MonoBehaviour
{
    private static GameplayHotkeys instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Ensure()
    {
        if (instance != null) return;

        GameObject host = new GameObject(nameof(GameplayHotkeys));
        DontDestroyOnLoad(host);
        instance = host.AddComponent<GameplayHotkeys>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (DeathScreen.IsOpen) return;
        if (PlayerController.Instance != null && PlayerController.Instance.IsDead) return;

        if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
        {
            ToggleSettings();
            return;
        }

        int potionSlot = -1;
        if (keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame) potionSlot = 0;
        else if (keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame) potionSlot = 1;
        else if (keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame) potionSlot = 2;

        if (potionSlot >= 0)
        {
            QuickPotionUI potions = QuickPotionUI.Instance != null ? QuickPotionUI.Instance : Object.FindAnyObjectByType<QuickPotionUI>();
            potions?.UsePotionBySlot(potionSlot);
            return;
        }

        if (LevelUpManager.Instance == null || !LevelUpManager.Instance.IsOpen) return;

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            LevelUpManager.Instance.SelectSkillBySlot(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            LevelUpManager.Instance.SelectSkillBySlot(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
        {
            LevelUpManager.Instance.SelectSkillBySlot(2);
        }
    }

    private static void ToggleSettings()
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ToggleSettingsPanel();
            return;
        }

        if (SettingsManager.Instance == null) return;

        if (SettingsManager.Instance.settingsPanel != null && SettingsManager.Instance.settingsPanel.activeSelf)
        {
            SettingsManager.Instance.CloseSettings();
        }
        else
        {
            SettingsManager.Instance.OpenSettings();
        }
    }
}
