using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerGripState :
    MonoBehaviour
{
    [Header("Available Grips")]
    [SerializeField]
    [Range(0, 2)]
    private int handGripCount = 2;

    [SerializeField]
    [Range(0, 1)]
    private int mouthGripCount = 0;

    private readonly InventoryItemInstance[] handItems =
        new InventoryItemInstance[2];

    private InventoryItemInstance mouthItem;

    public int HandGripCount =>
        handGripCount;

    public int MouthGripCount =>
        mouthGripCount;

    public bool HasAnyHeldItem =>
        handItems[0] != null ||
        handItems[1] != null ||
        mouthItem != null;

    public event Action Changed;

    internal bool Configure(
        CharacterGripProfile profile)
    {
        if (profile == null)
        {
            profile =
                CharacterGripProfile
                    .CreateHumanoidDefault();
        }

        int newHandGripCount =
            Mathf.Clamp(
                profile.handGripCount,
                0,
                2
            );

        int newMouthGripCount =
            Mathf.Clamp(
                profile.mouthGripCount,
                0,
                1
            );

        if (newHandGripCount < 2 &&
            handItems[1] != null)
        {
            return false;
        }

        if (newHandGripCount < 1 &&
            handItems[0] != null)
        {
            return false;
        }

        if (newMouthGripCount < 1 &&
            mouthItem != null)
        {
            return false;
        }

        bool changed =
            handGripCount !=
                newHandGripCount ||
            mouthGripCount !=
                newMouthGripCount;

        handGripCount =
            newHandGripCount;

        mouthGripCount =
            newMouthGripCount;

        if (changed)
            Changed?.Invoke();

        return true;
    }

    public InventoryItemInstance GetItem(
        GripType gripType,
        int gripIndex)
    {
        switch (gripType)
        {
            case GripType.Hand:
                if (gripIndex < 0 ||
                    gripIndex >= handGripCount)
                {
                    return null;
                }

                return handItems[gripIndex];

            case GripType.Mouth:
                if (gripIndex != 0 ||
                    mouthGripCount == 0)
                {
                    return null;
                }

                return mouthItem;

            default:
                return null;
        }
    }

    public bool IsHolding(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return false;

        return handItems[0] ==
                   itemInstance ||
               handItems[1] ==
                   itemInstance ||
               mouthItem ==
                   itemInstance;
    }

    public int GetAssignedGripCount(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return 0;

        int count = 0;

        if (handItems[0] ==
            itemInstance)
        {
            count++;
        }

        if (handItems[1] ==
            itemInstance)
        {
            count++;
        }

        if (mouthItem ==
            itemInstance)
        {
            count++;
        }

        return count;
    }

    public int GetFreeGripCount(
        GripType gripType)
    {
        switch (gripType)
        {
            case GripType.Hand:
                int freeHands = 0;

                for (int i = 0;
                     i < handGripCount;
                     i++)
                {
                    if (handItems[i] == null)
                        freeHands++;
                }

                return freeHands;

            case GripType.Mouth:
                return mouthGripCount > 0 &&
                       mouthItem == null
                    ? 1
                    : 0;

            default:
                return 0;
        }
    }

    internal bool TryHold(
        InventoryItemInstance itemInstance,
        GripType gripType,
        int gripCount)
    {
        if (itemInstance == null ||
            itemInstance.IsEmpty ||
            IsHolding(itemInstance) ||
            gripCount <= 0)
        {
            return false;
        }

        bool held;

        switch (gripType)
        {
            case GripType.Hand:
                held =
                    TryHoldWithHands(
                        itemInstance,
                        gripCount
                    );
                break;

            case GripType.Mouth:
                held =
                    TryHoldWithMouth(
                        itemInstance,
                        gripCount
                    );
                break;

            default:
                return false;
        }

        if (!held)
            return false;

        SubscribeItem(
            itemInstance
        );

        Changed?.Invoke();

        return true;
    }

    internal bool Release(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return false;

        bool removed = false;

        for (int i = 0;
             i < handItems.Length;
             i++)
        {
            if (handItems[i] !=
                itemInstance)
            {
                continue;
            }

            handItems[i] = null;
            removed = true;
        }

        if (mouthItem ==
            itemInstance)
        {
            mouthItem = null;
            removed = true;
        }

        if (!removed)
            return false;

        UnsubscribeItem(
            itemInstance
        );

        Changed?.Invoke();

        return true;
    }

    public void GetHeldItemsInCycleOrder(
        List<InventoryItemInstance> results)
    {
        if (results == null)
            return;

        results.Clear();

        AddUnique(
            results,
            handItems[0]
        );

        AddUnique(
            results,
            handItems[1]
        );

        AddUnique(
            results,
            mouthItem
        );
    }

    private bool TryHoldWithHands(
        InventoryItemInstance itemInstance,
        int gripCount)
    {
        if (gripCount >
                handGripCount ||
            GetFreeGripCount(
                GripType.Hand
            ) < gripCount)
        {
            return false;
        }

        int assigned = 0;

        for (int i = 0;
             i < handGripCount;
             i++)
        {
            if (handItems[i] != null)
                continue;

            handItems[i] =
                itemInstance;

            assigned++;

            if (assigned ==
                gripCount)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryHoldWithMouth(
        InventoryItemInstance itemInstance,
        int gripCount)
    {
        if (gripCount != 1 ||
            mouthGripCount == 0 ||
            mouthItem != null)
        {
            return false;
        }

        mouthItem =
            itemInstance;

        return true;
    }

    private void SubscribeItem(
        InventoryItemInstance itemInstance)
    {
        itemInstance.Changed -=
            OnHeldItemChanged;

        itemInstance.Changed +=
            OnHeldItemChanged;
    }

    private void UnsubscribeItem(
        InventoryItemInstance itemInstance)
    {
        itemInstance.Changed -=
            OnHeldItemChanged;
    }

    private void OnHeldItemChanged()
    {
        InventoryItemInstance emptyItem =
            FindEmptyHeldItem();

        if (emptyItem != null)
        {
            Release(
                emptyItem
            );

            return;
        }

        Changed?.Invoke();
    }

    private InventoryItemInstance
        FindEmptyHeldItem()
    {
        if (handItems[0] != null &&
            handItems[0].IsEmpty)
        {
            return handItems[0];
        }

        if (handItems[1] != null &&
            handItems[1].IsEmpty)
        {
            return handItems[1];
        }

        if (mouthItem != null &&
            mouthItem.IsEmpty)
        {
            return mouthItem;
        }

        return null;
    }

    private static void AddUnique(
        List<InventoryItemInstance> results,
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null ||
            results.Contains(itemInstance))
        {
            return;
        }

        results.Add(
            itemInstance
        );
    }

    private void OnDestroy()
    {
        if (handItems[0] != null)
        {
            UnsubscribeItem(
                handItems[0]
            );
        }

        if (handItems[1] != null &&
            handItems[1] !=
                handItems[0])
        {
            UnsubscribeItem(
                handItems[1]
            );
        }

        if (mouthItem != null &&
            mouthItem !=
                handItems[0] &&
            mouthItem !=
                handItems[1])
        {
            UnsubscribeItem(
                mouthItem
            );
        }
    }
}