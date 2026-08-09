using System;
using UnityEngine;

public sealed class PlayerEquipment :
    MonoBehaviour
{
    public const int ArmAttachmentCount = 2;

    private InventoryItemInstance equippedArmor;
    private InventoryItemInstance equippedHelmet;
    private InventoryItemInstance equippedSaddle;
    private InventoryItemInstance equippedAccessory;
    private InventoryItemInstance equippedGauntlets;

    private readonly InventoryItemInstance[]
        armAttachments =
        new InventoryItemInstance[
            ArmAttachmentCount
        ];

    private bool canEquipSaddles;

    public InventoryItemInstance EquippedArmor =>
        equippedArmor;

    public InventoryItemInstance EquippedHelmet =>
        equippedHelmet;

    public InventoryItemInstance EquippedSaddle =>
        equippedSaddle;

    public InventoryItemInstance EquippedAccessory =>
        equippedAccessory;

    public InventoryItemInstance EquippedGauntlets =>
        equippedGauntlets;

    public bool HasEquippedSaddle =>
        equippedSaddle != null;

    public bool CanEquipSaddles =>
        canEquipSaddles;

    public event Action OnEquipmentChanged;

    public bool Configure(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return false;

        bool newCanEquipSaddles =
            subraceDefinition.canEquipSaddles;

        if (!newCanEquipSaddles &&
            equippedSaddle != null)
        {
            return false;
        }

        if (canEquipSaddles ==
            newCanEquipSaddles)
        {
            return true;
        }

        canEquipSaddles =
            newCanEquipSaddles;

        OnEquipmentChanged?.Invoke();

        return true;
    }

    public InventoryItemInstance
        GetEquippedItem(
            EquipmentSlotType slotType,
            int slotIndex = 0)
    {
        if (!IsValidSlot(
            slotType,
            slotIndex))
        {
            return null;
        }

        switch (slotType)
        {
            case EquipmentSlotType.Armor:
                return equippedArmor;

            case EquipmentSlotType.Helmet:
                return equippedHelmet;

            case EquipmentSlotType.Saddle:
                return equippedSaddle;

            case EquipmentSlotType.Accessory:
                return equippedAccessory;

            case EquipmentSlotType.Gauntlets:
                return equippedGauntlets;

            case EquipmentSlotType.ArmAttachment:
                return armAttachments[
                    slotIndex
                ];

            default:
                return null;
        }
    }

    public InventoryItemInstance
        GetArmAttachment(
            int armIndex)
    {
        return GetEquippedItem(
            EquipmentSlotType.ArmAttachment,
            armIndex
        );
    }

    public bool IsEquipped(
        InventoryItemInstance itemInstance)
    {
        return TryFindEquippedItem(
            itemInstance,
            out _,
            out _
        );
    }

    public bool TryFindEquippedItem(
        InventoryItemInstance itemInstance,
        out EquipmentSlotType slotType,
        out int slotIndex)
    {
        slotType =
            EquipmentSlotType.Armor;

        slotIndex = -1;

        if (itemInstance == null)
            return false;

        EquipmentSlotType[] singleSlots =
        {
            EquipmentSlotType.Armor,
            EquipmentSlotType.Helmet,
            EquipmentSlotType.Saddle,
            EquipmentSlotType.Accessory,
            EquipmentSlotType.Gauntlets
        };

        for (int i = 0;
             i < singleSlots.Length;
             i++)
        {
            EquipmentSlotType currentSlot =
                singleSlots[i];

            if (!ReferenceEquals(
                GetEquippedItem(
                    currentSlot
                ),
                itemInstance))
            {
                continue;
            }

            slotType =
                currentSlot;

            slotIndex = 0;

            return true;
        }

        for (int armIndex = 0;
             armIndex <
                ArmAttachmentCount;
             armIndex++)
        {
            if (!ReferenceEquals(
                armAttachments[
                    armIndex
                ],
                itemInstance))
            {
                continue;
            }

            slotType =
                EquipmentSlotType
                    .ArmAttachment;

            slotIndex =
                armIndex;

            return true;
        }

        return false;
    }

    public bool CanEquipItemToSlot(
        InventoryItemInstance itemInstance,
        EquipmentSlotType slotType,
        int slotIndex = 0)
    {
        if (itemInstance == null ||
            itemInstance.IsEmpty ||
            itemInstance.Definition == null)
        {
            return false;
        }

        if (!IsValidSlot(
            slotType,
            slotIndex))
        {
            return false;
        }

        ItemDefinition definition =
            itemInstance.Definition;

        if (definition.itemCategory !=
            ItemCategory.Equipment)
        {
            return false;
        }

        if (definition.equipmentSlotType !=
            slotType)
        {
            return false;
        }

        if (slotType ==
                EquipmentSlotType.Saddle &&
            !canEquipSaddles)
        {
            return false;
        }

        if (!IsEquipped(itemInstance))
            return true;

        return ReferenceEquals(
            GetEquippedItem(
                slotType,
                slotIndex
            ),
            itemInstance
        );
    }

    internal bool TryEquipItemToSlot(
        InventoryItemInstance itemInstance,
        EquipmentSlotType slotType,
        int slotIndex,
        out InventoryItemInstance replacedItem)
    {
        replacedItem = null;

        if (!CanEquipItemToSlot(
            itemInstance,
            slotType,
            slotIndex))
        {
            return false;
        }

        InventoryItemInstance currentItem =
            GetEquippedItem(
                slotType,
                slotIndex
            );

        if (ReferenceEquals(
            currentItem,
            itemInstance))
        {
            return true;
        }

        if (currentItem != null)
        {
            replacedItem =
                currentItem;

            UnsubscribeItem(
                currentItem
            );
        }

        SetEquippedItem(
            slotType,
            slotIndex,
            itemInstance
        );

        SubscribeItem(
            itemInstance
        );

        OnEquipmentChanged?.Invoke();

        return true;
    }

    internal InventoryItemInstance
        UnequipSlot(
            EquipmentSlotType slotType,
            int slotIndex = 0)
    {
        if (!IsValidSlot(
            slotType,
            slotIndex))
        {
            return null;
        }

        InventoryItemInstance oldItem =
            GetEquippedItem(
                slotType,
                slotIndex
            );

        if (oldItem == null)
            return null;

        SetEquippedItem(
            slotType,
            slotIndex,
            null
        );

        UnsubscribeItem(
            oldItem
        );

        OnEquipmentChanged?.Invoke();

        return oldItem;
    }

    private void SetEquippedItem(
        EquipmentSlotType slotType,
        int slotIndex,
        InventoryItemInstance itemInstance)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Armor:
                equippedArmor =
                    itemInstance;
                break;

            case EquipmentSlotType.Helmet:
                equippedHelmet =
                    itemInstance;
                break;

            case EquipmentSlotType.Saddle:
                equippedSaddle =
                    itemInstance;
                break;

            case EquipmentSlotType.Accessory:
                equippedAccessory =
                    itemInstance;
                break;

            case EquipmentSlotType.Gauntlets:
                equippedGauntlets =
                    itemInstance;
                break;

            case EquipmentSlotType.ArmAttachment:
                armAttachments[
                    slotIndex
                ] = itemInstance;
                break;
        }
    }

    private static bool IsValidSlot(
        EquipmentSlotType slotType,
        int slotIndex)
    {
        if (slotType ==
            EquipmentSlotType.ArmAttachment)
        {
            return slotIndex >= 0 &&
                   slotIndex <
                       ArmAttachmentCount;
        }

        return slotIndex == 0;
    }

    private void SubscribeItem(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return;

        itemInstance.Changed -=
            OnEquippedItemChanged;

        itemInstance.Changed +=
            OnEquippedItemChanged;
    }

    private void UnsubscribeItem(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return;

        itemInstance.Changed -=
            OnEquippedItemChanged;
    }

    private void OnEquippedItemChanged()
    {
        if (RemoveEmptyItems())
            return;

        OnEquipmentChanged?.Invoke();
    }

    private bool RemoveEmptyItems()
    {
        bool changed = false;

        EquipmentSlotType[] singleSlots =
        {
            EquipmentSlotType.Armor,
            EquipmentSlotType.Helmet,
            EquipmentSlotType.Saddle,
            EquipmentSlotType.Accessory,
            EquipmentSlotType.Gauntlets
        };

        for (int i = 0;
             i < singleSlots.Length;
             i++)
        {
            EquipmentSlotType slotType =
                singleSlots[i];

            InventoryItemInstance item =
                GetEquippedItem(
                    slotType
                );

            if (item == null ||
                !item.IsEmpty)
            {
                continue;
            }

            SetEquippedItem(
                slotType,
                0,
                null
            );

            UnsubscribeItem(
                item
            );

            changed = true;
        }

        for (int armIndex = 0;
             armIndex <
                ArmAttachmentCount;
             armIndex++)
        {
            InventoryItemInstance item =
                armAttachments[
                    armIndex
                ];

            if (item == null ||
                !item.IsEmpty)
            {
                continue;
            }

            armAttachments[
                armIndex
            ] = null;

            UnsubscribeItem(
                item
            );

            changed = true;
        }

        if (changed)
            OnEquipmentChanged?.Invoke();

        return changed;
    }

    private void OnDestroy()
    {
        UnsubscribeItem(
            equippedArmor
        );

        UnsubscribeItem(
            equippedHelmet
        );

        UnsubscribeItem(
            equippedSaddle
        );

        UnsubscribeItem(
            equippedAccessory
        );

        UnsubscribeItem(
            equippedGauntlets
        );

        for (int armIndex = 0;
             armIndex <
                ArmAttachmentCount;
             armIndex++)
        {
            UnsubscribeItem(
                armAttachments[
                    armIndex
                ]
            );
        }
    }
}