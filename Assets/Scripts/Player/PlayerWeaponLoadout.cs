using System;
using UnityEngine;

public sealed class WeaponSet
{
    public const int SlotCount = 2;

    private readonly InventoryItemInstance[] weapons =
        new InventoryItemInstance[SlotCount];

    private CharacterForm requiredForm =
        CharacterForm.Standing;

    public CharacterForm RequiredForm =>
        requiredForm;

    public bool HasAnyWeapon =>
        weapons[0] != null ||
        weapons[1] != null;

    public int WeaponCount
    {
        get
        {
            int count = 0;

            for (int i = 0;
                 i < weapons.Length;
                 i++)
            {
                if (weapons[i] != null)
                    count++;
            }

            return count;
        }
    }

    public InventoryItemInstance GetWeapon(
        int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
            return null;

        return weapons[slotIndex];
    }

    internal void SetRequiredForm(
        CharacterForm form)
    {
        requiredForm = form;
    }

    internal bool SetWeapon(
        int slotIndex,
        InventoryItemInstance weapon)
    {
        if (!IsValidSlot(slotIndex) ||
            weapon == null ||
            weapons[slotIndex] != null)
        {
            return false;
        }

        weapons[slotIndex] =
            weapon;

        return true;
    }

    internal InventoryItemInstance RemoveWeapon(
        int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
            return null;

        InventoryItemInstance removedWeapon =
            weapons[slotIndex];

        weapons[slotIndex] = null;

        return removedWeapon;
    }

    private static bool IsValidSlot(
        int slotIndex)
    {
        return slotIndex >= 0 &&
               slotIndex < SlotCount;
    }
}

public sealed class PlayerWeaponLoadout :
    MonoBehaviour
{
    public const int WeaponSetCount = 2;

    private readonly WeaponSet[] weaponSets =
    {
        new WeaponSet(),
        new WeaponSet()
    };

    [Header("Active Weapon Set")]
    [SerializeField]
    [Range(0, WeaponSetCount - 1)]
    private int activeWeaponSetIndex;

    public int ActiveWeaponSetIndex =>
        activeWeaponSetIndex;

    public WeaponSet ActiveWeaponSet =>
        weaponSets[activeWeaponSetIndex];

    public CharacterForm ActiveRequiredForm =>
        ActiveWeaponSet.RequiredForm;

    public event Action Changed;

    private void OnValidate()
    {
        activeWeaponSetIndex =
            Mathf.Clamp(
                activeWeaponSetIndex,
                0,
                WeaponSetCount - 1
            );
    }

    internal void ConfigureForms(
        BodyType bodyType)
    {
        switch (bodyType)
        {
            case BodyType.Quadruped:
                for (int i = 0;
                     i < WeaponSetCount;
                     i++)
                {
                    weaponSets[i]
                        .SetRequiredForm(
                            CharacterForm.Feral
                        );
                }
                break;

            case BodyType.StanceSwitching:
                weaponSets[0]
                    .SetRequiredForm(
                        CharacterForm.Standing
                    );

                weaponSets[1]
                    .SetRequiredForm(
                        CharacterForm.Feral
                    );
                break;

            case BodyType.Humanoid:
            default:
                for (int i = 0;
                     i < WeaponSetCount;
                     i++)
                {
                    weaponSets[i]
                        .SetRequiredForm(
                            CharacterForm.Standing
                        );
                }
                break;
        }

        Changed?.Invoke();
    }

    internal bool SetRequiredForm(
        int setIndex,
        CharacterForm form,
        BodyType bodyType)
    {
        if (!IsValidSet(setIndex))
            return false;

        switch (bodyType)
        {
            case BodyType.Humanoid:
                if (form !=
                    CharacterForm.Standing)
                {
                    return false;
                }
                break;

            case BodyType.Quadruped:
                if (form !=
                    CharacterForm.Feral)
                {
                    return false;
                }
                break;

            case BodyType.StanceSwitching:
                break;

            default:
                return false;
        }

        WeaponSet set =
            weaponSets[setIndex];

        if (set.RequiredForm == form)
            return true;

        set.SetRequiredForm(
            form
        );

        Changed?.Invoke();

        return true;
    }

    public WeaponSet GetWeaponSet(
        int setIndex)
    {
        if (!IsValidSet(setIndex))
            return null;

        return weaponSets[setIndex];
    }

    public CharacterForm GetRequiredForm(
        int setIndex)
    {
        WeaponSet set =
            GetWeaponSet(setIndex);

        return set != null
            ? set.RequiredForm
            : CharacterForm.Standing;
    }

    public InventoryItemInstance GetWeapon(
        int setIndex,
        int slotIndex)
    {
        WeaponSet set =
            GetWeaponSet(setIndex);

        if (set == null)
            return null;

        return set.GetWeapon(
            slotIndex
        );
    }

    public InventoryItemInstance GetActiveWeapon(
        int slotIndex)
    {
        return ActiveWeaponSet.GetWeapon(
            slotIndex
        );
    }

    public bool IsWeaponAssigned(
        InventoryItemInstance weapon)
    {
        return TryFindWeapon(
            weapon,
            out _,
            out _
        );
    }

    public bool TryFindWeapon(
        InventoryItemInstance weapon,
        out int setIndex,
        out int slotIndex)
    {
        setIndex = -1;
        slotIndex = -1;

        if (weapon == null)
            return false;

        for (int currentSetIndex = 0;
             currentSetIndex < WeaponSetCount;
             currentSetIndex++)
        {
            WeaponSet set =
                weaponSets[currentSetIndex];

            for (int currentSlotIndex = 0;
                 currentSlotIndex <
                    WeaponSet.SlotCount;
                 currentSlotIndex++)
            {
                if (!ReferenceEquals(
                    set.GetWeapon(
                        currentSlotIndex
                    ),
                    weapon))
                {
                    continue;
                }

                setIndex =
                    currentSetIndex;

                slotIndex =
                    currentSlotIndex;

                return true;
            }
        }

        return false;
    }

    internal bool TryAssignWeapon(
        int setIndex,
        int slotIndex,
        InventoryItemInstance weapon)
    {
        if (!CanAssignWeapon(
            setIndex,
            slotIndex,
            weapon))
        {
            return false;
        }

        WeaponSet set =
            weaponSets[setIndex];

        if (!set.SetWeapon(
            slotIndex,
            weapon))
        {
            return false;
        }

        SubscribeWeapon(
            weapon
        );

        Changed?.Invoke();

        return true;
    }

    internal bool TryReplaceWeapon(
        int setIndex,
        int slotIndex,
        InventoryItemInstance weapon,
        out InventoryItemInstance replacedWeapon)
    {
        replacedWeapon = null;

        if (!IsValidSet(setIndex) ||
            !IsValidSlot(slotIndex) ||
            !IsLoadoutWeapon(weapon))
        {
            return false;
        }

        WeaponSet set =
            weaponSets[setIndex];

        InventoryItemInstance currentWeapon =
            set.GetWeapon(slotIndex);

        if (ReferenceEquals(
            currentWeapon,
            weapon))
        {
            return true;
        }

        if (IsWeaponAssigned(weapon))
            return false;

        if (currentWeapon != null)
        {
            replacedWeapon =
                set.RemoveWeapon(
                    slotIndex
                );

            UnsubscribeWeapon(
                replacedWeapon
            );
        }

        if (!set.SetWeapon(
            slotIndex,
            weapon))
        {
            if (replacedWeapon != null)
            {
                set.SetWeapon(
                    slotIndex,
                    replacedWeapon
                );

                SubscribeWeapon(
                    replacedWeapon
                );
            }

            replacedWeapon = null;

            return false;
        }

        SubscribeWeapon(
            weapon
        );

        Changed?.Invoke();

        return true;
    }

    internal InventoryItemInstance RemoveWeapon(
        int setIndex,
        int slotIndex)
    {
        if (!IsValidSet(setIndex) ||
            !IsValidSlot(slotIndex))
        {
            return null;
        }

        InventoryItemInstance removedWeapon =
            weaponSets[setIndex]
                .RemoveWeapon(
                    slotIndex
                );

        if (removedWeapon == null)
            return null;

        UnsubscribeWeapon(
            removedWeapon
        );

        Changed?.Invoke();

        return removedWeapon;
    }

    internal bool TryMoveWeapon(
        int sourceSetIndex,
        int sourceSlotIndex,
        int targetSetIndex,
        int targetSlotIndex)
    {
        if (!IsValidSet(sourceSetIndex) ||
            !IsValidSet(targetSetIndex) ||
            !IsValidSlot(sourceSlotIndex) ||
            !IsValidSlot(targetSlotIndex))
        {
            return false;
        }

        if (sourceSetIndex ==
                targetSetIndex &&
            sourceSlotIndex ==
                targetSlotIndex)
        {
            return true;
        }

        WeaponSet sourceSet =
            weaponSets[sourceSetIndex];

        WeaponSet targetSet =
            weaponSets[targetSetIndex];

        InventoryItemInstance weapon =
            sourceSet.GetWeapon(
                sourceSlotIndex
            );

        if (weapon == null ||
            targetSet.GetWeapon(
                targetSlotIndex
            ) != null)
        {
            return false;
        }

        sourceSet.RemoveWeapon(
            sourceSlotIndex
        );

        if (!targetSet.SetWeapon(
            targetSlotIndex,
            weapon))
        {
            sourceSet.SetWeapon(
                sourceSlotIndex,
                weapon
            );

            return false;
        }

        Changed?.Invoke();

        return true;
    }

    internal bool SetActiveWeaponSet(
        int setIndex)
    {
        if (!IsValidSet(setIndex))
            return false;

        if (activeWeaponSetIndex ==
            setIndex)
        {
            return true;
        }

        activeWeaponSetIndex =
            setIndex;

        Changed?.Invoke();

        return true;
    }

    private bool CanAssignWeapon(
        int setIndex,
        int slotIndex,
        InventoryItemInstance weapon)
    {
        if (!IsValidSet(setIndex) ||
            !IsValidSlot(slotIndex) ||
            !IsLoadoutWeapon(weapon))
        {
            return false;
        }

        if (weaponSets[setIndex]
            .GetWeapon(slotIndex) != null)
        {
            return false;
        }

        return !IsWeaponAssigned(
            weapon
        );
    }

    private static bool IsLoadoutWeapon(
        InventoryItemInstance weapon)
    {
        return weapon != null &&
               !weapon.IsEmpty &&
               weapon.Definition != null &&
               weapon.Definition.IsLoadoutWeapon;
    }

    private static bool IsValidSet(
        int setIndex)
    {
        return setIndex >= 0 &&
               setIndex < WeaponSetCount;
    }

    private static bool IsValidSlot(
        int slotIndex)
    {
        return slotIndex >= 0 &&
               slotIndex <
                   WeaponSet.SlotCount;
    }

    private void SubscribeWeapon(
        InventoryItemInstance weapon)
    {
        if (weapon == null)
            return;

        weapon.Changed -=
            OnAssignedWeaponChanged;

        weapon.Changed +=
            OnAssignedWeaponChanged;
    }

    private void UnsubscribeWeapon(
        InventoryItemInstance weapon)
    {
        if (weapon == null)
            return;

        weapon.Changed -=
            OnAssignedWeaponChanged;
    }

    private void OnAssignedWeaponChanged()
    {
        bool removedEmptyWeapon =
            RemoveEmptyWeapons();

        if (!removedEmptyWeapon)
            Changed?.Invoke();
    }

    private bool RemoveEmptyWeapons()
    {
        bool changed = false;

        for (int setIndex = 0;
             setIndex < WeaponSetCount;
             setIndex++)
        {
            WeaponSet set =
                weaponSets[setIndex];

            for (int slotIndex = 0;
                 slotIndex <
                    WeaponSet.SlotCount;
                 slotIndex++)
            {
                InventoryItemInstance weapon =
                    set.GetWeapon(
                        slotIndex
                    );

                if (weapon == null ||
                    !weapon.IsEmpty)
                {
                    continue;
                }

                set.RemoveWeapon(
                    slotIndex
                );

                UnsubscribeWeapon(
                    weapon
                );

                changed = true;
            }
        }

        if (changed)
            Changed?.Invoke();

        return changed;
    }

    private void OnDestroy()
    {
        for (int setIndex = 0;
             setIndex < WeaponSetCount;
             setIndex++)
        {
            WeaponSet set =
                weaponSets[setIndex];

            for (int slotIndex = 0;
                 slotIndex <
                    WeaponSet.SlotCount;
                 slotIndex++)
            {
                UnsubscribeWeapon(
                    set.GetWeapon(
                        slotIndex
                    )
                );
            }
        }
    }
}