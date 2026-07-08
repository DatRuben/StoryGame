using System.Collections.Generic;
using UnityEngine;

public static class CharacterSelection
{
    private const string SelectedProfileIdKey =
        "SelectedCharacterProfileId";

    public static List<CharacterProfileData> GetProfiles()
    {
        CharacterSaveFile saveFile =
            CharacterSaveSystem.Load();

        if (saveFile.profiles == null)
            saveFile.profiles = new List<CharacterProfileData>();

        return saveFile.profiles;
    }

    public static CharacterProfileData CreateCharacter(
        string characterName,
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<string> lineageIds)
    {
        CharacterProfileData profile =
            CharacterProfileData.CreateNew(
                characterName,
                raceDefinition.raceId,
                subraceDefinition.subraceId,
                lineageIds
            );

        CharacterSaveSystem.SaveProfile(profile);
        SelectProfile(profile.profileId);

        return profile;
    }

    public static bool TryCreateCharacter(
        string characterName,
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        List<LineageDefinition> lineageDefinitions,
        out CharacterProfileData profile,
        out string errorMessage)
    {
        profile = null;
        errorMessage = "";

        if (string.IsNullOrWhiteSpace(characterName))
        {
            errorMessage = "Character name is required.";
            return false;
        }

        if (raceDefinition == null)
        {
            errorMessage = "Race definition is missing.";
            return false;
        }

        if (subraceDefinition == null)
        {
            errorMessage = "Subrace definition is missing.";
            return false;
        }

        if (subraceDefinition.race == null ||
            subraceDefinition.race.raceId != raceDefinition.raceId)
        {
            errorMessage =
                $"{subraceDefinition.displayName} does not belong to {raceDefinition.displayName}.";

            return false;
        }

        if (!raceDefinition.AreLineagesValid(
            lineageDefinitions,
            out errorMessage))
        {
            return false;
        }

        List<string> lineageIds = new();

        if (lineageDefinitions != null)
        {
            foreach (LineageDefinition lineageDefinition in lineageDefinitions)
            {
                if (lineageDefinition != null)
                    lineageIds.Add(lineageDefinition.lineageId);
            }
        }

        profile =
            CreateCharacter(
                characterName,
                raceDefinition,
                subraceDefinition,
                lineageIds
            );

        return true;
    }

    public static void SelectProfile(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            return;

        PlayerPrefs.SetString(
            SelectedProfileIdKey,
            profileId
        );

        PlayerPrefs.Save();
    }

    public static bool TryGetSelectedProfile(
        out CharacterProfileData profile)
    {
        profile = null;

        string selectedProfileId =
            PlayerPrefs.GetString(
                SelectedProfileIdKey,
                string.Empty
            );

        if (string.IsNullOrWhiteSpace(selectedProfileId))
            return false;

        return CharacterSaveSystem.TryGetProfile(
            selectedProfileId,
            out profile
        );
    }

    public static bool HasSelectedProfile()
    {
        return TryGetSelectedProfile(out _);
    }

    public static void ClearSelection()
    {
        PlayerPrefs.DeleteKey(SelectedProfileIdKey);
        PlayerPrefs.Save();
    }
}