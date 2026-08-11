using System.Collections.Generic;
using UnityEngine;

public static class WeaponUsePlanResolver
{
    public static bool TryResolve(
        IReadOnlyList<ItemDefinition> items,
        CharacterHandlingProfile character,
        int availableHands,
        int availableMouth,
        List<ResolvedItemHandling> results)
    {
        if (results == null)
            return false;

        results.Clear();

        if (items == null ||
            character == null ||
            items.Count == 0)
        {
            return false;
        }

        availableHands =
            Mathf.Clamp(
                availableHands,
                0,
                character.handGripCount
            );

        availableMouth =
            Mathf.Clamp(
                availableMouth,
                0,
                character.mouthGripCount
            );

        ResolvedItemHandling[] currentPlan =
            new ResolvedItemHandling[
                items.Count
            ];

        ResolvedItemHandling[] bestPlan =
            new ResolvedItemHandling[
                items.Count
            ];

        bool foundPlan = false;

        int bestWorstTier =
            int.MaxValue;

        int bestTotalTier =
            int.MaxValue;

        Search(
            items,
            character,
            0,
            availableHands,
            availableMouth,
            currentPlan,
            bestPlan,
            0,
            0,
            ref foundPlan,
            ref bestWorstTier,
            ref bestTotalTier
        );

        if (!foundPlan)
            return false;

        for (int i = 0;
             i < bestPlan.Length;
             i++)
        {
            results.Add(
                bestPlan[i]
            );
        }

        return true;
    }

    private static void Search(
        IReadOnlyList<ItemDefinition> items,
        CharacterHandlingProfile character,
        int itemIndex,
        int availableHands,
        int availableMouth,
        ResolvedItemHandling[] currentPlan,
        ResolvedItemHandling[] bestPlan,
        int currentWorstTier,
        int currentTotalTier,
        ref bool foundPlan,
        ref int bestWorstTier,
        ref int bestTotalTier)
    {
        if (itemIndex >=
            items.Count)
        {
            if (!IsBetterPlan(
                foundPlan,
                currentWorstTier,
                currentTotalTier,
                bestWorstTier,
                bestTotalTier))
            {
                return;
            }

            for (int i = 0;
                 i < currentPlan.Length;
                 i++)
            {
                bestPlan[i] =
                    currentPlan[i];
            }

            foundPlan = true;

            bestWorstTier =
                currentWorstTier;

            bestTotalTier =
                currentTotalTier;

            return;
        }

        ItemDefinition item =
            items[itemIndex];

        if (item == null)
            return;

        SearchGrip(
            items,
            character,
            itemIndex,
            item,
            GripType.Hand,
            availableHands,
            availableMouth,
            currentPlan,
            bestPlan,
            currentWorstTier,
            currentTotalTier,
            ref foundPlan,
            ref bestWorstTier,
            ref bestTotalTier
        );

        SearchGrip(
            items,
            character,
            itemIndex,
            item,
            GripType.Mouth,
            availableHands,
            availableMouth,
            currentPlan,
            bestPlan,
            currentWorstTier,
            currentTotalTier,
            ref foundPlan,
            ref bestWorstTier,
            ref bestTotalTier
        );

        currentPlan[itemIndex] =
            null;
    }

    private static void SearchGrip(
        IReadOnlyList<ItemDefinition> items,
        CharacterHandlingProfile character,
        int itemIndex,
        ItemDefinition item,
        GripType gripType,
        int availableHands,
        int availableMouth,
        ResolvedItemHandling[] currentPlan,
        ResolvedItemHandling[] bestPlan,
        int currentWorstTier,
        int currentTotalTier,
        ref bool foundPlan,
        ref int bestWorstTier,
        ref int bestTotalTier)
    {
        if (!character.CanOperateWith(
            gripType))
        {
            return;
        }

        int availableGripCount =
            gripType == GripType.Hand
                ? availableHands
                : availableMouth;

        int minimumGripCount =
            item.GetMinimumUseGripCount(
                gripType
            );

        if (availableGripCount <
            minimumGripCount)
        {
            return;
        }

        for (int gripCount =
                 minimumGripCount;
             gripCount <=
                 availableGripCount;
             gripCount++)
        {
            ResolvedItemHandling resolved =
                ItemHandlingResolver.Resolve(
                    item,
                    character,
                    gripType,
                    gripCount
                );

            if (resolved == null ||
                !resolved.canUse)
            {
                continue;
            }

            int newWorstTier =
                Mathf.Max(
                    currentWorstTier,
                    (int)resolved.tier
                );

            int newTotalTier =
                currentTotalTier +
                (int)resolved.tier;

            if (CannotBeatBest(
                foundPlan,
                newWorstTier,
                newTotalTier,
                bestWorstTier,
                bestTotalTier))
            {
                continue;
            }

            currentPlan[itemIndex] =
                resolved;

            int remainingHands =
                availableHands;

            int remainingMouth =
                availableMouth;

            if (gripType ==
                GripType.Hand)
            {
                remainingHands -=
                    gripCount;
            }
            else
            {
                remainingMouth -=
                    gripCount;
            }

            Search(
                items,
                character,
                itemIndex + 1,
                remainingHands,
                remainingMouth,
                currentPlan,
                bestPlan,
                newWorstTier,
                newTotalTier,
                ref foundPlan,
                ref bestWorstTier,
                ref bestTotalTier
            );
        }
    }

    private static bool IsBetterPlan(
        bool foundPlan,
        int worstTier,
        int totalTier,
        int bestWorstTier,
        int bestTotalTier)
    {
        if (!foundPlan)
            return true;

        if (worstTier <
            bestWorstTier)
        {
            return true;
        }

        if (worstTier >
            bestWorstTier)
        {
            return false;
        }

        return totalTier <
               bestTotalTier;
    }

    private static bool CannotBeatBest(
        bool foundPlan,
        int worstTier,
        int totalTier,
        int bestWorstTier,
        int bestTotalTier)
    {
        if (!foundPlan)
            return false;

        if (worstTier >
            bestWorstTier)
        {
            return true;
        }

        return worstTier ==
                   bestWorstTier &&
               totalTier >=
                   bestTotalTier;
    }
}