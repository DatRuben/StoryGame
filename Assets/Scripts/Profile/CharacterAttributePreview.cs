using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterAttributePreview
{
    [Header("Calculation Stages")]
    public CharacterAttributes baseRaceAttributes =
        CharacterAttributes.CreateDefault(10);

    public CharacterAttributes subraceAttributes =
        CharacterAttributes.CreateDefault(10);

    [Header("Ancestry Attributes")]
    public CharacterAttributes ancestryAttributes =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int ancestryTotal;

    [Header("Lineage Influence")]
    [Range(0f, 1f)]
    public float mainAncestryInfluence = 1f;

    public List<LineageInfluencePreview> lineageInfluences =
        new();

    [Header("Post-Ancestry Modifiers")]
    public List<AttributeModifierPreview> modifierSources =
        new();

    [SerializeField]
    private int modifierTotal;

    [Header("Level 1 Starting Attributes")]
    public CharacterAttributes levelOneAttributes =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int levelOneTotal;

    public int ModifierTotal =>
    modifierTotal;

    public CharacterAttributeModifiers SubraceModifiers =>
        CharacterAttributeModifiers.FromDifference(
            subraceAttributes,
            baseRaceAttributes
        );

    public CharacterAttributeModifiers LineageModifiers =>
        CharacterAttributeModifiers.FromDifference(
            ancestryAttributes,
            subraceAttributes
        );

    public CharacterAttributeModifiers PostAncestryModifiers
    {
        get
        {
            CharacterAttributeModifiers total =
                CharacterAttributeModifiers.CreateZero();

            if (modifierSources == null)
                return total;

            foreach (AttributeModifierPreview source
                     in modifierSources)
            {
                if (source?.modifiers == null)
                    continue;

                total =
                    CharacterAttributeModifiers.Add(
                        total,
                        source.modifiers
                    );
            }

            return total;
        }
    }

    public int AncestryTotal
    {
        get { return ancestryTotal; }
    }

    public int BackgroundModifierTotal
    {
        get { return backgroundModifierTotal; }
    }

    public int TraitModifierTotal
    {
        get { return traitModifierTotal; }
    }

    public int RacialPassiveModifierTotal
    {
        get { return racialPassiveModifierTotal; }
    }

    public int LevelOneTotal
    {
        get { return levelOneTotal; }
    }

    public void Recalculate()
    {
        CharacterAttributes result =
            CharacterAttributes.Copy(ancestryAttributes);

        modifierTotal = 0;

        if (modifierSources != null)
        {
            foreach (AttributeModifierPreview source
                     in modifierSources)
            {
                if (source?.modifiers == null)
                    continue;

                result =
                    CharacterAttributes.AddModifiers(
                        result,
                        source.modifiers
                    );

                modifierTotal +=
                    source.modifiers.Total();
            }
        }

        levelOneTotal =
            GetAttributeTotal(levelOneAttributes);
    }

    public static CharacterAttributePreview CreateEmpty()
    {
        CharacterAttributePreview preview =
            new CharacterAttributePreview();

        preview.Recalculate();

        return preview;
    }

    public static CharacterAttributePreview Create(
        CharacterAttributes baseRace,
        CharacterAttributes subrace,
        CharacterAttributes ancestry,
        float mainAncestryInfluence,
        List<LineageInfluencePreview> lineageInfluences,
        List<AttributeModifierPreview> modifierSources)
    {
        CharacterAttributePreview preview =
            new CharacterAttributePreview();

        preview.baseRaceAttributes =
            CharacterAttributes.Copy(baseRace);

        preview.subraceAttributes =
            CharacterAttributes.Copy(subrace);

        preview.ancestryAttributes =
            CharacterAttributes.Copy(ancestry);

        preview.mainAncestryInfluence =
            Mathf.Clamp01(
                mainAncestryInfluence
            );

        preview.lineageInfluences =
            lineageInfluences != null
                ? lineageInfluences
                : new List<LineageInfluencePreview>();

        preview.modifierSources =
            modifierSources != null
                ? modifierSources
                : new List<AttributeModifierPreview>();

        preview.Recalculate();

        return preview;
    }

    private static int GetAttributeTotal(
        CharacterAttributes attributes)
    {
        if (attributes == null)
            return 0;

        return attributes.BasePoints();
    }

    private static int GetModifierTotal(
        CharacterAttributeModifiers modifiers)
    {
        if (modifiers == null)
            return 0;

        return modifiers.Total();
    }
}