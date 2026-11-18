using System;
using UnityEngine;

public sealed class CombatSkillLoadout : MonoBehaviour
{
    public const int StartingUnlockedSlots = 4;

    [SerializeField]
    private CombatSkillDefinition[] skillSlots = new CombatSkillDefinition[StartingUnlockedSlots];

    [Header("Skill Definitions")]
    [SerializeField]
    private SkillDefinitionLibrary skillDefinitionLibrary;

    private CharacterProfileData boundProfile;
    private int activeLoadoutIndex;

    public int ActiveLoadoutIndex => activeLoadoutIndex;
    public int UnlockedSlotCount => StartingUnlockedSlots;

    public event Action Changed;

    public void BindProfile(CharacterProfileData profile)
    {
        boundProfile = profile;
        boundProfile?.NormalizeCombatSkillData();

        activeLoadoutIndex = 0;
        Changed?.Invoke();
    }

    public CombatSkillDefinition GetSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= UnlockedSlotCount)
            return null;

        if (boundProfile == null)
            return skillSlots != null && slotIndex < skillSlots.Length
                ? skillSlots[slotIndex]
                : null;

        if (skillDefinitionLibrary == null)
            return null;

        CharacterCombatSkillLoadoutData[] loadouts = boundProfile.combatSkillLoadouts;

        if (loadouts == null || activeLoadoutIndex >= loadouts.Length)
            return null;

        CharacterCombatSkillLoadoutData loadout = loadouts[activeLoadoutIndex];

        if (loadout?.skillIds == null || slotIndex >= loadout.skillIds.Length)
            return null;

        string skillId = loadout.skillIds[slotIndex];

        if (!boundProfile.HasLearnedCombatSkill(skillId))
            return null;

        return skillDefinitionLibrary.TryGetSkill(skillId, out CombatSkillDefinition skill)
            ? skill
            : null;
    }


    public bool TryAssignSkill(int loadoutIndex, int slotIndex, CombatSkillDefinition skill, out string error)
    {
        error = null;

        if (boundProfile == null)
        {
            error = "No character profile is bound.";
            return false;
        }

        EntityCombatState combatState = GetComponent<EntityCombatState>();
        CombatSkillController skillController = GetComponent<CombatSkillController>();

        if (combatState == null || skillController == null)
        {
            error = "Combat systems are unavailable.";
            return false;
        }

        if (combatState.IsInCombat || skillController.IsBusy)
        {
            error = "Skills cannot be changed during combat or an active skill.";
            return false;
        }

        CharacterCombatSkillLoadoutData[] loadouts = boundProfile.combatSkillLoadouts;

        if (loadouts == null || loadoutIndex < 0 || loadoutIndex >= loadouts.Length)
        {
            error = "Invalid skill loadout.";
            return false;
        }

        CharacterCombatSkillLoadoutData loadout = loadouts[loadoutIndex];

        if (loadout?.skillIds == null || slotIndex < 0 ||
            slotIndex >= UnlockedSlotCount || slotIndex >= loadout.skillIds.Length)
        {
            error = "Skill slot is unavailable.";
            return false;
        }

        if (skill != null)
        {
            if (skillDefinitionLibrary == null ||
                !skillDefinitionLibrary.TryGetSkill(skill.Id, out CombatSkillDefinition definition) ||
                definition != skill)
            {
                error = "Skill is not available in the Skill Definition Library.";
                return false;
            }

            if (!boundProfile.HasLearnedCombatSkill(skill.Id))
            {
                error = "This character has not learned the skill.";
                return false;
            }
        }

        string skillId = skill != null ? skill.Id : null;

        if (string.Equals(loadout.skillIds[slotIndex], skillId, StringComparison.OrdinalIgnoreCase))
            return true;

        loadout.skillIds[slotIndex] = skillId;

        CharacterSaveSystem.SaveProfile(boundProfile);
        Changed?.Invoke();

        return true;
    }

    internal bool TrySetActiveLoadout(int index)
    {
        if (boundProfile?.combatSkillLoadouts == null ||
            index < 0 ||
            index >= boundProfile.combatSkillLoadouts.Length)
        {
            return false;
        }

        if (activeLoadoutIndex == index)
            return true;

        activeLoadoutIndex = index;
        Changed?.Invoke();

        return true;
    }

    private void OnValidate()
    {
        if (skillSlots != null && skillSlots.Length == StartingUnlockedSlots)
            return;

        CombatSkillDefinition[] newSlots = new CombatSkillDefinition[StartingUnlockedSlots];

        if (skillSlots != null)
            Array.Copy(skillSlots, newSlots, Math.Min(skillSlots.Length, newSlots.Length));

        skillSlots = newSlots;
    }
}
