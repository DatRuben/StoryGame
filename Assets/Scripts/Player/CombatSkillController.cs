using UnityEngine;

[RequireComponent(typeof(CombatSkillLoadout))]
public sealed class CombatSkillController :
    MonoBehaviour
{
    private CombatSkillLoadout skillLoadout;

    private void Awake()
    {
        ResolveReferences();
    }

    public bool TryRequestSkill(
        int slotIndex)
    {
        ResolveReferences();

        if (skillLoadout == null)
            return false;

        CombatSkillDefinition skill =
            skillLoadout.GetSkill(
                slotIndex
            );

        if (skill == null)
            return false;

        Debug.Log(
            "Requested skill: " +
            skill.skillName,
            this
        );

        return true;
    }

    private void ResolveReferences()
    {
        if (skillLoadout == null)
        {
            skillLoadout =
                GetComponent<
                    CombatSkillLoadout>();
        }
    }
}