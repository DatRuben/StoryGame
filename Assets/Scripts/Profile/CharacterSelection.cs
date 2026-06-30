using System.Collections.Generic;
using UnityEngine;

public static class CharacterSelection
{
    private const string SelectedProfileIdKey = "SelectedCharacterProfileId";

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
        string raceProfileId = "Human_Default")
    {
        CharacterProfileData profile =
            CharacterProfileData.CreateNew(
                characterName,
                raceProfileId
            );

        CharacterSaveSystem.SaveProfile(profile);
        SelectProfile(profile.profileId);

        return profile;
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