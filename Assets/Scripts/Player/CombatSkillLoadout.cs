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

        if (string.IsNullOrWhiteSpace(skillId))
            return null;

        return skillDefinitionLibrary.TryGetSkill(skillId, out CombatSkillDefinition skill)
            ? skill
            : null;
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
