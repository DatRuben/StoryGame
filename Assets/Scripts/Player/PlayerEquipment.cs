using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    [Header("Equipment")]
    [SerializeField] private ItemData equippedSaddle;

    [Header("Race Rules")]
    [SerializeField] private bool canEquipSaddles = false;

    [Header("Saddle Turret Settings")]
    [Tooltip("0 = Weapon Set 1, 1 = Weapon Set 2")]
    [SerializeField] private int manualSaddleTurretWeaponSetIndex = 1;

    public ItemData EquippedSaddle => equippedSaddle;
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

    public void ApplyRaceProfile(RaceProfile raceProfile)
    {
        if (raceProfile == null)
            return;

        canEquipSaddles = raceProfile.canEquipSaddles;

        if (!canEquipSaddles && equippedSaddle != null)
        {
            UnequipSaddle();
        }

        OnEquipmentChanged?.Invoke();
    }

    public bool CanEquipSaddle(ItemData item)
    {
        if (!canEquipSaddles)
            return false;

        if (item == null)
            return false;

        // Temporary rule:
        // until we add EquipmentSlotType, any Equipment item can go in SaddleSlot.
        return item.itemCategory == ItemCategory.Equipment;
    }

    public bool TryEquipSaddle(ItemData saddleItem)
    {
        return TryEquipSaddle(
            saddleItem,
            out ItemData ignoredOldSaddle
        );
    }

    public bool TryEquipSaddle(
        ItemData saddleItem,
        out ItemData replacedSaddle)
    {
        replacedSaddle = null;

        if (!CanEquipSaddle(saddleItem))
            return false;

        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();

        ItemData oldSaddle =
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

    public ItemData UnequipSaddle()
    {
        ItemData oldSaddle =
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

    private void RestoreOldSaddle(ItemData oldSaddle)
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