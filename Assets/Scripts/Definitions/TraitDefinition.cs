using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Trait Definition")]
public class TraitDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector] public string traitId;
    public string displayName;

    [TextArea]
    public string description;

    [Header("Attribute Trade Modifiers")]
    public CharacterAttributeModifiers modifiers =
        CharacterAttributeModifiers.CreateZero();

    [Header("Rules")]
    public List<TraitDefinition> mutuallyExclusiveTraits = new();

    public bool IsMutuallyExclusiveWith(
        TraitDefinition otherTrait)
    {
        if (otherTrait == null)
            return false;

        return mutuallyExclusiveTraits != null &&
               mutuallyExclusiveTraits.Contains(otherTrait);
    }

    public bool HasNeutralModifierTotal()
    {
        return modifiers == null ||
               modifiers.Total() == 0;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        traitId = MakeId(displayName);
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