using UnityEngine;

public static class CombatSkillOriginResolver
{
    public static bool TryResolve(
        GameObject actor,
        CombatSkillDefinition skill,
        out Transform origin,
        out string error)
    {
        origin = null;
        error = "";

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

            default:
                error =
                    $"Skill '{skill.skillName}' has an unsupported action origin source.";

                return false;
        }
    }

    private static bool TryResolveCharacterOrigin(
        GameObject actor,
        CombatSkillDefinition skill,
        out Transform origin,
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
                out origin))
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

        return true;
    }
}