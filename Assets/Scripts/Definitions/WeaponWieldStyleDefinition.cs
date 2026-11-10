using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWieldStyle",
    menuName = "Game/Combat/Weapon Wield Style")]
public sealed class WeaponWieldStyleDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName;

    [TextArea]
    public string description;
}