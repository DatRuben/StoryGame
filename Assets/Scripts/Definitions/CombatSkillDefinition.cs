using UnityEngine;

public enum CombatSkillOriginSource
{
    None,
    Character
}

[CreateAssetMenu(
    menuName = "Game/Combat/Combat Skill Definition"
)]
public sealed class CombatSkillDefinition :
    ScriptableObject
{
    [Header("Identity")]
    public string skillName;

    [Header("Timing")]

    [Min(0f)]
    public float castTime;

    [Min(0f)]
    public float cooldown;

    [Header("Resource Cost")]

    [Min(0f)]
    public float aetherCost;

    [Header("Action Origin")]

    public CombatSkillOriginSource
        originSource =
            CombatSkillOriginSource.None;

    public CharacterActionPointType
        characterActionPoint =
            CharacterActionPointType.Center;

    public string customActionPointId;
}