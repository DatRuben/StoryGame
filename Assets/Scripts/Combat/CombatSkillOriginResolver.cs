using UnityEngine;

public static class CombatSkillOriginResolver
{
    public static bool TryResolve(
        CombatSkillRequestContext context,
        ResolvedCombatSkill skill,
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

        switch (skill.OriginSource)
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
                    skill,
                    out origin,
                    out error
                );

            default:
                error =
                    $"Skill '{skill.SkillName}' has an unsupported action origin source.";

                return false;
        }
    }

    private static bool TryResolveCharacterOrigin(
        GameObject actor,
        ResolvedCombatSkill skill,
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
                $"Cannot use skill '{skill.SkillName}'. " +
                $"Actor '{actor.name}' has no CharacterModelSetup.";

            return false;
        }

        CharacterActionPoints actionPoints =
            setup.ActionPoints;

        if (actionPoints == null)
        {
            error =
                $"Cannot use skill '{skill.SkillName}'. " +
                $"Character model '{setup.gameObject.name}' " +
                $"has no CharacterActionPoints.";

            return false;
        }

        if (skill.CharacterActionPoint ==
                CharacterActionPointType.Custom &&
            string.IsNullOrWhiteSpace(
                skill.CustomActionPointId))
        {
            error =
                $"Cannot use skill '{skill.SkillName}'. " +
                "It requires a custom character action point, " +
                "but no custom action point ID is configured.";

            return false;
        }

        if (!actionPoints.TryGetPoint(
                skill.CharacterActionPoint,
                skill.CustomActionPointId,
                out Transform point))
        {
            string pointName =
                skill.CharacterActionPoint ==
                    CharacterActionPointType.Custom
                    ? $"Custom / {skill.CustomActionPointId}"
                    : skill.CharacterActionPoint.ToString();

            error =
                $"Cannot use skill '{skill.SkillName}'. " +
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
        ResolvedCombatSkill skill,
        out CombatSkillOrigin origin,
        out string error)
    {
        origin = null;
        error = "";

        switch (skill.HeldItemSource)
        {
            case CombatSkillHeldItemSource.SkillSourceItem:
                return TryResolveSkillSourceItemOrigin(
                    context,
                    skill,
                    out origin,
                    out error
                );

            default:
                error =
                    $"Skill '{skill.SkillName}' has an unsupported held item source.";

                return false;
        }
    }

    private static bool TryResolveSkillSourceItemOrigin(
        CombatSkillRequestContext context,
        ResolvedCombatSkill skill,
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
                $"Cannot use skill '{skill.SkillName}'. " +
                "It requires a held source item, but no source item was provided.";

            return false;
        }

        PlayerHeldItemPresenter presenter =
            context.Actor.GetComponent<
                PlayerHeldItemPresenter>();

        if (presenter == null)
        {
            error =
                $"Cannot use skill '{skill.SkillName}'. " +
                $"Actor '{context.Actor.name}' has no PlayerHeldItemPresenter.";

            return false;
        }

        if (!presenter.TryGetHeldItemCastPoint(
                sourceItem,
                out Transform castPoint))
        {
            error =
                $"Cannot use skill '{skill.SkillName}'. " +
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