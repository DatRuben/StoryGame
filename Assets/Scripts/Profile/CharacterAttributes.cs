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

    public static CharacterAttributes CreateDefault(int value = 5)
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
}