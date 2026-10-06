using UnityEngine;

public static class CombatSkillOriginResolver
{
    public static bool TryResolve(
        CombatSkillRequestContext context,
        out CombatSkillOrigin origin,
        out string error)
    {
        origin = null;
        error = "";

        if (context == null)
        {
            error =
                "Cannot resolve a combat skill origin because the request context is missing.";

            return false;
        }

        GameObject actor =
            context.Actor;

        CombatSkillDefinition skill =
            context.Skill;

        if (actor == null)
        {
            error =
                "Cannot resolve a combat skill origin because the actor is missing.";

            return false;
        }

        if (skill == null)
        {
            error =
                "Cannot resolve a combat skill origin because the skill is missing.";

            return false;
        }

        switch (skill.originSource)
        {
            case CombatSkillOriginSource.None:
                return true;

            case CombatSkillOriginSource.Character:
                return TryResolveCharacterOrigin(
                    actor,
                    skill,
                    out origin,
                    out error
                );

            case CombatSkillOriginSource.HeldItem:
                return TryResolveHeldItemOrigin(
                    context,
                    out origin,
                    out error
                );

            default:
                error =
                    $"Skill '{skill.skillName}' has an unsupported action origin source.";

                return false;
        }
    }

    private static bool TryResolveCharacterOrigin(
        GameObject actor,
        CombatSkillDefinition skill,
        out CombatSkillOrigin origin,
        out string error)
    {
        origin = null;
        error = "";

        CharacterModelSetup setup =
            actor.GetComponentInChildren<
                CharacterModelSetup>(true);

        if (setup == null)
        {
            error =
                $"Cannot use skill '{skill.skillName}'. " +
                $"Actor '{actor.name}' has no CharacterModelSetup.";

            return false;
        }

        CharacterActionPoints actionPoints =
            setup.ActionPoints;

        if (actionPoints == null)
        {
            error =
                $"Cannot use skill '{skill.skillName}'. " +
                $"Character model '{setup.gameObject.name}' " +
                $"has no CharacterActionPoints.";

            return false;
        }

        if (skill.characterActionPoint ==
                CharacterActionPointType.Custom &&
            string.IsNullOrWhiteSpace(
                skill.customActionPointId))
        {
            error =
                $"Cannot use skill '{skill.skillName}'. " +
                "It requires a custom character action point, " +
                "but no custom action point ID is configured.";

            return false;
        }

        if (!actionPoints.TryGetPoint(
                skill.characterActionPoint,
                skill.customActionPointId,
                out Transform point))
        {
            string pointName =
                skill.characterActionPoint ==
                    CharacterActionPointType.Custom
                    ? $"Custom / {skill.customActionPointId}"
                    : skill.characterActionPoint.ToString();

            error =
                $"Cannot use skill '{skill.skillName}'. " +
                $"Character model '{setup.gameObject.name}' " +
                $"does not have the required action point " +
                $"'{pointName}' configured.";

            return false;
        }

        origin =
            new TransformCombatSkillOrigin(
                point
            );

        return true;
    }

    private static bool TryResolveHeldItemOrigin(
    CombatSkillRequestContext context,
    out CombatSkillOrigin origin,
    out string error)
    {
        origin = null;
        error = "";

        CombatSkillDefinition skill =
            context.Skill;

        switch (skill.heldItemSource)
        {
            case CombatSkillHeldItemSource.SkillSourceItem:
                return TryResolveSkillSourceItemOrigin(
                    context,
                    out origin,
                    out error
                );

            default:
                error =
                    $"Skill '{skill.skillName}' has an unsupported held item source.";

                return false;
        }
    }

    private static bool TryResolveSkillSourceItemOrigin(
        CombatSkillRequestContext context,
        out CombatSkillOrigin origin,
        out string error)
    {
        origin = null;
        error = "";

        InventoryItemInstance sourceItem =
            context.SourceItem;

        if (sourceItem == null ||
            sourceItem.IsEmpty)
        {
            error =
                $"Cannot use skill '{context.Skill.skillName}'. " +
                "It requires a held source item, but no source item was provided.";

            return false;
        }

        PlayerHeldItemPresenter presenter =
            context.Actor.GetComponent<
                PlayerHeldItemPresenter>();

        if (presenter == null)
        {
            error =
                $"Cannot use skill '{context.Skill.skillName}'. " +
                $"Actor '{context.Actor.name}' has no PlayerHeldItemPresenter.";

            return false;
        }

        if (!presenter.TryGetHeldItemCastPoint(
                sourceItem,
                out Transform castPoint))
        {
            error =
                $"Cannot use skill '{context.Skill.skillName}'. " +
                $"Source item '{sourceItem.Definition?.itemName}' " +
                "is not currently held or has no CastPoint configured.";

            return false;
        }

        origin =
            new TransformCombatSkillOrigin(
                castPoint
            );

        return true;
    }
}