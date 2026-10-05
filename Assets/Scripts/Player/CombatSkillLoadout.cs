using UnityEngine;

public sealed class CombatSkillLoadout :
    MonoBehaviour
{
    public const int SlotCount = 4;

    [SerializeField]
    private CombatSkillDefinition[] skillSlots =
        new CombatSkillDefinition[SlotCount];

    public CombatSkillDefinition GetSkill(
        int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= skillSlots.Length)
        {
            return null;
        }

        return skillSlots[slotIndex];
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