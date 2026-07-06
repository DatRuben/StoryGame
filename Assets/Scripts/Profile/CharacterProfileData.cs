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
    public bool storyCompleted;

    public CharacterAttributes allocatedAttributes =
        CharacterAttributes.CreateDefault(0);

    public int unspentAttributePoints;

    public List<CharacterSkillLevel> skillLevels = new();

    public string createdUtc;
    public string lastSavedUtc;

    public static CharacterProfileData CreateNew(
        string characterName,
        string raceId,
        string subraceId,
        List<string> lineageIds = null)
    {
        string now = DateTime.UtcNow.ToString("O");

        return new CharacterProfileData
        {
            profileId = Guid.NewGuid().ToString("N"),
            characterName = characterName,
            raceId = raceId,
            subraceId = subraceId,
            lineageIds = lineageIds != null
                ? new List<string>(lineageIds)
                : new List<string>(),
            level = 1,
            storyCompleted = false,
            createdUtc = now,
            lastSavedUtc = now,
            allocatedAttributes = CharacterAttributes.CreateDefault(0),
            unspentAttributePoints = 5
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