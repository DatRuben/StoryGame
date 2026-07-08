using System;
using UnityEngine;

[Serializable]
public class CharacterAttributePreview
{
    [Header("Ancestry Attributes")]
    public CharacterAttributes ancestryAttributes =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int ancestryTotal;

    [Header("Background Modifiers")]
    public CharacterAttributeModifiers backgroundModifiers =
        CharacterAttributeModifiers.CreateZero();

    [SerializeField] private int backgroundModifierTotal;

    [Header("Trait / Quirk Modifiers")]
    public CharacterAttributeModifiers traitModifiers =
        CharacterAttributeModifiers.CreateZero();

    [SerializeField] private int traitModifierTotal;

    [Header("Racial Passive / Starting Skill Modifiers")]
    public CharacterAttributeModifiers racialPassiveModifiers =
        CharacterAttributeModifiers.CreateZero();

    [SerializeField] private int racialPassiveModifierTotal;

    [Header("Level 1 Starting Attributes")]
    public CharacterAttributes levelOneAttributes =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int levelOneTotal;

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

        result =
            CharacterAttributes.AddModifiers(
                result,
                backgroundModifiers
            );

        result =
            CharacterAttributes.AddModifiers(
                result,
                traitModifiers
            );

        result =
            CharacterAttributes.AddModifiers(
                result,
                racialPassiveModifiers
            );

        levelOneAttributes = result;

        ancestryTotal =
            GetAttributeTotal(ancestryAttributes);

        backgroundModifierTotal =
            GetModifierTotal(backgroundModifiers);

        traitModifierTotal =
            GetModifierTotal(traitModifiers);

        racialPassiveModifierTotal =
            GetModifierTotal(racialPassiveModifiers);

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
        CharacterAttributes ancestry,
        CharacterAttributeModifiers background,
        CharacterAttributeModifiers traits,
        CharacterAttributeModifiers racialPassive)
    {
        CharacterAttributePreview preview =
            new CharacterAttributePreview();

        preview.ancestryAttributes =
            CharacterAttributes.Copy(ancestry);

        preview.backgroundModifiers =
            background != null
                ? background
                : CharacterAttributeModifiers.CreateZero();

        preview.traitModifiers =
            traits != null
                ? traits
                : CharacterAttributeModifiers.CreateZero();

        preview.racialPassiveModifiers =
            racialPassive != null
                ? racialPassive
                : CharacterAttributeModifiers.CreateZero();

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

        return modifiers.strength +
               modifiers.dexterity +
               modifiers.agility +
               modifiers.vitality +
               modifiers.endurance +
               modifiers.intelligence +
               modifiers.willpower +
               modifiers.spirit +
               modifiers.perception;
    }
}