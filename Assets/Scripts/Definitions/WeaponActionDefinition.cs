using UnityEngine;

public enum WeaponActionType
{
    None,
    Projectile
}

[CreateAssetMenu(
    menuName = "Game/Combat/Weapon Action Definition"
)]
public sealed class WeaponActionDefinition :
    ScriptableObject
{
    [Header("Identity")]
    public string actionName;

    [Header("Action")]
    public WeaponActionType actionType =
        WeaponActionType.None;

    [Min(0f)]
    public float cooldown;

    [Header("Damage")]
    [Min(0f)]
    public float baseDamage = 10f;

    public DamageType damageType =
        DamageType.Aether;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Min(0.1f)]
    public float projectileSpeed = 10f;

    [Min(0.1f)]
    public float projectileLifetime = 5f;
}