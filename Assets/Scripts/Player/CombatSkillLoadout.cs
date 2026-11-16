using UnityEngine;

public sealed class CombatSkillLoadout :
    MonoBehaviour
{
    public const int SlotCount = 4;

    [SerializeField]
    private CombatSkillDefinition[] skillSlots =
        new CombatSkillDefinition[SlotCount];


    [Header("Skill Definitions")]
    [SerializeField]
    private SkillDefinitionLibrary skillDefinitionLibrary;

    private CharacterProfileData boundProfile;

    public void BindProfile(CharacterProfileData profile)
    {
        boundProfile = profile;

        boundProfile?.NormalizeCombatSkillData();
    }


    public CombatSkillDefinition GetSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount)
            return null;

        if (boundProfile == null)
            return skillSlots[slotIndex];

        if (skillDefinitionLibrary == null)
            return null;

        CharacterCombatSkillLoadoutData[] loadouts =
            boundProfile.combatSkillLoadouts;

        if (loadouts == null ||
            loadouts.Length == 0 ||
            loadouts[0] == null)
        {
            return null;
        }

        string[] skillIds = loadouts[0].skillIds;

        if (skillIds == null ||
            slotIndex >= skillIds.Length)
        {
            return null;
        }

        string skillId = skillIds[slotIndex];

        if (string.IsNullOrWhiteSpace(skillId))
            return null;

        return skillDefinitionLibrary.TryGetSkill(
            skillId,
            out CombatSkillDefinition skill)
                ? skill
                : null;
    }


    private void OnValidate()
    {
        if (skillSlots != null &&
            skillSlots.Length == SlotCount)
        {
            return;
        }

        CombatSkillDefinition[] newSlots =
            new CombatSkillDefinition[
                SlotCount
            ];

        if (skillSlots != null)
        {
            int copyCount =
                Mathf.Min(
                    skillSlots.Length,
                    newSlots.Length
                );

            for (int i = 0;
                 i < copyCount;
                 i++)
            {
                newSlots[i] =
                    skillSlots[i];
            }
        }

        skillSlots =
            newSlots;
    }
}