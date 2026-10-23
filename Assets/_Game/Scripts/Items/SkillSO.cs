using UnityEngine;

public enum SkillType
{
    IncreaseAtk,
    IncreaseHp,
    IncreaseAtkSpeed,
    HealHp
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "RPG/Skill SO")]
public class SkillSO : ScriptableObject
{
    public string skillID;
    public string SkillName;
    public Sprite skillIcon;
    [TextArea] public string description;
    public SkillType skillType;
    public float value;
}


