using System;
using UnityEngine;

public enum WeaponEquipPoint
{
    LeftHand,
    RightHand,
    BothHands,
    Mouth
}

public enum WeaponSetMode
{
    Empty,
    OneHandedPair,
    TwoHandedWeapon,
    MouthWeapon,
    ManualSaddleTurret
}

[Serializable]
public class WeaponSet
{
    [Header("Mode")]
    [SerializeField] private WeaponSetMode mode = WeaponSetMode.Empty;

    [Header("One-Handed Pair")]
    [SerializeField] private ItemData leftHandWeapon;
    [SerializeField] private ItemData rightHandWeapon;

    [Header("Single Weapon Modes")]
    [SerializeField] private ItemData twoHandedWeapon;
    [SerializeField] private ItemData mouthWeapon;

    [Header("Saddle Turret Reservation")]
    [SerializeField] private ItemData saddleSourceEquipment;

    public WeaponSetMode Mode => mode;

    public ItemData LeftHandWeapon => leftHandWeapon;
    public ItemData RightHandWeapon => rightHandWeapon;
    public ItemData TwoHandedWeapon => twoHandedWeapon;
    public ItemData MouthWeapon => mouthWeapon;
    public ItemData SaddleSourceEquipment => saddleSourceEquipment;

    public bool HasAnyWeaponOrReservation =>
        leftHandWeapon != null ||
        rightHandWeapon != null ||
        twoHandedWeapon != null ||
        mouthWeapon != null ||
        saddleSourceEquipment != null;

    public bool IsReservedByManualSaddleTurret =>
        mode == WeaponSetMode.ManualSaddleTurret &&
        saddleSourceEquipment != null;

    public bool TryEquipWeapon(
        ItemData weapon,
        WeaponEquipPoint equipPoint,
        bool canUseMouthWeapons)
    {
        if (!IsWeapon(weapon))
            return false;

        switch (equipPoint)
        {
            case WeaponEquipPoint.LeftHand:
                return TryEquipOneHandedWeapon(
                    weapon,
                    true
                );

            case WeaponEquipPoint.RightHand:
                return TryEquipOneHandedWeapon(
                    weapon,
                    false
                );

            case WeaponEquipPoint.BothHands:
                return TryEquipTwoHandedWeapon(weapon);

            case WeaponEquipPoint.Mouth:
                if (!canUseMouthWeapons)
                    return false;

                return TryEquipMouthWeapon(weapon);

            default:
                return false;
        }
    }

    private bool TryEquipOneHandedWeapon(
        ItemData weapon,
        bool leftHand)
    {
        if (!IsValidOneHandedHandWeapon(weapon))
            return false;

        if (mode != WeaponSetMode.Empty &&
            mode != WeaponSetMode.OneHandedPair)
        {
            return false;
        }

        mode = WeaponSetMode.OneHandedPair;
        ClearSingleWeaponModes();

        if (leftHand)
        {
            if (leftHandWeapon != null)
                return false;

            leftHandWeapon = weapon;
            return true;
        }

        if (rightHandWeapon != null)
            return false;

        rightHandWeapon = weapon;
        return true;
    }

    private bool TryEquipTwoHandedWeapon(ItemData weapon)
    {
        if (!IsValidTwoHandedHandWeapon(weapon))
            return false;

        if (mode != WeaponSetMode.Empty)
            return false;

        mode = WeaponSetMode.TwoHandedWeapon;
        ClearAllContents();

        twoHandedWeapon = weapon;

        return true;
    }

    private bool TryEquipMouthWeapon(ItemData weapon)
    {
        if (!IsValidMouthWeapon(weapon))
            return false;

        if (mode != WeaponSetMode.Empty)
            return false;

        mode = WeaponSetMode.MouthWeapon;
        ClearAllContents();

        mouthWeapon = weapon;

        return true;
    }

    public bool TryReserveForManualSaddleTurret(ItemData saddleEquipment)
    {
        if (saddleEquipment == null)
            return false;

        if (saddleEquipment.itemCategory != ItemCategory.Equipment)
            return false;

        if (!saddleEquipment.hasManualSaddleTurret)
            return false;

        if (mode != WeaponSetMode.Empty)
            return false;

        mode = WeaponSetMode.ManualSaddleTurret;
        ClearAllContents();

        saddleSourceEquipment = saddleEquipment;

        return true;
    }

    public bool ClearManualSaddleTurretReservation(ItemData saddleEquipment)
    {
        if (mode != WeaponSetMode.ManualSaddleTurret)
            return false;

        if (saddleSourceEquipment != saddleEquipment)
            return false;

        Clear();
        return true;
    }

    public ItemData GetWeaponInSlot(WeaponEquipPoint equipPoint)
    {
        switch (equipPoint)
        {
            case WeaponEquipPoint.LeftHand:
                if (mode != WeaponSetMode.OneHandedPair)
                    return null;

                return leftHandWeapon;

            case WeaponEquipPoint.RightHand:
                if (mode != WeaponSetMode.OneHandedPair)
                    return null;

                return rightHandWeapon;

            case WeaponEquipPoint.BothHands:
                if (mode != WeaponSetMode.TwoHandedWeapon)
                    return null;

                return twoHandedWeapon;

            case WeaponEquipPoint.Mouth:
                if (mode != WeaponSetMode.MouthWeapon)
                    return null;

                return mouthWeapon;

            default:
                return null;
        }
    }

    public ItemData RemoveWeaponInSlot(WeaponEquipPoint equipPoint)
    {
        ItemData removedWeapon = null;

        switch (equipPoint)
        {
            case WeaponEquipPoint.LeftHand:
                if (mode == WeaponSetMode.OneHandedPair)
                {
                    removedWeapon = leftHandWeapon;
                    leftHandWeapon = null;
                }
                break;

            case WeaponEquipPoint.RightHand:
                if (mode == WeaponSetMode.OneHandedPair)
                {
                    removedWeapon = rightHandWeapon;
                    rightHandWeapon = null;
                }
                break;

            case WeaponEquipPoint.BothHands:
                if (mode == WeaponSetMode.TwoHandedWeapon)
                {
                    removedWeapon = twoHandedWeapon;
                    twoHandedWeapon = null;
                }
                break;

            case WeaponEquipPoint.Mouth:
                if (mode == WeaponSetMode.MouthWeapon)
                {
                    removedWeapon = mouthWeapon;
                    mouthWeapon = null;
                }
                break;
        }

        CleanEmptyMode();

        return removedWeapon;
    }

    public void Clear()
    {
        mode = WeaponSetMode.Empty;
        ClearAllContents();
    }

    public void Validate()
    {
        if (mode == WeaponSetMode.Empty)
            AutoChooseModeFromAssignedFields();

        switch (mode)
        {
            case WeaponSetMode.Empty:
                ClearAllContents();
                break;

            case WeaponSetMode.OneHandedPair:
                ClearSingleWeaponModes();

                if (!IsValidOneHandedHandWeapon(leftHandWeapon))
                    leftHandWeapon = null;

                if (!IsValidOneHandedHandWeapon(rightHandWeapon))
                    rightHandWeapon = null;

                CleanEmptyMode();
                break;

            case WeaponSetMode.TwoHandedWeapon:
                leftHandWeapon = null;
                rightHandWeapon = null;
                mouthWeapon = null;
                saddleSourceEquipment = null;

                if (!IsValidTwoHandedHandWeapon(twoHandedWeapon))
                    twoHandedWeapon = null;

                CleanEmptyMode();
                break;

            case WeaponSetMode.MouthWeapon:
                leftHandWeapon = null;
                rightHandWeapon = null;
                twoHandedWeapon = null;
                saddleSourceEquipment = null;

                if (!IsValidMouthWeapon(mouthWeapon))
                    mouthWeapon = null;

                CleanEmptyMode();
                break;

            case WeaponSetMode.ManualSaddleTurret:
                leftHandWeapon = null;
                rightHandWeapon = null;
                twoHandedWeapon = null;
                mouthWeapon = null;

                if (saddleSourceEquipment == null ||
                    saddleSourceEquipment.itemCategory != ItemCategory.Equipment ||
                    !saddleSourceEquipment.hasManualSaddleTurret)
                {
                    saddleSourceEquipment = null;
                }

                CleanEmptyMode();
                break;
        }
    }

    private void AutoChooseModeFromAssignedFields()
    {
        if (saddleSourceEquipment != null &&
            saddleSourceEquipment.itemCategory == ItemCategory.Equipment &&
            saddleSourceEquipment.hasManualSaddleTurret)
        {
            mode = WeaponSetMode.ManualSaddleTurret;
            return;
        }

        if (mouthWeapon != null)
        {
            mode = WeaponSetMode.MouthWeapon;
            return;
        }

        if (twoHandedWeapon != null)
        {
            mode = WeaponSetMode.TwoHandedWeapon;
            return;
        }

        if (leftHandWeapon != null ||
            rightHandWeapon != null)
        {
            mode = WeaponSetMode.OneHandedPair;
        }
    }

    private void CleanEmptyMode()
    {
        if (HasAnyWeaponOrReservation)
            return;

        mode = WeaponSetMode.Empty;
    }

    private void ClearSingleWeaponModes()
    {
        twoHandedWeapon = null;
        mouthWeapon = null;
        saddleSourceEquipment = null;
    }

    private void ClearAllContents()
    {
        leftHandWeapon = null;
        rightHandWeapon = null;
        twoHandedWeapon = null;
        mouthWeapon = null;
        saddleSourceEquipment = null;
    }

    private bool IsWeapon(ItemData item)
    {
        return item != null &&
               item.itemCategory == ItemCategory.Weapon;
    }

    private bool IsValidOneHandedHandWeapon(ItemData item)
    {
        return IsWeapon(item) &&
               item.weaponUseType == WeaponUseType.HandWeapon &&
               item.handUsage == ItemHandUsage.OneHanded;
    }

    private bool IsValidTwoHandedHandWeapon(ItemData item)
    {
        return IsWeapon(item) &&
               item.weaponUseType == WeaponUseType.HandWeapon &&
               item.handUsage == ItemHandUsage.TwoHanded;
    }

    private bool IsValidMouthWeapon(ItemData item)
    {
        return IsWeapon(item) &&
               item.weaponUseType == WeaponUseType.MouthWeapon;
    }
}

public class PlayerWeaponSlots : MonoBehaviour
{
    [Header("Weapon Sets")]
    [SerializeField] private WeaponSet weaponSet1 = new WeaponSet();
    [SerializeField] private WeaponSet weaponSet2 = new WeaponSet();

    [Header("Active Weapon Set")]
    [Tooltip("0 = Weapon Set 1, 1 = Weapon Set 2")]
    [SerializeField] private int activeWeaponSetIndex = 0;

    [Header("Draw State")]
    [SerializeField] private bool weaponsDrawn = false;

    [Header("Race Rules")]
    [SerializeField] private bool canUseMouthWeapons = false;

    public WeaponSet WeaponSet1 => weaponSet1;
    public WeaponSet WeaponSet2 => weaponSet2;

    public int ActiveWeaponSetIndex => activeWeaponSetIndex;
    public bool WeaponsDrawn => weaponsDrawn;
    public bool CanUseMouthWeapons => canUseMouthWeapons;

    public WeaponSet ActiveWeaponSet =>
        activeWeaponSetIndex == 1
            ? weaponSet2
            : weaponSet1;

    public event Action OnWeaponSlotsChanged;

    private void OnValidate()
    {
        weaponSet1?.Validate();
        weaponSet2?.Validate();

        activeWeaponSetIndex =
            Mathf.Clamp(activeWeaponSetIndex, 0, 1);

        if (ActiveWeaponSet == null ||
            !ActiveWeaponSet.HasAnyWeaponOrReservation)
        {
            weaponsDrawn = false;
        }
    }

    public void ApplyRaceProfile(RaceProfile raceProfile)
    {
        if (raceProfile == null)
            return;

        canUseMouthWeapons =
            raceProfile.canUseMouthWeapons;

        OnWeaponSlotsChanged?.Invoke();
    }

    public bool TryEquipWeaponToActiveSet(
        ItemData weapon,
        WeaponEquipPoint equipPoint)
    {
        return TryEquipWeaponToSet(
            activeWeaponSetIndex,
            weapon,
            equipPoint
        );
    }

    public bool TryEquipWeaponToSet(
        int setIndex,
        ItemData weapon,
        WeaponEquipPoint equipPoint)
    {
        WeaponSet set =
            GetWeaponSet(setIndex);

        if (set == null)
            return false;

        bool equipped =
            set.TryEquipWeapon(
                weapon,
                equipPoint,
                canUseMouthWeapons
            );

        if (!equipped)
            return false;

        OnWeaponSlotsChanged?.Invoke();

        return true;
    }

    public ItemData GetWeaponInSetSlot(
        int setIndex,
        WeaponEquipPoint equipPoint)
    {
        WeaponSet set =
            GetWeaponSet(setIndex);

        if (set == null)
            return null;

        return set.GetWeaponInSlot(equipPoint);
    }

    public ItemData RemoveWeaponFromSetSlot(
        int setIndex,
        WeaponEquipPoint equipPoint)
    {
        WeaponSet set =
            GetWeaponSet(setIndex);

        if (set == null)
            return null;

        ItemData removedWeapon =
            set.RemoveWeaponInSlot(equipPoint);

        if (removedWeapon == null)
            return null;

        if (ActiveWeaponSet == null ||
            !ActiveWeaponSet.HasAnyWeaponOrReservation)
        {
            weaponsDrawn = false;
        }

        OnWeaponSlotsChanged?.Invoke();

        return removedWeapon;
    }

    public bool TryReserveSetForManualSaddleTurret(
        int setIndex,
        ItemData saddleEquipment)
    {
        WeaponSet set =
            GetWeaponSet(setIndex);

        if (set == null)
            return false;

        bool reserved =
            set.TryReserveForManualSaddleTurret(
                saddleEquipment
            );

        if (!reserved)
            return false;

        OnWeaponSlotsChanged?.Invoke();

        return true;
    }

    public void ClearManualSaddleTurretReservation(
        ItemData saddleEquipment)
    {
        bool changed = false;

        if (weaponSet1.ClearManualSaddleTurretReservation(saddleEquipment))
            changed = true;

        if (weaponSet2.ClearManualSaddleTurretReservation(saddleEquipment))
            changed = true;

        if (!changed)
            return;

        if (ActiveWeaponSet == null ||
            !ActiveWeaponSet.HasAnyWeaponOrReservation)
        {
            weaponsDrawn = false;
        }

        OnWeaponSlotsChanged?.Invoke();
    }

    public void SetActiveWeaponSet(int setIndex)
    {
        setIndex =
            Mathf.Clamp(setIndex, 0, 1);

        if (activeWeaponSetIndex == setIndex)
            return;

        activeWeaponSetIndex = setIndex;
        weaponsDrawn = false;

        OnWeaponSlotsChanged?.Invoke();
    }

    public bool DrawWeapons()
    {
        if (ActiveWeaponSet == null ||
            !ActiveWeaponSet.HasAnyWeaponOrReservation)
        {
            return false;
        }

        if (weaponsDrawn)
            return true;

        weaponsDrawn = true;

        OnWeaponSlotsChanged?.Invoke();

        return true;
    }

    public bool SheatheWeapons()
    {
        if (!weaponsDrawn)
            return false;

        weaponsDrawn = false;

        OnWeaponSlotsChanged?.Invoke();

        return true;
    }

    public bool ToggleWeaponsDrawn()
    {
        if (weaponsDrawn)
            return SheatheWeapons();

        return DrawWeapons();
    }

    public WeaponSet GetWeaponSet(int setIndex)
    {
        return setIndex == 1
            ? weaponSet2
            : weaponSet1;
    }

    public ItemData GetDrawnLeftHandWeapon()
    {
        if (!weaponsDrawn ||
            ActiveWeaponSet == null ||
            ActiveWeaponSet.Mode != WeaponSetMode.OneHandedPair)
        {
            return null;
        }

        return ActiveWeaponSet.LeftHandWeapon;
    }

    public ItemData GetDrawnRightHandWeapon()
    {
        if (!weaponsDrawn ||
            ActiveWeaponSet == null ||
            ActiveWeaponSet.Mode != WeaponSetMode.OneHandedPair)
        {
            return null;
        }

        return ActiveWeaponSet.RightHandWeapon;
    }

    public ItemData GetDrawnTwoHandedWeapon()
    {
        if (!weaponsDrawn ||
            ActiveWeaponSet == null ||
            ActiveWeaponSet.Mode != WeaponSetMode.TwoHandedWeapon)
        {
            return null;
        }

        return ActiveWeaponSet.TwoHandedWeapon;
    }

    public ItemData GetDrawnMouthWeapon()
    {
        if (!weaponsDrawn ||
            ActiveWeaponSet == null ||
            ActiveWeaponSet.Mode != WeaponSetMode.MouthWeapon)
        {
            return null;
        }

        return ActiveWeaponSet.MouthWeapon;
    }

    public ItemData GetDrawnManualSaddleTurretSource()
    {
        if (!weaponsDrawn ||
            ActiveWeaponSet == null ||
            ActiveWeaponSet.Mode != WeaponSetMode.ManualSaddleTurret)
        {
            return null;
        }

        return ActiveWeaponSet.SaddleSourceEquipment;
    }
}