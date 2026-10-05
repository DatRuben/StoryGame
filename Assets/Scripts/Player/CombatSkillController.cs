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
        return TryRequestSkill(
            slotIndex,
            null
        );
    }

    public bool TryRequestSkill(
        int slotIndex,
        InventoryItemInstance sourceItem)
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

        CombatSkillRequestContext context =
            new CombatSkillRequestContext(
                gameObject,
                skill,
                sourceItem
            );

        if (!CombatSkillOriginResolver.TryResolve(
                context,
                out CombatSkillOrigin origin,
                out string originError))
        {
            Debug.LogError(
                originError,
                this
            );

            return false;
        }

        Debug.Log(
            $"Requested skill: {skill.skillName}" +
            (origin != null
                ? $" from action point '{origin.Name}'."
                : "."),
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