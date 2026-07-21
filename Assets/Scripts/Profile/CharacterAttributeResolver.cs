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
        CharacterAttributes ancestryAttributes =
            CalculateAncestryAttributes(
                raceDefinition,
                subraceDefinition,
                lineages
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
            racialPassiveModifiers
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