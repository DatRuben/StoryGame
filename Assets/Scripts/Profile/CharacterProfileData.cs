using System;
using System.Collections.Generic;

public enum CharacterGender
{
    Male,
    Female
}

[Serializable]
public class CharacterProfileData
{
    public string profileId;
    public string characterName;
    public CharacterGender gender;

    public string raceId;
    public string subraceId;
    public List<string> lineageIds = new();

    public CharacterAppearanceData appearance =
        CharacterAppearanceData.CreateDefault();

    public int level = 1;
    public bool storyCompleted;

    public List<CharacterSkillLevel> skillLevels = new();

    public string createdUtc;
    public string lastSavedUtc;

    public static CharacterProfileData CreateNew(
        string characterName,
        CharacterGender gender,
        string raceId,
        string subraceId,
        List<string> lineageIds = null,
        CharacterAppearanceData appearance = null)
    {
        string now = DateTime.UtcNow.ToString("O");

        return new CharacterProfileData
        {
            profileId = Guid.NewGuid().ToString("N"),
            characterName = characterName,
            gender = gender,
            raceId = raceId,
            subraceId = subraceId,
            lineageIds = lineageIds != null
                ? new List<string>(lineageIds)
                : new List<string>(),
            appearance = CharacterAppearanceData.Copy(appearance),
            level = 1,
            storyCompleted = false,
            createdUtc = now,
            lastSavedUtc = now
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