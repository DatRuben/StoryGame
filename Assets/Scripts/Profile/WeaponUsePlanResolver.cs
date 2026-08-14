using System.Collections.Generic;
using UnityEngine;

public static class WeaponUsePlanResolver
{
    private const int MaxOptionsPerWeapon = 3;

    public static bool TryResolve(
        IReadOnlyList<ItemDefinition> weapons,
        CharacterHandlingProfile handling,
        int availableHands,
        int availableMouth,
        List<ResolvedItemHandling> results)
    {
        if (results == null)
            return false;

        results.Clear();

        if (weapons == null ||
            handling == null ||
            weapons.Count == 0 ||
            weapons.Count > WeaponSet.SlotCount)
        {
            return false;
        }

        availableHands =
            Mathf.Clamp(
                availableHands,
                0,
                handling.GripProfile.HandGripCount
            );

        availableMouth =
            Mathf.Clamp(
                availableMouth,
                0,
                handling.GripProfile.MouthGripCount
            );

        if (handling.GripProfile.WeaponMode !=
                ConventionalWeaponMode.Humanoid &&
            weapons.Count > 1)
        {
            return false;
        }

        ResolvedItemHandling[] firstOptions =
            new ResolvedItemHandling[
                MaxOptionsPerWeapon
            ];

        int firstCount =
            GetOptions(
                weapons[0],
                handling,
                availableHands,
                availableMouth,
                firstOptions
            );

        if (firstCount == 0)
            return false;

        if (weapons.Count == 1)
        {
            ResolvedItemHandling best =
                GetBest(
                    firstOptions,
                    firstCount
                );

            if (best == null)
                return false;

            results.Add(best);

            return true;
        }

        ResolvedItemHandling[] secondOptions =
            new ResolvedItemHandling[
                MaxOptionsPerWeapon
            ];

        int secondCount =
            GetOptions(
                weapons[1],
                handling,
                availableHands,
                availableMouth,
                secondOptions
            );

        if (secondCount == 0)
            return false;

        ResolvedItemHandling bestFirst = null;
        ResolvedItemHandling bestSecond = null;

        int bestScore =
            int.MaxValue;

        for (int firstIndex = 0;
             firstIndex < firstCount;
             firstIndex++)
        {
            for (int secondIndex = 0;
                 secondIndex < secondCount;
                 secondIndex++)
            {
                ResolvedItemHandling first =
                    firstOptions[firstIndex];

                ResolvedItemHandling second =
                    secondOptions[secondIndex];

                if (!FitsTogether(
                    first,
                    second,
                    availableHands,
                    availableMouth))
                {
                    continue;
                }

                int score =
                    GetPairScore(
                        first,
                        second
                    );

                if (score >= bestScore)
                    continue;

                bestScore = score;

                bestFirst = first;
                bestSecond = second;
            }
        }

        if (bestFirst == null ||
            bestSecond == null)
        {
            return false;
        }

        results.Add(bestFirst);
        results.Add(bestSecond);

        return true;
    }

    private static int GetOptions(
        ItemDefinition weapon,
        CharacterHandlingProfile handling,
        int availableHands,
        int availableMouth,
        ResolvedItemHandling[] options)
    {
        if (weapon == null ||
            handling == null ||
            options == null)
        {
            return 0;
        }

        int count = 0;

        bool allowHands =
            handling.GripProfile.WeaponMode !=
            ConventionalWeaponMode.MouthOnly;

        if (allowHands)
        {
            int minimumHands =
                weapon.GetMinimumUseGripCount(
                    GripType.Hand
                );

            int maximumHands =
                handling.GripProfile.WeaponMode ==
                    ConventionalWeaponMode
                        .MouthOrOneHand
                    ? Mathf.Min(
                        1,
                        availableHands
                    )
                    : availableHands;

            for (int handCount =
                     minimumHands;
                 handCount <= maximumHands &&
                 count < options.Length;
                 handCount++)
            {
                ResolvedItemHandling resolved =
                    ItemHandlingResolver.Resolve(
                        weapon,
                        handling,
                        GripType.Hand,
                        handCount
                    );

                if (resolved == null ||
                    !resolved.canUse)
                {
                    continue;
                }

                options[count] = resolved;
                count++;
            }
        }

        bool allowMouth =
            handling.GripProfile.CanOperateWith(
                GripType.Mouth
            ) &&
            availableMouth > 0 &&
            weapon.canUseWithMouth;

        if (allowMouth &&
            count < options.Length)
        {
            if (weapon.GetMinimumUseGripCount(
                    GripType.Hand) <= 2)
            {
                ResolvedItemHandling resolved =
                    ItemHandlingResolver.Resolve(
                        weapon,
                        handling,
                        GripType.Mouth,
                        1
                    );

                if (resolved != null &&
                    resolved.canUse)
                {
                    options[count] = resolved;
                    count++;
                }
            }
        }

        return count;
    }

    private static bool FitsTogether(
        ResolvedItemHandling first,
        ResolvedItemHandling second,
        int availableHands,
        int availableMouth)
    {
        if (first == null ||
            second == null)
        {
            return false;
        }

        int usedHands =
            GetGripUse(
                first,
                GripType.Hand
            ) +
            GetGripUse(
                second,
                GripType.Hand
            );

        int usedMouth =
            GetGripUse(
                first,
                GripType.Mouth
            ) +
            GetGripUse(
                second,
                GripType.Mouth
            );

        return usedHands <= availableHands &&
               usedMouth <= availableMouth;
    }

    private static int GetGripUse(
        ResolvedItemHandling handling,
        GripType gripType)
    {
        if (handling == null ||
            handling.gripType != gripType)
        {
            return 0;
        }

        return handling.assignedGripCount;
    }

    private static ResolvedItemHandling GetBest(
        ResolvedItemHandling[] options,
        int count)
    {
        ResolvedItemHandling best = null;

        int bestScore =
            int.MaxValue;

        for (int i = 0;
             i < count;
             i++)
        {
            int score =
                GetScore(
                    options[i]
                );

            if (score >= bestScore)
                continue;

            bestScore = score;
            best = options[i];
        }

        return best;
    }

    private static int GetPairScore(
        ResolvedItemHandling first,
        ResolvedItemHandling second)
    {
        int worstTier =
            Mathf.Max(
                (int)first.tier,
                (int)second.tier
            );

        return
            worstTier * 100 +
            GetScore(first) +
            GetScore(second);
    }

    private static int GetScore(
        ResolvedItemHandling handling)
    {
        if (handling == null)
            return int.MaxValue;

        int score =
            (int)handling.tier * 10;

        score +=
            handling.assignedGripCount;

        if (handling.gripType ==
            GripType.Mouth)
        {
            score++;
        }

        return score;
    }
}