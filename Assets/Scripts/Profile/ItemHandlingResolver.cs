using System;
using UnityEngine;

public enum ItemHandlingTier
{
    Effortless,
    Comfortable,
    Strained,
    SeverelyStrained,
    Unusable
}

public enum ItemHandlingFailureReason
{
    None,
    MissingItem,
    MissingCharacterHandling,
    GripCannotHoldItem,
    GripCannotUseItem,
    NoAvailableGrip,
    NotEnoughAssignedGrips,
    NotEnoughAvailableGrips,
    TooHeavy
}

[Serializable]
public class ResolvedItemHandling
{
    public GripType gripType;

    [Min(0)]
    public int availableGripCount;

    [Min(0)]
    public int assignedGripCount;

    [Min(1)]
    public int minimumUseGripCount = 1;

    [Min(0f)]
    public float itemWeight;

    [Min(0f)]
    public float physicalStrength;

    [Min(0f)]
    public float gripCapacity;

    [Min(0f)]
    public float loadRatio;

    public ItemHandlingTier tier =
        ItemHandlingTier.Unusable;

    public bool canHold;
    public bool canUse;

    public ItemHandlingFailureReason
        holdFailureReason;

    public ItemHandlingFailureReason
        useFailureReason;
}

public static class ItemHandlingResolver
{
    private const float EffortlessMaximum = 0.5f;
    private const float ComfortableMaximum = 0.85f;
    private const float StrainedMaximum = 1.1f;
    private const float SevereMaximum = 1.5f;

    public static ResolvedItemHandling ResolveBestHold(
        ItemDefinition item,
        CharacterHandlingProfile character,
        GripType gripType)
    {
        return ResolveBest(
            item,
            character,
            gripType,
            false
        );
    }

    public static ResolvedItemHandling ResolveBestUse(
        ItemDefinition item,
        CharacterHandlingProfile character,
        GripType gripType)
    {
        return ResolveBest(
            item,
            character,
            gripType,
            true
        );
    }

    public static ResolvedItemHandling Resolve(
        ItemDefinition item,
        CharacterHandlingProfile character,
        GripType gripType,
        int assignedGripCount)
    {
        ResolvedItemHandling result =
            CreateBaseResult(
                item,
                character,
                gripType,
                assignedGripCount
            );

        if (item == null)
        {
            result.holdFailureReason =
                ItemHandlingFailureReason.MissingItem;

            result.useFailureReason =
                ItemHandlingFailureReason.MissingItem;

            return result;
        }

        if (character == null)
        {
            result.holdFailureReason =
                ItemHandlingFailureReason
                    .MissingCharacterHandling;

            result.useFailureReason =
                ItemHandlingFailureReason
                    .MissingCharacterHandling;

            return result;
        }

        if (!CanHoldWith(
                character,
                gripType))
        {
            result.holdFailureReason =
                ItemHandlingFailureReason
                    .GripCannotHoldItem;

            result.useFailureReason =
                ItemHandlingFailureReason
                    .GripCannotHoldItem;

            return result;
        }

        if (result.availableGripCount <= 0)
        {
            result.holdFailureReason =
                ItemHandlingFailureReason
                    .NoAvailableGrip;

            result.useFailureReason =
                ItemHandlingFailureReason
                    .NoAvailableGrip;

            return result;
        }

        if (assignedGripCount <= 0)
        {
            result.holdFailureReason =
                ItemHandlingFailureReason
                    .NotEnoughAssignedGrips;

            result.useFailureReason =
                ItemHandlingFailureReason
                    .NotEnoughAssignedGrips;

            return result;
        }

        if (assignedGripCount >
            result.availableGripCount)
        {
            result.holdFailureReason =
                ItemHandlingFailureReason
                    .NotEnoughAvailableGrips;

            result.useFailureReason =
                ItemHandlingFailureReason
                    .NotEnoughAvailableGrips;

            return result;
        }

        result.gripCapacity =
            Mathf.Max(
                0f,
                result.physicalStrength *
                assignedGripCount
            );

        result.loadRatio =
            CalculateLoadRatio(
                result.itemWeight,
                result.gripCapacity
            );

        result.tier =
            GetTier(
                result.loadRatio
            );

        if (result.tier ==
            ItemHandlingTier.Unusable)
        {
            result.holdFailureReason =
                ItemHandlingFailureReason.TooHeavy;

            result.useFailureReason =
                ItemHandlingFailureReason.TooHeavy;

            return result;
        }

        result.canHold = true;

        result.holdFailureReason =
            ItemHandlingFailureReason.None;

        if (!CanUseWith(
            item,
            character,
            gripType))
        {
            result.useFailureReason =
                ItemHandlingFailureReason
                    .GripCannotUseItem;

            return result;
        }

        if (assignedGripCount <
            result.minimumUseGripCount)
        {
            result.useFailureReason =
                ItemHandlingFailureReason
                    .NotEnoughAssignedGrips;

            return result;
        }

        result.canUse = true;

        result.useFailureReason =
            ItemHandlingFailureReason.None;

        return result;
    }

    private static ResolvedItemHandling ResolveBest(
        ItemDefinition item,
        CharacterHandlingProfile character,
        GripType gripType,
        bool resolvingUse)
    {
        if (item == null ||
            character == null)
        {
            return Resolve(
                item,
                character,
                gripType,
                0
            );
        }

        int availableGripCount =
            GetAvailableGripCount(
                character,
                gripType
            );

        int minimumGripCount =
            resolvingUse
                ? item.GetMinimumUseGripCount(
                    gripType
                )
                : 1;

        if (availableGripCount <
            minimumGripCount)
        {
            ResolvedItemHandling failure =
                Resolve(
                    item,
                    character,
                    gripType,
                    availableGripCount
                );

            if (availableGripCount <= 0)
            {
                failure.holdFailureReason =
                    ItemHandlingFailureReason
                        .NoAvailableGrip;

                failure.useFailureReason =
                    ItemHandlingFailureReason
                        .NoAvailableGrip;
            }
            else if (
                failure.useFailureReason ==
                    ItemHandlingFailureReason.None ||
                failure.useFailureReason ==
                    ItemHandlingFailureReason
                        .NotEnoughAssignedGrips)
            {
                failure.useFailureReason =
                    ItemHandlingFailureReason
                        .NotEnoughAvailableGrips;
            }

            return failure;
        }

        ResolvedItemHandling lastResult = null;

        for (int gripCount = minimumGripCount;
             gripCount <= availableGripCount;
             gripCount++)
        {
            ResolvedItemHandling result =
                Resolve(
                    item,
                    character,
                    gripType,
                    gripCount
                );

            lastResult = result;

            bool valid =
                resolvingUse
                    ? result.canUse
                    : result.canHold;

            if (!valid)
                continue;

            // Use the fewest grips that avoid severe strain.
            // More grips may still be assigned deliberately later.
            if (result.tier <=
                ItemHandlingTier.Strained)
            {
                return result;
            }
        }

        return lastResult ??
               Resolve(
                   item,
                   character,
                   gripType,
                   minimumGripCount
               );
    }

    private static ResolvedItemHandling
        CreateBaseResult(
            ItemDefinition item,
            CharacterHandlingProfile character,
            GripType gripType,
            int assignedGripCount)
    {
        return new ResolvedItemHandling
        {
            gripType = gripType,

            availableGripCount =
                GetAvailableGripCount(
                    character,
                    gripType
                ),

            assignedGripCount =
                Mathf.Max(
                    0,
                    assignedGripCount
                ),

            minimumUseGripCount =
                item != null
                    ? item.GetMinimumUseGripCount(
                        gripType
                    )
                    : 1,

            itemWeight =
                item != null
                    ? Mathf.Max(
                        0f,
                        item.weight
                    )
                    : 0f,

            physicalStrength =
                character != null
                    ? Mathf.Max(
                        0f,
                        character.physicalStrength
                    )
                    : 0f,

            gripCapacity = 0f,
            loadRatio = 0f,

            tier =
                ItemHandlingTier.Unusable,

            canHold = false,
            canUse = false,

            holdFailureReason =
                ItemHandlingFailureReason.None,

            useFailureReason =
                ItemHandlingFailureReason.None
        };
    }

    private static int GetAvailableGripCount(
        CharacterHandlingProfile character,
        GripType gripType)
    {
        if (character == null)
            return 0;

        switch (gripType)
        {
            case GripType.Mouth:
                return Mathf.Max(
                    0,
                    character.mouthGripCount
                );

            case GripType.Hand:
            default:
                return Mathf.Max(
                    0,
                    character.handGripCount
                );
        }
    }

    private static bool CanHoldWith(
        ItemDefinition item,
        CharacterHandlingProfile character,
        GripType gripType)
    {
        if (item == null ||
            character == null)
        {
            return false;
        }

        switch (gripType)
        {
            case GripType.Mouth:
                return character.mouthGripCount > 0;

            case GripType.Hand:
            default:
                return character.handGripCount > 0;
        }
    }

    private static bool CanUseWith(
        ItemDefinition item,
        CharacterHandlingProfile character,
        GripType gripType)
    {
        if (item == null ||
            character == null ||
            !character.CanOperateWith(
                gripType))
        {
            return false;
        }

        switch (gripType)
        {
            case GripType.Mouth:
                return item.canUseWithMouth;

            case GripType.Hand:
            default:
                return item.canUseWithHands;
        }
    }

    public static bool TryResolveBestHold(
        ItemDefinition item,
        CharacterHandlingProfile character,
        int availableHands,
        int availableMouth,
        out ResolvedItemHandling result)
    {
        result = null;

        if (item == null ||
            character == null)
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

        if (TryResolveAvailableGrip(
            item,
            character,
            GripType.Hand,
            availableHands,
            false,
            out result))
        {
            return true;
        }

        return TryResolveAvailableGrip(
            item,
            character,
            GripType.Mouth,
            availableMouth,
            false,
            out result
        );
    }

    private static bool TryResolveAvailableGrip(
        ItemDefinition item,
        CharacterHandlingProfile character,
        GripType gripType,
        int availableGripCount,
        bool resolvingUse,
        out ResolvedItemHandling result)
    {
        result = null;

        int minimumGripCount =
            resolvingUse
                ? item.GetMinimumUseGripCount(
                    gripType
                )
                : 1;

        if (availableGripCount <
            minimumGripCount)
        {
            return false;
        }

        ResolvedItemHandling severeResult =
            null;

        for (int gripCount =
                 minimumGripCount;
             gripCount <=
                 availableGripCount;
             gripCount++)
        {
            ResolvedItemHandling current =
                Resolve(
                    item,
                    character,
                    gripType,
                    gripCount
                );

            bool valid =
                resolvingUse
                    ? current.canUse
                    : current.canHold;

            if (!valid)
                continue;

            if (current.tier <=
                ItemHandlingTier.Strained)
            {
                result = current;
                return true;
            }

            if (severeResult == null)
            {
                severeResult =
                    current;
            }
        }

        result =
            severeResult;

        return result != null;
    }

    private static float CalculateLoadRatio(
        float itemWeight,
        float gripCapacity)
    {
        itemWeight =
            Mathf.Max(
                0f,
                itemWeight
            );

        gripCapacity =
            Mathf.Max(
                0f,
                gripCapacity
            );

        if (itemWeight <= 0f)
            return 0f;

        if (gripCapacity <= 0f)
            return float.PositiveInfinity;

        return itemWeight /
               gripCapacity;
    }

    private static ItemHandlingTier GetTier(
        float loadRatio)
    {
        if (float.IsInfinity(loadRatio) ||
            float.IsNaN(loadRatio))
        {
            return ItemHandlingTier.Unusable;
        }

        if (loadRatio <= EffortlessMaximum)
            return ItemHandlingTier.Effortless;

        if (loadRatio <= ComfortableMaximum)
            return ItemHandlingTier.Comfortable;

        if (loadRatio <= StrainedMaximum)
            return ItemHandlingTier.Strained;

        if (loadRatio <= SevereMaximum)
        {
            return ItemHandlingTier
                .SeverelyStrained;
        }

        return ItemHandlingTier.Unusable;
    }
}