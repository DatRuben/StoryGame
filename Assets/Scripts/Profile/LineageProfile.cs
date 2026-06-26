using UnityEngine;

[CreateAssetMenu(menuName = "Game/Lineage Profile")]
public class LineageProfile : ScriptableObject
{
    [Header("Identity")]
    public string LineageName;

    [Header("Attribute Bonuses")]
    public CharacterAttributeModifiers attributeModifiers =
    CharacterAttributeModifiers.CreateZero();

    [TextArea]
    public string description;

    [Header("Applies To")]
    public BaseRace baseRace;

    [Header("Visual Weight")]
    //[Range(0f, 1f)]
    //public float defaultInfluence = 1f;

    [Header("Skill Tree")]
    public string skillTheme;

    [TextArea]
    public string skillTreeTheme;
}