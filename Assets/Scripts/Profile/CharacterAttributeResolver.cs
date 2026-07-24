using System.Collections.Generic;

public static class CharacterAttributeResolver
{
    private const int AncestryAttributeTotal = 90;

    public static CharacterAttributePreview CreatePreview(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageSelection> lineages)
    {
        return CreatePreview(
            raceDefinition,
            subraceDefinition,
            lineages,
            null,
            null
        );
    }

    public static CharacterAttributePreview CreatePreview(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageSelection> lineages,
        BackgroundDefinition backgroundDefinition,
        List<TraitDefinition> traitDefinitions)
    {
        List<LineageSelection> validLineages =
            GetValidLineages(
                lineages
            );

        CharacterAttributes ancestryAttributes =
            CalculateAncestryAttributes(
                raceDefinition,
                subraceDefinition,
                validLineages
            );

        CharacterAttributes baseAncestryAttributes =
            GetBaseAncestryTarget(
                raceDefinition,
                subraceDefinition
            );

        List<LineageInfluencePreview> lineageInfluences =
            BuildLineageInfluences(
                raceDefinition,
                baseAncestryAttributes,
                ancestryAttributes,
                validLineages,
                out float mainAncestryInfluence
            );

        CharacterAttributeModifiers backgroundModifiers =
            GetBackgroundModifiers(
                backgroundDefinition
            );

        CharacterAttributeModifiers traitModifiers =
            GetTraitModifiers(
                traitDefinitions
            );

        CharacterAttributeModifiers racialPassiveModifiers =
            CharacterAttributeModifiers.CreateZero();

        return CharacterAttributePreview.Create(
            ancestryAttributes,
            backgroundModifiers,
            traitModifiers,
            racialPassiveModifiers,
            mainAncestryInfluence,
            lineageInfluences
        );
    }

    private static CharacterAttributeModifiers GetBackgroundModifiers(
        BackgroundDefinition backgroundDefinition)
    {
        if (backgroundDefinition == null)
        {
            return CharacterAttributeModifiers.CreateZero();
        }

        return CharacterAttributeModifiers.Copy(
            backgroundDefinition.modifiers
        );
    }

    private static CharacterAttributeModifiers GetTraitModifiers(
        List<TraitDefinition> traitDefinitions)
    {
        CharacterAttributeModifiers totalModifiers =
            CharacterAttributeModifiers.CreateZero();

        if (traitDefinitions == null)
            return totalModifiers;

        foreach (TraitDefinition traitDefinition
                 in traitDefinitions)
        {
            if (traitDefinition == null)
                continue;

            totalModifiers =
                CharacterAttributeModifiers.Add(
                    totalModifiers,
                    traitDefinition.modifiers
                );
        }

        return totalModifiers;
    }

    private static CharacterAttributes CalculateAncestryAttributes(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageSelection> lineages)
    {
        List<LineageSelection> validLineages =
            GetValidLineages(lineages);

        CharacterAttributes result;

        if (raceDefinition != null &&
            raceDefinition.allowedLineageType ==
            LineageType.AnimalSpecies)
        {
            result =
                ResolveAnimalSpecies(
                    raceDefinition,
                    subraceDefinition,
                    validLineages
                );
        }
        else
        {
            result =
                ResolveHybridAncestry(
                    raceDefinition,
                    subraceDefinition,
                    validLineages
                );
        }

        return CharacterAttributes.ClampMinimum(
            result
        );
    }

    private static CharacterAttributes ResolveHybridAncestry(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageSelection> lineages)
    {
        CharacterAttributes mainShape =
            GetBaseAncestryTarget(
                raceDefinition,
                subraceDefinition
            );

        List<CharacterAttributes> lineageShapes =
            GetHybridLineageShapes(lineages);

        if (lineageShapes.Count == 0)
        {
            return mainShape;
        }

        List<CharacterAttributes> targets = new()
    {
        mainShape
    };

        List<int> weights = new()
    {
        lineageShapes.Count == 1
            ? 3
            : 2
    };

        foreach (CharacterAttributes lineageShape
                 in lineageShapes)
        {
            targets.Add(lineageShape);
            weights.Add(1);
        }

        return BlendWeightedTargets(
            targets,
            weights,
            AncestryAttributeTotal
        );
    }

    private static List<CharacterAttributes>
        GetHybridLineageShapes(
            List<LineageSelection> lineages)
    {
        List<CharacterAttributes> shapes = new();

        if (lineages == null)
            return shapes;

        foreach (LineageSelection lineage
                 in lineages)
        {
            CharacterAttributes shape = null;

            if (lineage?.Subrace != null)
            {
                shape =
                    lineage
                        .Subrace
                        .FinalAttributesPreview;
            }
            else if (lineage?.CustomLineage != null)
            {
                shape =
                    lineage
                        .CustomLineage
                        .hybridAttributeShape;
            }

            if (shape == null)
                continue;

            shapes.Add(
                CharacterAttributes.Copy(shape)
            );
        }

        return shapes;
    }

    private static CharacterAttributes ResolveAnimalSpecies(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageSelection> lineages)
    {
        CharacterAttributes baseShape =
            GetBaseAncestryTarget(
                raceDefinition,
                subraceDefinition
            );

        List<CharacterAttributes> speciesTargets =
            new();

        List<int> weights = new();

        if (lineages != null)
        {
            foreach (LineageSelection lineage
                     in lineages)
            {
                LineageDefinition species =
                    lineage?.CustomLineage;

                if (species == null)
                    continue;

                CharacterAttributes speciesTarget =
                    CharacterAttributes.AddModifiers(
                        baseShape,
                        species.animalSpeciesModifiers
                    );

                speciesTargets.Add(
                    speciesTarget
                );

                weights.Add(1);
            }
        }

        if (speciesTargets.Count == 0)
        {
            return baseShape;
        }

        return BlendWeightedTargets(
            speciesTargets,
            weights,
            AncestryAttributeTotal
        );
    }

    private static List<LineageSelection> GetValidLineages(
        List<LineageSelection> lineages)
    {
        List<LineageSelection> validLineages = new();

        if (lineages == null)
            return validLineages;

        foreach (LineageSelection lineage in lineages)
        {
            if (lineage != null &&
                lineage.IsValid)
            {
                validLineages.Add(lineage);
            }
        }

        return validLineages;
    }

    private static List<LineageInfluencePreview>
    BuildLineageInfluences(
        RaceDefinition raceDefinition,
        CharacterAttributes baseAttributes,
        CharacterAttributes finalAttributes,
        List<LineageSelection> lineages,
        out float mainInfluence)
    {
        mainInfluence = 1f;

        List<LineageInfluencePreview> previews =
            new();

        List<CharacterAttributeModifiers> sourceModifiers =
            new();

        if (raceDefinition == null ||
            baseAttributes == null ||
            finalAttributes == null ||
            lineages == null)
        {
            return previews;
        }

        bool usesAnimalSpecies =
            raceDefinition.allowedLineageType ==
            LineageType.AnimalSpecies;

        foreach (LineageSelection lineage in lineages)
        {
            if (lineage == null ||
                !lineage.IsValid)
            {
                continue;
            }

            CharacterAttributeModifiers source =
                GetLineageSourceModifiers(
                    lineage,
                    baseAttributes,
                    usesAnimalSpecies
                );

            if (source == null)
                continue;

            previews.Add(
                LineageInfluencePreview.Create(
                    lineage,
                    0f
                )
            );

            sourceModifiers.Add(source);
        }

        int lineageCount =
            previews.Count;

        if (lineageCount == 0)
            return previews;

        int denominator;
        float lineageInfluence;

        if (usesAnimalSpecies)
        {
            denominator =
                lineageCount;

            lineageInfluence =
                1f / lineageCount;

            mainInfluence = 1f;
        }
        else
        {
            denominator = 4;
            lineageInfluence = 0.25f;

            mainInfluence =
                lineageCount == 1
                    ? 0.75f
                    : 0.5f;
        }

        foreach (LineageInfluencePreview preview
                 in previews)
        {
            preview.influence =
                lineageInfluence;
        }

        for (int attributeIndex = 0;
             attributeIndex < 9;
             attributeIndex++)
        {
            AssignExactLineageContributions(
                attributeIndex,
                baseAttributes,
                finalAttributes,
                sourceModifiers,
                previews,
                denominator
            );
        }

        return previews;
    }

    private static CharacterAttributeModifiers
        GetLineageSourceModifiers(
            LineageSelection lineage,
            CharacterAttributes baseAttributes,
            bool usesAnimalSpecies)
    {
        if (lineage == null ||
            baseAttributes == null)
        {
            return null;
        }

        if (usesAnimalSpecies)
        {
            if (lineage.CustomLineage == null ||
                lineage.CustomLineage
                    .animalSpeciesModifiers == null)
            {
                return null;
            }

            return CharacterAttributeModifiers.Copy(
                lineage.CustomLineage
                    .animalSpeciesModifiers
            );
        }

        CharacterAttributes lineageShape =
            GetHybridLineageShape(
                lineage
            );

        if (lineageShape == null)
            return null;

        return CharacterAttributeModifiers.FromDifference(
            lineageShape,
            baseAttributes
        );
    }

    private static CharacterAttributes GetHybridLineageShape(
        LineageSelection lineage)
    {
        if (lineage?.Subrace != null)
        {
            return lineage
                .Subrace
                .FinalAttributesPreview;
        }

        if (lineage?.CustomLineage != null)
        {
            return lineage
                .CustomLineage
                .hybridAttributeShape;
        }

        return null;
    }

    private static void AssignExactLineageContributions(
        int attributeIndex,
        CharacterAttributes baseAttributes,
        CharacterAttributes finalAttributes,
        List<CharacterAttributeModifiers> sourceModifiers,
        List<LineageInfluencePreview> previews,
        int denominator)
    {
        if (sourceModifiers == null ||
            previews == null ||
            denominator <= 0)
        {
            return;
        }

        int count =
            sourceModifiers.Count < previews.Count
                ? sourceModifiers.Count
                : previews.Count;

        if (count <= 0)
            return;

        List<LineageContributionRemainder> remainders =
            new();

        int assignedTotal = 0;

        for (int i = 0; i < count; i++)
        {
            int numerator =
                GetModifierValue(
                    sourceModifiers[i],
                    attributeIndex
                );

            int contribution =
                numerator / denominator;

            assignedTotal +=
                contribution;

            AddToModifier(
                previews[i].effectiveModifiers,
                attributeIndex,
                contribution
            );

            remainders.Add(
                new LineageContributionRemainder(
                    i,
                    numerator % denominator
                )
            );
        }

        int targetDifference =
            GetAttributeValue(
                finalAttributes,
                attributeIndex
            ) -
            GetAttributeValue(
                baseAttributes,
                attributeIndex
            );

        int remaining =
            targetDifference -
            assignedTotal;

        if (remaining == 0)
            return;

        if (remaining > 0)
        {
            remainders.Sort(
                (first, second) =>
                    second.remainder.CompareTo(
                        first.remainder
                    )
            );
        }
        else
        {
            remainders.Sort(
                (first, second) =>
                    first.remainder.CompareTo(
                        second.remainder
                    )
            );
        }

        int amount =
            remaining > 0
                ? 1
                : -1;

        int points =
            remaining > 0
                ? remaining
                : -remaining;

        for (int i = 0; i < points; i++)
        {
            int lineageIndex =
                remainders[
                    i % remainders.Count
                ].lineageIndex;

            AddToModifier(
                previews[lineageIndex]
                    .effectiveModifiers,
                attributeIndex,
                amount
            );
        }
    }

    private static CharacterAttributes GetBaseAncestryTarget(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition)
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

    private static CharacterAttributes BlendWeightedTargets(
        List<CharacterAttributes> targets,
        List<int> weights,
        int targetTotal)
    {
        if (targets == null ||
            weights == null ||
            targets.Count == 0 ||
            weights.Count == 0)
        {
            return CharacterAttributes.CreateDefault(10);
        }

        int totalWeight = 0;

        int strength = 0;
        int dexterity = 0;
        int agility = 0;
        int vitality = 0;
        int endurance = 0;
        int intelligence = 0;
        int willpower = 0;
        int spirit = 0;
        int perception = 0;

        int count =
            targets.Count < weights.Count
                ? targets.Count
                : weights.Count;

        for (int i = 0; i < count; i++)
        {
            CharacterAttributes target =
                targets[i];

            int weight =
                weights[i];

            if (target == null ||
                weight <= 0)
            {
                continue;
            }

            totalWeight += weight;

            strength +=
                target.strength * weight;

            dexterity +=
                target.dexterity * weight;

            agility +=
                target.agility * weight;

            vitality +=
                target.vitality * weight;

            endurance +=
                target.endurance * weight;

            intelligence +=
                target.intelligence * weight;

            willpower +=
                target.willpower * weight;

            spirit +=
                target.spirit * weight;

            perception +=
                target.perception * weight;
        }

        if (totalWeight <= 0)
        {
            return CharacterAttributes.CreateDefault(10);
        }

        CharacterAttributes result =
            new CharacterAttributes
            {
                strength =
                    strength / totalWeight,

                dexterity =
                    dexterity / totalWeight,

                agility =
                    agility / totalWeight,

                vitality =
                    vitality / totalWeight,

                endurance =
                    endurance / totalWeight,

                intelligence =
                    intelligence / totalWeight,

                willpower =
                    willpower / totalWeight,

                spirit =
                    spirit / totalWeight,

                perception =
                    perception / totalWeight
            };

        List<AttributeRemainder> remainders = new()
        {
            new AttributeRemainder(
                0,
                strength % totalWeight
            ),

            new AttributeRemainder(
                1,
                dexterity % totalWeight
            ),

            new AttributeRemainder(
                2,
                agility % totalWeight
            ),

            new AttributeRemainder(
                3,
                vitality % totalWeight
            ),

            new AttributeRemainder(
                4,
                endurance % totalWeight
            ),

            new AttributeRemainder(
                5,
                intelligence % totalWeight
            ),

            new AttributeRemainder(
                6,
                willpower % totalWeight
            ),

            new AttributeRemainder(
                7,
                spirit % totalWeight
            ),

            new AttributeRemainder(
                8,
                perception % totalWeight
            )
        };

        int pointDifference =
            targetTotal - result.BasePoints();

        if (pointDifference > 0)
        {
            AddMissingPoints(
                result,
                remainders,
                pointDifference
            );
        }
        else if (pointDifference < 0)
        {
            RemoveExtraPoints(
                result,
                remainders,
                -pointDifference
            );
        }

        return result;
    }

    private static void AddMissingPoints(
        CharacterAttributes attributes,
        List<AttributeRemainder> remainders,
        int pointsToAdd)
    {
        if (attributes == null ||
            remainders == null ||
            remainders.Count == 0)
        {
            return;
        }

        remainders.Sort(
            (first, second) =>
                second.remainder.CompareTo(
                    first.remainder
                )
        );

        for (int i = 0;
             i < pointsToAdd;
             i++)
        {
            AddToAttribute(
                attributes,
                remainders[
                    i % remainders.Count
                ].attributeIndex,
                1
            );
        }
    }

    private static void RemoveExtraPoints(
        CharacterAttributes attributes,
        List<AttributeRemainder> remainders,
        int pointsToRemove)
    {
        if (attributes == null ||
            remainders == null ||
            remainders.Count == 0)
        {
            return;
        }

        remainders.Sort(
            (first, second) =>
                first.remainder.CompareTo(
                    second.remainder
                )
        );

        for (int i = 0;
             i < pointsToRemove;
             i++)
        {
            AddToAttribute(
                attributes,
                remainders[
                    i % remainders.Count
                ].attributeIndex,
                -1
            );
        }
    }

    private static int GetAttributeValue(
    CharacterAttributes attributes,
    int attributeIndex)
    {
        if (attributes == null)
            return 0;

        switch (attributeIndex)
        {
            case 0:
                return attributes.strength;

            case 1:
                return attributes.dexterity;

            case 2:
                return attributes.agility;

            case 3:
                return attributes.vitality;

            case 4:
                return attributes.endurance;

            case 5:
                return attributes.intelligence;

            case 6:
                return attributes.willpower;

            case 7:
                return attributes.spirit;

            case 8:
                return attributes.perception;

            default:
                return 0;
        }
    }

    private static int GetModifierValue(
        CharacterAttributeModifiers modifiers,
        int attributeIndex)
    {
        if (modifiers == null)
            return 0;

        switch (attributeIndex)
        {
            case 0:
                return modifiers.strength;

            case 1:
                return modifiers.dexterity;

            case 2:
                return modifiers.agility;

            case 3:
                return modifiers.vitality;

            case 4:
                return modifiers.endurance;

            case 5:
                return modifiers.intelligence;

            case 6:
                return modifiers.willpower;

            case 7:
                return modifiers.spirit;

            case 8:
                return modifiers.perception;

            default:
                return 0;
        }
    }

    private static void AddToModifier(
        CharacterAttributeModifiers modifiers,
        int attributeIndex,
        int amount)
    {
        if (modifiers == null)
            return;

        switch (attributeIndex)
        {
            case 0:
                modifiers.strength += amount;
                break;

            case 1:
                modifiers.dexterity += amount;
                break;

            case 2:
                modifiers.agility += amount;
                break;

            case 3:
                modifiers.vitality += amount;
                break;

            case 4:
                modifiers.endurance += amount;
                break;

            case 5:
                modifiers.intelligence += amount;
                break;

            case 6:
                modifiers.willpower += amount;
                break;

            case 7:
                modifiers.spirit += amount;
                break;

            case 8:
                modifiers.perception += amount;
                break;
        }
    }   

    private static void AddToAttribute(
        CharacterAttributes attributes,
        int attributeIndex,
        int amount)
    {
        switch (attributeIndex)
        {
            case 0:
                attributes.strength += amount;
                break;

            case 1:
                attributes.dexterity += amount;
                break;

            case 2:
                attributes.agility += amount;
                break;

            case 3:
                attributes.vitality += amount;
                break;

            case 4:
                attributes.endurance += amount;
                break;

            case 5:
                attributes.intelligence += amount;
                break;

            case 6:
                attributes.willpower += amount;
                break;

            case 7:
                attributes.spirit += amount;
                break;

            case 8:
                attributes.perception += amount;
                break;
        }
    }

    private struct LineageContributionRemainder
    {
        public int lineageIndex;
        public int remainder;

        public LineageContributionRemainder(
            int lineageIndex,
            int remainder)
        {
            this.lineageIndex =
                lineageIndex;

            this.remainder =
                remainder;
        }
    }

    private struct AttributeRemainder
    {
        public int attributeIndex;
        public int remainder;

        public AttributeRemainder(
            int attributeIndex,
            int remainder)
        {
            this.attributeIndex =
                attributeIndex;

            this.remainder =
                remainder;
        }
    }
}