using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("UI References")]
    public GameObject levelUpPanel;
    public Text headerText;
    public SkillSlotUI[] skillSlots;

    [Header("Skill Database (Kéo các SkillSO vào đây)")]
    public List<SkillSO> availableSkills = new List<SkillSO>();

    public bool IsOpen => levelUpPanel != null && levelUpPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void OpenLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Time.timeScale = 0f;
        }


        if (headerText != null) headerText.text = "Skill Level Up";

        // Tự động tìm các SkillSlotUI con nếu mảng bị trống
        if (skillSlots == null || skillSlots.Length == 0)
        {
            if (levelUpPanel != null)
            {
                skillSlots = levelUpPanel.GetComponentsInChildren<SkillSlotUI>(true);
            }
        }

        RandomizeAndDisplaySkills();
    }

    public void SelectSkillBySlot(int slotIndex)
    {
        if (!IsOpen || skillSlots == null) return;
        if (slotIndex < 0 || slotIndex >= skillSlots.Length) return;
        if (skillSlots[slotIndex] == null) return;

        skillSlots[slotIndex].TriggerSelect();
    }

    private void RandomizeAndDisplaySkills()
    {
        // 1. Kiểm tra nếu chưa gán SkillSO
        if (availableSkills == null || availableSkills.Count == 0)
        {
            Debug.LogError("<color=red>[LevelUpManager] Bị thiếu SkillSO! Hãy kéo gán danh sách SkillSO vào ô Available Skills trong Inspector!</color>");
            return;
        }

        // 2. Kiểm tra nếu chưa gán SkillSlotUI
        if (skillSlots == null || skillSlots.Length == 0)
        {
            Debug.LogError("<color=red>[LevelUpManager] Chưa gán SkillSlotUI vào mảng Skill Slots trong Inspector!</color>");
            return;
        }

        // Xáo trộn ngẫu nhiên danh sách Skill
        List<SkillSO> shuffledList = new List<SkillSO>(availableSkills);
        for (int i = 0; i < shuffledList.Count; i++)
        {
            SkillSO temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // Đổ dữ liệu Skill ngẫu nhiên vào từng Slot UI
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (i < shuffledList.Count && skillSlots[i] != null)
            {
                skillSlots[i].gameObject.SetActive(true);
                skillSlots[i].Setup(shuffledList[i], (i + 1).ToString(), OnSkillSelected);
            }
            else if (skillSlots[i] != null)
            {
                skillSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnSkillSelected(SkillSO selectedSkill)
    {
        if (selectedSkill == null) return;

        if (PlayerStats.Instance != null)
        {
            switch (selectedSkill.skillType)
            {
                case SkillType.IncreaseAtk:
                    PlayerStats.Instance.baseAtk += (int)selectedSkill.value;
                    break;
                case SkillType.IncreaseHp:
                    PlayerStats.Instance.baseMaxHp += (int)selectedSkill.value;
                    PlayerStats.Instance.Heal((int)selectedSkill.value);
                    break;
                case SkillType.IncreaseAtkSpeed:
                    PlayerStats.Instance.baseAtkSpeed += selectedSkill.value;
                    break;
                case SkillType.HealHp:
                    PlayerStats.Instance.Heal((int)selectedSkill.value);
                    break;
            }
            PlayerStats.Instance.UpdateAllUI();
            PlayerStats.Instance.SaveCurrentPlayerData(true);
        }

        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification($"Đã chọn: <color=yellow>{selectedSkill.SkillName}</color>");
        }

        // Đóng Bảng Lên Cấp
        if (levelUpPanel != null) 
            {
                levelUpPanel.SetActive(false);
                Time.timeScale = 1f;
            }

    }
}