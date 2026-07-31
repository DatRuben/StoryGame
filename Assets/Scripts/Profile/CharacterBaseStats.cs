using System;

[Serializable]
public class CharacterBaseStats
{
    public int health;
    public int stamina;
    public int mana;
    public int staggerResist;
    public int carryWeight;

    public static CharacterBaseStats CreateHumanDefault()
    {
        return new CharacterBaseStats
        {
            health = 100,
            stamina = 100,
            mana = 50,
            staggerResist = 50,
            carryWeight = 50
        };
    }

    public static CharacterBaseStats CreateZero()
    {
        return new CharacterBaseStats();
    }

    public static CharacterBaseStats Copy(
    CharacterBaseStats source)
    {
        if (source == null)
            return CreateHumanDefault();

        return new CharacterBaseStats
        {
            health = source.health,
            stamina = source.stamina,
            mana = source.mana,
            staggerResist = source.staggerResist,
            carryWeight = source.carryWeight
        };
    }

    public static CharacterBaseStats Add(
        CharacterBaseStats baseStats,
        CharacterBaseStats modifiers)
    {
        if (baseStats == null)
            baseStats = CreateHumanDefault();

        if (modifiers == null)
            modifiers = CreateZero();

        return new CharacterBaseStats
        {
            health = baseStats.health + modifiers.health,
            stamina = baseStats.stamina + modifiers.stamina,
            mana = baseStats.mana + modifiers.mana,
            staggerResist = baseStats.staggerResist + modifiers.staggerResist,
            carryWeight = baseStats.carryWeight + modifiers.carryWeight
        };
    }
}