using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    [Header("Equipment")]
    [SerializeField] private ItemData equippedSaddle;

    [Header("Saddle Turret Settings")]
    [Tooltip("0 = Weapon Set 1, 1 = Weapon Set 2")]
    [SerializeField] private int manualSaddleTurretWeaponSetIndex = 1;

    public ItemData EquippedSaddle => equippedSaddle;

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();
    }

    private void Start()
    {
        ApplySaddleToWeaponSlots();
    }

    private void OnValidate()
    {
        manualSaddleTurretWeaponSetIndex =
            Mathf.Clamp(manualSaddleTurretWeaponSetIndex, 0, 1);
    }

    public bool TryEquipSaddle(ItemData saddleItem)
    {
        if (saddleItem == null)
            return false;

        if (saddleItem.itemCategory != ItemCategory.Equipment)
            return false;

        if (equippedSaddle != null)
            UnequipSaddle();

        equippedSaddle = saddleItem;

        ApplySaddleToWeaponSlots();

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

    public void ApplySaddleToWeaponSlots()
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
}