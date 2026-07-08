using System.Collections.Generic;

public static class CharacterAttributeResolver
{
    public static CharacterAttributePreview CreatePreview(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageDefinition> lineages)
    {
        CharacterAttributes ancestryAttributes =
            CalculateAncestryAttributes(
                raceDefinition,
                subraceDefinition,
                lineages
            );

        CharacterAttributeModifiers backgroundModifiers =
            CharacterAttributeModifiers.CreateZero();

        CharacterAttributeModifiers traitModifiers =
            CharacterAttributeModifiers.CreateZero();

        CharacterAttributeModifiers racialPassiveModifiers =
            CharacterAttributeModifiers.CreateZero();

        return CharacterAttributePreview.Create(
            ancestryAttributes,
            backgroundModifiers,
            traitModifiers,
            racialPassiveModifiers
        );
    }

    private static CharacterAttributes CalculateAncestryAttributes(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageDefinition> lineages)
    {
        if (subraceDefinition != null &&
            subraceDefinition.FinalAttributesPreview != null)
        {
            return CharacterAttributes.Copy(
                subraceDefinition.FinalAttributesPreview
            );
        }

        if (raceDefinition != null &&
            raceDefinition.FinalAttributesPreview != null)
        {
            return CharacterAttributes.Copy(
                raceDefinition.FinalAttributesPreview
            );
        }

        return CharacterAttributes.CreateDefault(10);
    }
}