using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterProfileData
{
    public string profileId;
    public string characterName;

    public string raceId;
    public string subraceId;

    public List<string> lineageIds = new();

    public int level = 1;

    public CharacterAttributes attributes = new();

    public List<CharacterSkillLevel> skillLevels = new();

    public string createdUtc;
    public string lastSavedUtc;

    public static CharacterProfileData CreateNew(string characterName)
    {
        string now = DateTime.UtcNow.ToString("O");

        return new CharacterProfileData
        {
            profileId = Guid.NewGuid().ToString("N"),
            characterName = characterName,
            raceId = "Human",
            subraceId = "Human",
            level = 1,
            createdUtc = now,
            lastSavedUtc = now,
            attributes = CharacterAttributes.CreateDefault()
        };
    }
}

[Serializable]
public class CharacterAttributes
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

    public static CharacterAttributes CreateDefault()
    {
        return new CharacterAttributes
        {
            strength = 5,
            dexterity = 5,
            agility = 5,
            vitality = 5,
            endurance = 5,
            intelligence = 5,
            willpower = 5,
            spirit = 5,
            perception = 5,
            technique = 5
        };
    }
}

[Serializable]
public class CharacterSkillLevel
{
    public string skillId;
    public int level;

    public CharacterSkillLevel(string skillId, int level)
    {
        this.skillId = skillId;
        this.level = level;
    }
}