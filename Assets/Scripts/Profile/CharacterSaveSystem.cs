using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CharacterSaveFile
{
    public List<CharacterProfileData> profiles = new();
}

public static class CharacterSaveSystem
{
    private const string FileName = "characters.json";

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    public static CharacterSaveFile Load()
    {
        if (!File.Exists(SavePath))
            return new CharacterSaveFile();

        string json = File.ReadAllText(SavePath);

        if (string.IsNullOrWhiteSpace(json))
            return new CharacterSaveFile();

        CharacterSaveFile saveFile =
            JsonUtility.FromJson<CharacterSaveFile>(json);

        if (saveFile == null)
            return new CharacterSaveFile();

        if (saveFile.profiles == null)
            saveFile.profiles = new List<CharacterProfileData>();

        return saveFile;
    }

    public static void Save(CharacterSaveFile saveFile)
    {
        if (saveFile == null)
            saveFile = new CharacterSaveFile();

        if (saveFile.profiles == null)
            saveFile.profiles = new List<CharacterProfileData>();

        foreach (CharacterProfileData profile in saveFile.profiles)
        {
            if (profile != null)
                profile.lastSavedUtc = DateTime.UtcNow.ToString("O");
        }

        string json =
            JsonUtility.ToJson(saveFile, true);

        File.WriteAllText(SavePath, json);

        Debug.Log($"Saved character data to: {SavePath}");
    }

    public static void SaveProfile(CharacterProfileData profile)
    {
        if (profile == null)
            return;

        CharacterSaveFile saveFile = Load();

        int existingIndex =
            saveFile.profiles.FindIndex(
                existingProfile =>
                    existingProfile.profileId == profile.profileId
            );

        if (existingIndex >= 0)
            saveFile.profiles[existingIndex] = profile;
        else
            saveFile.profiles.Add(profile);

        Save(saveFile);
    }

    public static bool TryGetProfile(
        string profileId,
        out CharacterProfileData profile)
    {
        CharacterSaveFile saveFile = Load();

        profile =
            saveFile.profiles.Find(
                existingProfile =>
                    existingProfile.profileId == profileId
            );

        return profile != null;
    }

    public static string GetSavePath()
    {
        return SavePath;
    }
}