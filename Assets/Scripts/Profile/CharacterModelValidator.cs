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

        HeldItemAnchors anchors =
            model.GetComponentInChildren<
                HeldItemAnchors>(true);

        if (anchors == null)
        {
            warning =
                $"Character model '{model.name}' is missing a HeldItemAnchors component.";

            return false;
        }

        int requiredHandGrips = 0;
        bool requiresMouth = false;

        switch (bodyType)
        {
            case BodyType.Humanoid:
                IncludeProfile(
                    standingGripProfile,
                    ref requiredHandGrips,
                    ref requiresMouth
                );
                break;

            case BodyType.Quadruped:
                IncludeProfile(
                    feralGripProfile,
                    ref requiredHandGrips,
                    ref requiresMouth
                );
                break;

            case BodyType.StanceSwitching:
                IncludeProfile(
                    standingGripProfile,
                    ref requiredHandGrips,
                    ref requiresMouth
                );

                IncludeProfile(
                    feralGripProfile,
                    ref requiredHandGrips,
                    ref requiresMouth
                );
                break;
        }

        List<string> missingAnchors =
            new List<string>();

        if (requiredHandGrips >= 1 &&
            anchors.LeftHand == null)
        {
            missingAnchors.Add(
                "Left Hand"
            );
        }

        if (requiredHandGrips >= 2 &&
            anchors.RightHand == null)
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
        ref int requiredHandGrips,
        ref bool requiresMouth)
    {
        if (profile == null)
            return;

        requiredHandGrips =
            Mathf.Max(
                requiredHandGrips,
                profile.HandGripCount
            );

        if (profile.MouthGripCount > 0)
        {
            requiresMouth = true;
        }
    }
}