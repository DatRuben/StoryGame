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
    public int technique;

    public static CharacterAttributeModifiers CreateZero()
    {
        return new CharacterAttributeModifiers();
    }
}