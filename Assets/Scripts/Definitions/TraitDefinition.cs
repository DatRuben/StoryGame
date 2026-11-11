using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Trait Definition")]
public class TraitDefinition : GameDefinition
{
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
}