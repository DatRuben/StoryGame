public static class CombatSkillResolver
{
    public static bool TryResolve(
        CombatSkillRequestContext context,
        out ResolvedCombatSkill skill,
        out string error)
    {
        skill = null;
        error = "";

        if (context == null)
        {
            error =
                "Cannot resolve a combat skill because the request context is missing.";

            return false;
        }

        if (context.Actor == null)
        {
            error =
                "Cannot resolve a combat skill because the actor is missing.";

            return false;
        }

        if (context.Skill == null)
        {
            error =
                "Cannot resolve a combat skill because the base skill definition is missing.";

            return false;
        }

        skill =
            new ResolvedCombatSkill(
                context.Skill
            );

        return true;
    }
}