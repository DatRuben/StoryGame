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

    public static CharacterAttributeModifiers Multiply(
        CharacterAttributeModifiers modifiers,
        int multiplier)
    {
        if (modifiers == null ||
            multiplier <= 0)
        {
            return CreateZero();
        }

        return new CharacterAttributeModifiers
        {
            strength =
                modifiers.strength * multiplier,

            dexterity =
                modifiers.dexterity * multiplier,

            agility =
                modifiers.agility * multiplier,

            vitality =
                modifiers.vitality * multiplier,

            endurance =
                modifiers.endurance * multiplier,

            intelligence =
                modifiers.intelligence * multiplier,

            willpower =
                modifiers.willpower * multiplier,

            spirit =
                modifiers.spirit * multiplier,

            perception =
                modifiers.perception * multiplier
        };
    }

    public static CharacterAttributeModifiers FromDifference(
    CharacterAttributes value,
    CharacterAttributes comparison)
    {
        if (value == null)
        {
            value =
                CharacterAttributes.CreateDefault(0);
        }

        if (comparison == null)
        {
            comparison =
                CharacterAttributes.CreateDefault(0);
        }

        return new CharacterAttributeModifiers
        {
            strength =
                value.strength -
                comparison.strength,

            dexterity =
                value.dexterity -
                comparison.dexterity,

            agility =
                value.agility -
                comparison.agility,

            vitality =
                value.vitality -
                comparison.vitality,

            endurance =
                value.endurance -
                comparison.endurance,

            intelligence =
                value.intelligence -
                comparison.intelligence,

            willpower =
                value.willpower -
                comparison.willpower,

            spirit =
                value.spirit -
                comparison.spirit,

            perception =
                value.perception -
                comparison.perception
        };
    }

    public bool HasAny()
    {
        return strength != 0 ||
               dexterity != 0 ||
               agility != 0 ||
               vitality != 0 ||
               endurance != 0 ||
               intelligence != 0 ||
               willpower != 0 ||
               spirit != 0 ||
               perception != 0;
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