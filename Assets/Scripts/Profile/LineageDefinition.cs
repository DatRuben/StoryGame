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

    [Header("Attribute Shape Modifiers")]
    public CharacterAttributeModifiers modifiers =
        CharacterAttributeModifiers.CreateZero();

    [Header("Lineage Attribute Shape")]
    [SerializeField]
    private CharacterAttributes attributeShapePreview =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int totalAttributeShapePointsPreview;

    public CharacterAttributes AttributeShapePreview =>
        attributeShapePreview;

    public int TotalAttributeShapePointsPreview =>
        totalAttributeShapePointsPreview;

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

    public void RecalculatePreview()
    {
        attributeShapePreview =
            CharacterAttributes.AddModifiers(
                CharacterAttributes.CreateDefault(10),
                modifiers
            );

        totalAttributeShapePointsPreview =
            attributeShapePreview.BasePoints();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        lineageId = MakeId(displayName);

        RecalculatePreview();
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