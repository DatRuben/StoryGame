using System;

[Serializable]
public class CharacterAttributeModifiers
{
    public int strength;
    public int dexterity;
    public int agility;
    public int vitality;
    public int endurance;
    public int intelligence;
    public int willpower;
    public int spirit;
    public int perception;

    public static CharacterAttributeModifiers CreateZero()
    {
        return new CharacterAttributeModifiers();
    }

    public static CharacterAttributeModifiers Copy(
    CharacterAttributeModifiers source)
    {
        if (source == null)
            return CreateZero();

        return new CharacterAttributeModifiers
        {
            strength = source.strength,
            dexterity = source.dexterity,
            agility = source.agility,
            vitality = source.vitality,
            endurance = source.endurance,
            intelligence = source.intelligence,
            willpower = source.willpower,
            spirit = source.spirit,
            perception = source.perception
        };
    }

    public static CharacterAttributeModifiers Add(
        CharacterAttributeModifiers a,
        CharacterAttributeModifiers b)
    {
        if (a == null && b == null)
            return CreateZero();

        if (a == null)
            return Copy(b);

        if (b == null)
            return Copy(a);

        return new CharacterAttributeModifiers
        {
            strength = a.strength + b.strength,
            dexterity = a.dexterity + b.dexterity,
            agility = a.agility + b.agility,
            vitality = a.vitality + b.vitality,
            endurance = a.endurance + b.endurance,
            intelligence = a.intelligence + b.intelligence,
            willpower = a.willpower + b.willpower,
            spirit = a.spirit + b.spirit,
            perception = a.perception + b.perception
        };
    }

    public int Total()
    {
        return strength +
               dexterity +
               agility +
               vitality +
               endurance +
               intelligence +
               willpower +
               spirit +
               perception;
    }
}