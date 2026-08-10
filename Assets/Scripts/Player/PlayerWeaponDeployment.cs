using System;
using UnityEngine;

[RequireComponent(
    typeof(PlayerWeaponLoadout)
)]
public sealed class PlayerWeaponDeployment :
    MonoBehaviour
{
    [SerializeField]
    private bool weaponsDrawn;

    public bool WeaponsDrawn =>
        weaponsDrawn;

    public event Action Changed;

    public void DrawWeapons()
    {
        SetWeaponsDrawn(true);
    }

    public void SheatheWeapons()
    {
        SetWeaponsDrawn(false);
    }

    public void ToggleWeaponsDrawn()
    {
        SetWeaponsDrawn(
            !weaponsDrawn
        );
    }

    public void SetWeaponsDrawn(
        bool drawn)
    {
        if (weaponsDrawn ==
            drawn)
        {
            return;
        }

        weaponsDrawn =
            drawn;

        Changed?.Invoke();
    }
}