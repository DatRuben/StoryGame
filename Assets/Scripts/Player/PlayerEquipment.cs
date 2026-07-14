using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    [Header("Equipment")]
    [SerializeField] private ItemDefinition equippedSaddle;
    [SerializeField] private ItemDefinition equippedArmor;
    [SerializeField] private ItemDefinition equippedHelmet;
    [SerializeField] private ItemDefinition equippedAccessory;

    [Header("Body Equipment Rules")]
    [SerializeField] private bool canEquipSaddles = false;

    [Header("Saddle Turret Settings")]
    [Tooltip("0 = Weapon Set 1, 1 = Weapon Set 2")]
    [SerializeField] private int manualSaddleTurretWeaponSetIndex = 1;

    public ItemDefinition EquippedSaddle => equippedSaddle;
    public ItemDefinition EquippedArmor => equippedArmor;
    public ItemDefinition EquippedHelmet => equippedHelmet;
    public ItemDefinition EquippedAccessory => equippedAccessory;

    public bool HasEquippedSaddle => equippedSaddle != null;
    public bool CanEquipSaddles => canEquipSaddles;

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();
    }

    private void Start()
    {
        RefreshSaddleWeaponSlotReservation();
    }

    private void OnValidate()
    {
        manualSaddleTurretWeaponSetIndex =
            Mathf.Clamp(manualSaddleTurretWeaponSetIndex, 0, 1);
    }

    public void ApplySubraceDefinition(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return;

        canEquipSaddles =
            subraceDefinition.canEquipSaddles;

        if (!canEquipSaddles && equippedSaddle != null)
        {
            UnequipSaddle();
        }

        OnEquipmentChanged?.Invoke();
    }

    public bool CanEquipItemToSlot(
        ItemDefinition item,
        EquipmentSlotType slotType)
    {
        if (item == null)
            return false;

        if (item.itemCategory != ItemCategory.Equipment)
            return false;

        if (item.equipmentSlotType != slotType)
            return false;

        if (slotType == EquipmentSlotType.Saddle &&
            !canEquipSaddles)
        {
            return false;
        }

        return true;
    }

    public bool TryEquipItemToSlot(
        ItemDefinition item,
        EquipmentSlotType slotType,
        out ItemDefinition replacedItem)
    {
        replacedItem = null;

        if (!CanEquipItemToSlot(item, slotType))
            return false;

        if (slotType == EquipmentSlotType.Saddle)
        {
            return TryEquipSaddle(
                item,
                out replacedItem
            );
        }

        replacedItem =
            GetEquippedItem(slotType);

        SetEquippedItemDirect(
            slotType,
            item
        );

        OnEquipmentChanged?.Invoke();

        return true;
    }

    public ItemDefinition UnequipSlot(EquipmentSlotType slotType)
    {
        if (slotType == EquipmentSlotType.Saddle)
            return UnequipSaddle();

        ItemDefinition oldItem =
            GetEquippedItem(slotType);

        if (oldItem == null)
            return null;

        SetEquippedItemDirect(
            slotType,
            null
        );

        OnEquipmentChanged?.Invoke();

        return oldItem;
    }

    public ItemDefinition GetEquippedItem(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Saddle:
                return equippedSaddle;

            case EquipmentSlotType.Armor:
                return equippedArmor;

            case EquipmentSlotType.Helmet:
                return equippedHelmet;

            case EquipmentSlotType.Accessory:
                return equippedAccessory;

            default:
                return null;
        }
    }

    private void SetEquippedItemDirect(
        EquipmentSlotType slotType,
        ItemDefinition item)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Saddle:
                equippedSaddle = item;
                break;

            case EquipmentSlotType.Armor:
                equippedArmor = item;
                break;

            case EquipmentSlotType.Helmet:
                equippedHelmet = item;
                break;

            case EquipmentSlotType.Accessory:
                equippedAccessory = item;
                break;
        }
    }

    public bool CanEquipSaddle(ItemDefinition item)
    {
        return CanEquipItemToSlot(
            item,
            EquipmentSlotType.Saddle
        );
    }

    public bool TryEquipSaddle(ItemDefinition saddleItem)
    {
        return TryEquipSaddle(
            saddleItem,
            out ItemDefinition ignoredOldSaddle
        );
    }

    public bool TryEquipSaddle(
        ItemDefinition saddleItem,
        out ItemDefinition replacedSaddle)
    {
        replacedSaddle = null;

        if (!CanEquipSaddle(saddleItem))
            return false;

        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();

        ItemDefinition oldSaddle =
            equippedSaddle;

        if (oldSaddle == saddleItem)
            return true;

        if (oldSaddle != null &&
            playerWeaponSlots != null)
        {
            playerWeaponSlots.ClearManualSaddleTurretReservation(
                oldSaddle
            );
        }

        equippedSaddle = null;

        if (saddleItem.hasManualSaddleTurret)
        {
            if (playerWeaponSlots == null)
            {
                RestoreOldSaddle(oldSaddle);
                return false;
            }

            bool reserved =
                playerWeaponSlots.TryReserveSetForManualSaddleTurret(
                    manualSaddleTurretWeaponSetIndex,
                    saddleItem
                );

            if (!reserved)
            {
                RestoreOldSaddle(oldSaddle);

                Debug.LogWarning(
                    "Could not equip saddle. Its manual turret could not reserve the selected weapon set."
                );

                return false;
            }
        }

        equippedSaddle = saddleItem;
        replacedSaddle = oldSaddle;

        OnEquipmentChanged?.Invoke();

        return true;
    }

    public ItemDefinition UnequipSaddle()
    {
        ItemDefinition oldSaddle =
            equippedSaddle;

        if (oldSaddle == null)
            return null;

        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();

        if (playerWeaponSlots != null)
        {
            playerWeaponSlots.ClearManualSaddleTurretReservation(
                oldSaddle
            );
        }

        equippedSaddle = null;

        OnEquipmentChanged?.Invoke();

        return oldSaddle;
    }

    public void RefreshSaddleWeaponSlotReservation()
    {
        if (equippedSaddle == null)
            return;

        if (!equippedSaddle.hasManualSaddleTurret)
            return;

        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();

        if (playerWeaponSlots == null)
            return;

        bool reserved =
            playerWeaponSlots.TryReserveSetForManualSaddleTurret(
                manualSaddleTurretWeaponSetIndex,
                equippedSaddle
            );

        if (!reserved)
        {
            Debug.LogWarning(
                "Could not reserve weapon set for manual saddle turret. The selected weapon set may already be occupied."
            );
        }
    }

    private void RestoreOldSaddle(ItemDefinition oldSaddle)
    {
        if (oldSaddle == null)
        {
            equippedSaddle = null;
            OnEquipmentChanged?.Invoke();
            return;
        }

        equippedSaddle = oldSaddle;

        if (oldSaddle.hasManualSaddleTurret &&
            playerWeaponSlots != null)
        {
            playerWeaponSlots.TryReserveSetForManualSaddleTurret(
                manualSaddleTurretWeaponSetIndex,
                oldSaddle
            );
        }

        OnEquipmentChanged?.Invoke();
    }
}