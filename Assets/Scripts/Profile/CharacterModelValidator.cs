using System.Collections.Generic;
using UnityEngine;

public static class CharacterModelValidator
{
    public static bool TryValidate(
        GameObject model,
        BodyType bodyType,
        CharacterGripProfile standingGripProfile,
        CharacterGripProfile feralGripProfile,
        out string warning)
    {
        warning = "";

        if (model == null)
        {
            warning =
                "Character model is missing.";

            return false;
        }

        CharacterModelSetup setup =
            model.GetComponent<CharacterModelSetup>();

        if (setup == null)
        {
            warning =
                $"Character model '{model.name}' is missing CharacterModelSetup.";

            return false;
        }

        if (setup.AnimatorComponent == null)
        {
            warning =
                $"Character model '{model.name}' is missing its required Animator.";

            return false;
        }

        HeldItemAnchors anchors =
            setup.HeldItemAnchors;

        if (anchors == null)
        {
            warning =
                $"Character model '{model.name}' is missing its required HeldItemAnchors.";

            return false;
        }

        bool requiresMouth = false;

        switch (bodyType)
        {
            case BodyType.Humanoid:
                IncludeProfile(
                    standingGripProfile,
                    ref requiresMouth
                );
                break;

            case BodyType.Quadruped:
                IncludeProfile(
                    feralGripProfile,
                    ref requiresMouth
                );
                break;

            case BodyType.StanceSwitching:
                IncludeProfile(
                    standingGripProfile,
                    ref requiresMouth
                );

                IncludeProfile(
                    feralGripProfile,
                    ref requiresMouth
                );
                break;
        }

        List<string> missingAnchors =
            new List<string>();

        if (anchors.LeftHand == null)
        {
            missingAnchors.Add(
                "Left Hand"
            );
        }

        if (anchors.RightHand == null)
        {
            missingAnchors.Add(
                "Right Hand"
            );
        }

        if (requiresMouth &&
            anchors.Mouth == null)
        {
            missingAnchors.Add(
                "Mouth"
            );
        }

        if (missingAnchors.Count == 0)
            return true;

        warning =
            $"Character model '{model.name}' is missing required held-item anchors:\n- " +
            string.Join(
                "\n- ",
                missingAnchors
            );

        return false;
    }

    private static void IncludeProfile(
        CharacterGripProfile profile,
        ref bool requiresMouth)
    {
        if (profile == null)
            return;

        if (profile.MouthGripCount > 0)
        {
            requiresMouth = true;
        }
    }
}