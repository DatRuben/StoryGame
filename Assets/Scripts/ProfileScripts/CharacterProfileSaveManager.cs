using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CharacterProfileSaveManager
{
    private const string SaveFolderName = "CharacterProfiles";
    private const string SaveExtension = ".json";

    private static string SaveFolderPath =>
        Path.Combine(Application.persistentDataPath, SaveFolderName);

    public static CharacterProfileData CreateProfile(string characterName)
    {
        CharacterProfileData profile =
            CharacterProfileData.CreateNew(characterName);

        SaveProfile(profile);

        return profile;
    }

    public static void SaveProfile(CharacterProfileData profile)
    {
        if (profile == null)
        {
            Debug.LogWarning("Cannot save a null character profile.");
            return;
        }

        if (string.IsNullOrWhiteSpace(profile.profileId))
            profile.profileId = Guid.NewGuid().ToString("N");

        profile.lastSavedUtc =
            DateTime.UtcNow.ToString("O");

        EnsureSaveFolderExists();

        string json =
            JsonUtility.ToJson(profile, true);

        File.WriteAllText(
            GetProfilePath(profile.profileId),
            json
        );
    }

    public static bool TryLoadProfile(
        string profileId,
        out CharacterProfileData profile)
    {
        profile = null;

        if (string.IsNullOrWhiteSpace(profileId))
            return false;

        string path =
            GetProfilePath(profileId);

        if (!File.Exists(path))
            return false;

        string json =
            File.ReadAllText(path);

        profile =
            JsonUtility.FromJson<CharacterProfileData>(json);

        return profile != null;
    }

    public static List<CharacterProfileData> LoadAllProfiles()
    {
        List<CharacterProfileData> profiles = new();

        EnsureSaveFolderExists();

        string[] files =
            Directory.GetFiles(
                SaveFolderPath,
                "*" + SaveExtension
            );

        foreach (string file in files)
        {
            try
            {
                string json =
                    File.ReadAllText(file);

                CharacterProfileData profile =
                    JsonUtility.FromJson<CharacterProfileData>(json);

                if (profile != null)
                    profiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Failed to load character profile at {file}: {exception.Message}"
                );
            }
        }

        profiles.Sort(
            (a, b) => string.Compare(
                b.lastSavedUtc,
                a.lastSavedUtc,
                StringComparison.Ordinal
            )
        );

        return profiles;
    }

    public static bool DeleteProfile(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            return false;

        string path =
            GetProfilePath(profileId);

        if (!File.Exists(path))
            return false;

        File.Delete(path);
        return true;
    }

    public static string GetProfilePath(string profileId)
    {
        string safeId =
            MakeSafeFileName(profileId);

        return Path.Combine(
            SaveFolderPath,
            safeId + SaveExtension
        );
    }

    private static void EnsureSaveFolderExists()
    {
        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);
    }

    private static string MakeSafeFileName(string value)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value;
    }
}