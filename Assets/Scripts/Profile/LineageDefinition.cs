using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum LineageType
{
    AnimalSpecies,
    HybridAncestry
}

[CreateAssetMenu(menuName = "Game/Lineage Definition")]
public class LineageDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector] public string lineageId;
    public string displayName;
    public LineageType lineageType;

    [TextArea]
    public string description;

    [Header("Allowed Main Races")]
    public List<RaceDefinition> allowedRaces = new();

    [Header("Attribute Modifiers")]
    public CharacterAttributeModifiers modifiers =
        CharacterAttributeModifiers.CreateZero();

    [Header("Calculated Preview")]
    [SerializeField]
    private CharacterAttributes finalAttributesPreview =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int totalAttributePointsPreview;

    public CharacterAttributes FinalAttributesPreview =>
        finalAttributesPreview;

    public int TotalAttributePointsPreview =>
        totalAttributePointsPreview;

    [Header("Skill / Theme")]
    public string skillTheme;

    [TextArea]
    public string skillTreeTheme;

    public bool IsAllowedForRace(RaceDefinition race)
    {
        if (race == null)
            return false;

        if (allowedRaces == null ||
            allowedRaces.Count == 0)
        {
            return false;
        }

        return allowedRaces.Contains(race);
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        lineageId = MakeId(displayName);

        finalAttributesPreview =
            CharacterAttributes.AddModifiers(
                CharacterAttributes.CreateDefault(10),
                modifiers
            );

        totalAttributePointsPreview =
            finalAttributesPreview.BasePoints();
    }

    private string MakeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        StringBuilder builder = new();

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length > 0 &&
                     builder[^1] != '_')
            {
                builder.Append('_');
            }
        }

        return builder.ToString().Trim('_');
    }
}