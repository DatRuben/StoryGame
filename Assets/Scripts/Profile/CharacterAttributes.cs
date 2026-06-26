using System;
using UnityEngine;

[Serializable]
public class CharacterAttributes
{
    [Min(0)] public int strength;
    [Min(0)] public int dexterity;
    [Min(0)] public int agility;
    [Min(0)] public int vitality;
    [Min(0)] public int endurance;
    [Min(0)] public int intelligence;
    [Min(0)] public int willpower;
    [Min(0)] public int spirit;
    [Min(0)] public int perception;
    [Min(0)] public int technique;

    public static CharacterAttributes CreateDefault(int value)
    {
        return new CharacterAttributes
        {
            strength = value,
            dexterity = value,
            agility = value,
            vitality = value,
            endurance = value,
            intelligence = value,
            willpower = value,
            spirit = value,
            perception = value,
            technique = value
        };
    }

    public static CharacterAttributes ClampMinimum(
    CharacterAttributes attributes,
    int minimumValue = 1)
    {
        if (attributes == null)
            return CreateDefault(minimumValue);

        return new CharacterAttributes
        {
            strength = Mathf.Max(minimumValue, attributes.strength),
            dexterity = Mathf.Max(minimumValue, attributes.dexterity),
            agility = Mathf.Max(minimumValue, attributes.agility),
            vitality = Mathf.Max(minimumValue, attributes.vitality),
            endurance = Mathf.Max(minimumValue, attributes.endurance),
            intelligence = Mathf.Max(minimumValue, attributes.intelligence),
            willpower = Mathf.Max(minimumValue, attributes.willpower),
            spirit = Mathf.Max(minimumValue, attributes.spirit),
            perception = Mathf.Max(minimumValue, attributes.perception),
            technique = Mathf.Max(minimumValue, attributes.technique)
        };
    }

    public static CharacterAttributes Add(
        CharacterAttributes a,
        CharacterAttributes b)
    {
        if (a == null && b == null)
            return CreateDefault(0);

        if (a == null)
            return Copy(b);

        if (b == null)
            return Copy(a);

        return new CharacterAttributes
        {
            strength = a.strength + b.strength,
            dexterity = a.dexterity + b.dexterity,
            agility = a.agility + b.agility,
            vitality = a.vitality + b.vitality,
            endurance = a.endurance + b.endurance,
            intelligence = a.intelligence + b.intelligence,
            willpower = a.willpower + b.willpower,
            spirit = a.spirit + b.spirit,
            perception = a.perception + b.perception,
            technique = a.technique + b.technique
        };
    }

    public static CharacterAttributes Copy(CharacterAttributes source)
    {
        if (source == null)
            return CreateDefault(0);

        return new CharacterAttributes
        {
            strength = source.strength,
            dexterity = source.dexterity,
            agility = source.agility,
            vitality = source.vitality,
            endurance = source.endurance,
            intelligence = source.intelligence,
            willpower = source.willpower,
            spirit = source.spirit,
            perception = source.perception,
            technique = source.technique
        };
    }

    public static CharacterAttributes AddModifiers(
    CharacterAttributes attributes,
    CharacterAttributeModifiers modifiers)
    {
        if (attributes == null)
            attributes = CreateDefault(0);

        if (modifiers == null)
            return Copy(attributes);

        return new CharacterAttributes
        {
            strength = attributes.strength + modifiers.strength,
            dexterity = attributes.dexterity + modifiers.dexterity,
            agility = attributes.agility + modifiers.agility,
            vitality = attributes.vitality + modifiers.vitality,
            endurance = attributes.endurance + modifiers.endurance,
            intelligence = attributes.intelligence + modifiers.intelligence,
            willpower = attributes.willpower + modifiers.willpower,
            spirit = attributes.spirit + modifiers.spirit,
            perception = attributes.perception + modifiers.perception,
            technique = attributes.technique + modifiers.technique
        };
    }
}