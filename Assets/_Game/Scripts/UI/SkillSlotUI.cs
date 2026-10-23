using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Text skillIndexText;
    public Text skillNameText;
    public Image skillIcon;
    public Button selectButton;

    private SkillSO currentSkill;
    private System.Action<SkillSO> onSelectCallback;
    private bool selecting;

    private void Awake()
    {
        EnsureButtonReference();
    }

    private void EnsureButtonReference()
    {
        if (selectButton == null)
        {
            selectButton = GetComponent<Button>();
        }

        if (selectButton == null)
        {
            selectButton = gameObject.AddComponent<Button>();
        }

        Image btnImage = GetComponent<Image>();
        if (btnImage == null)
        {
            btnImage = GetComponentInChildren<Image>();
        }

        if (btnImage != null)
        {
            btnImage.raycastTarget = true;
            selectButton.targetGraphic = btnImage;
        }
    }

    public void Setup(SkillSO skillData, string indexDisplay, System.Action<SkillSO> onSelect)
    {
        EnsureButtonReference();
        selecting = false;

        currentSkill = skillData;
        onSelectCallback = onSelect;

        if (skillIndexText != null)
        {
            skillIndexText.text = indexDisplay;
            skillIndexText.raycastTarget = false;
        }

        if (currentSkill != null)
        {
            if (skillNameText != null)
            {
                skillNameText.text = string.IsNullOrEmpty(currentSkill.SkillName) ? currentSkill.name : currentSkill.SkillName;
                skillNameText.raycastTarget = false;
            }

            if (skillIcon != null)
            {
                if (currentSkill.skillIcon != null)
                {
                    skillIcon.sprite = currentSkill.skillIcon;
                    skillIcon.enabled = true;
                }
                skillIcon.raycastTarget = false;
            }
        }

        if (selectButton != null)
        {
            selectButton.interactable = true;
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(TriggerSelect);
        }
    }

    public void TriggerSelect()
    {
        if (selecting || currentSkill == null || onSelectCallback == null) return;
        selecting = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        onSelectCallback.Invoke(currentSkill);
        selecting = false;
    }
}
