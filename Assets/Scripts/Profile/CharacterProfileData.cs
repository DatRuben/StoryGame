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
    public string backgroundId;
    public List<string> traitIds = new();

    public CharacterAppearanceData appearance =
        CharacterAppearanceData.CreateDefault();

    public CharacterAttributes createdAttributes =
    CharacterAttributes.CreateDefault(10);

    public CharacterAttributes currentAttributes =
        CharacterAttributes.CreateDefault(10);

    public CharacterBaseStats createdBaseStats =
        CharacterBaseStats.CreateHumanDefault();

    public CharacterBaseStats currentBaseStats =
        CharacterBaseStats.CreateHumanDefault();

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
        string backgroundId = "",
        List<string> traitIds = null,
        CharacterAppearanceData appearance = null,
        CharacterAttributes createdAttributes = null,
        CharacterBaseStats createdBaseStats = null)
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
            backgroundId =
                string.IsNullOrWhiteSpace(backgroundId)
                    ? ""
                    : backgroundId,
            traitIds = traitIds != null
                ? new List<string>(traitIds)
                : new List<string>(),
            appearance = CharacterAppearanceData.Copy(appearance),
            createdAttributes =
                CharacterAttributes.Copy(createdAttributes),
            currentAttributes =
                CharacterAttributes.Copy(createdAttributes),
            createdBaseStats =
                CharacterBaseStats.Copy(createdBaseStats),
            currentBaseStats =
                CharacterBaseStats.Copy(createdBaseStats),
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