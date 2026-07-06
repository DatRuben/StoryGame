using System;
using UnityEngine;

public enum HoldingSlot
{
    LeftHand,
    RightHand,
    BothHands,
    Mouth
}

public class PlayerHolding : MonoBehaviour
{
    [Header("Body Holding Rules")]
    [SerializeField] private bool canHoldItemInMouth = false;

    [Header("Current Held Items")]
    [SerializeField] private ItemData leftHandItem;
    [SerializeField] private ItemData rightHandItem;
    [SerializeField] private ItemData twoHandedItem;
    [SerializeField] private ItemData mouthItem;

    public bool CanHoldItemInMouth => canHoldItemInMouth;

    public ItemData LeftHandItem => leftHandItem;
    public ItemData RightHandItem => rightHandItem;
    public ItemData TwoHandedItem => twoHandedItem;
    public ItemData MouthItem => mouthItem;

    public bool IsHoldingTwoHanded => twoHandedItem != null;

    public bool HasAnyHeldItem =>
        leftHandItem != null ||
        rightHandItem != null ||
        twoHandedItem != null ||
        mouthItem != null;

    public event Action OnHoldingChanged;

    private void OnValidate()
    {
        ValidateHoldingState();
    }

    public void ApplySubraceDefinition(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return;

        canHoldItemInMouth =
            subraceDefinition.canHoldItemInMouth;

        if (!canHoldItemInMouth)
            mouthItem = null;

        OnHoldingChanged?.Invoke();
    }

    private void ValidateHoldingState()
    {
        if (twoHandedItem != null)
        {
            leftHandItem = null;
            rightHandItem = null;
        }

        if (leftHandItem != null &&
            leftHandItem.handUsage == ItemHandUsage.TwoHanded)
        {
            leftHandItem = null;
        }

        if (rightHandItem != null &&
            rightHandItem.handUsage == ItemHandUsage.TwoHanded)
        {
            rightHandItem = null;
        }

        if (twoHandedItem != null &&
            twoHandedItem.handUsage != ItemHandUsage.TwoHanded)
        {
            twoHandedItem = null;
        }
    }

    public bool TryHoldItem(
        ItemData item,
        HoldingSlot holdingSlot)
    {
        if (item == null)
            return false;

        switch (holdingSlot)
        {
            case HoldingSlot.LeftHand:
                if (item.handUsage == ItemHandUsage.TwoHanded)
                    return TryHoldTwoHanded(item);

                return TryHoldInLeftHand(item);

            case HoldingSlot.RightHand:
                if (item.handUsage == ItemHandUsage.TwoHanded)
                    return TryHoldTwoHanded(item);

                return TryHoldInRightHand(item);

            case HoldingSlot.BothHands:
                return TryHoldTwoHanded(item);

            case HoldingSlot.Mouth:
                return TryHoldInMouth(item);

            default:
                return false;
        }
    }

    public bool TryHoldInLeftHand(ItemData item)
    {
        if (!CanHoldOneHandedItem(item))
            return false;

        if (twoHandedItem != null)
            return false;

        leftHandItem = item;

        OnHoldingChanged?.Invoke();

        return true;
    }

    public bool TryHoldInRightHand(ItemData item)
    {
        if (!CanHoldOneHandedItem(item))
            return false;

        if (twoHandedItem != null)
            return false;

        rightHandItem = item;

        OnHoldingChanged?.Invoke();

        return true;
    }

    public bool TryHoldTwoHanded(ItemData item)
    {
        if (item == null)
            return false;

        if (item.handUsage != ItemHandUsage.TwoHanded)
            return false;

        leftHandItem = null;
        rightHandItem = null;
        twoHandedItem = item;

        OnHoldingChanged?.Invoke();

        return true;
    }

    public bool TryHoldInMouth(ItemData item)
    {
        if (!canHoldItemInMouth)
        {
            Debug.Log("This race cannot hold items in its mouth.");
            return false;
        }

        if (item == null)
            return false;

        mouthItem = item;

        OnHoldingChanged?.Invoke();

        return true;
    }

    public void ClearLeftHand()
    {
        if (leftHandItem == null)
            return;

        leftHandItem = null;

        OnHoldingChanged?.Invoke();
    }

    public void ClearRightHand()
    {
        if (rightHandItem == null)
            return;

        rightHandItem = null;

        OnHoldingChanged?.Invoke();
    }

    public void ClearTwoHanded()
    {
        if (twoHandedItem == null)
            return;

        twoHandedItem = null;

        OnHoldingChanged?.Invoke();
    }

    public void ClearMouth()
    {
        if (mouthItem == null)
            return;

        mouthItem = null;

        OnHoldingChanged?.Invoke();
    }

    public void ClearSlot(HoldingSlot holdingSlot)
    {
        switch (holdingSlot)
        {
            case HoldingSlot.LeftHand:
                ClearLeftHand();
                break;

            case HoldingSlot.RightHand:
                ClearRightHand();
                break;

            case HoldingSlot.BothHands:
                ClearTwoHanded();
                break;

            case HoldingSlot.Mouth:
                ClearMouth();
                break;
        }
    }

    public void ClearAllHolding()
    {
        bool changed =
            leftHandItem != null ||
            rightHandItem != null ||
            twoHandedItem != null ||
            mouthItem != null;

        leftHandItem = null;
        rightHandItem = null;
        twoHandedItem = null;
        mouthItem = null;

        if (changed)
            OnHoldingChanged?.Invoke();
    }

    private bool CanHoldOneHandedItem(ItemData item)
    {
        if (item == null)
            return false;

        return item.handUsage == ItemHandUsage.OneHanded;
    }
}