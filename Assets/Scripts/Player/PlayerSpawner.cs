using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private RaceProfileLibrary raceProfileLibrary;

    [Header("Fallback")]
    [SerializeField] private bool createDefaultProfileIfNone = true;
    [SerializeField] private string defaultCharacterName = "New Character";

    private GameObject spawnedPlayer;

    public GameObject SpawnedPlayer => spawnedPlayer;

    private void Start()
    {
        SpawnSelectedCharacter();
    }

    public bool SpawnSelectedCharacter()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not spawn because Player Prefab is missing.",
                this
            );

            return false;
        }

        if (raceProfileLibrary == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not spawn because RaceProfileLibrary is missing.",
                this
            );

            return false;
        }

        if (!TryGetProfileToSpawn(out CharacterProfileData profile))
            return false;

        RaceProfile raceProfile =
            GetRaceProfileFor(profile);

        if (raceProfile == null)
            return false;

        Vector3 position =
            spawnPoint != null
                ? spawnPoint.position
                : transform.position;

        Quaternion rotation =
            spawnPoint != null
                ? spawnPoint.rotation
                : transform.rotation;

        spawnedPlayer =
            Instantiate(
                playerPrefab,
                position,
                rotation
            );

        PlayerCharacterProfile playerCharacterProfile =
            spawnedPlayer.GetComponent<PlayerCharacterProfile>();

        if (playerCharacterProfile == null)
            playerCharacterProfile = spawnedPlayer.AddComponent<PlayerCharacterProfile>();

        playerCharacterProfile.Initialize(
            profile,
            raceProfile,
            new LineageProfile[0]
        );

        return true;
    }

    private bool TryGetProfileToSpawn(
        out CharacterProfileData profile)
    {
        if (CharacterSelection.TryGetSelectedProfile(out profile))
            return true;

        if (!createDefaultProfileIfNone)
        {
            Debug.LogWarning(
                "PlayerSpawner could not spawn because no character profile is selected.",
                this
            );

            return false;
        }

        RaceProfile defaultRaceProfile =
            raceProfileLibrary.GetDefaultRaceProfile();

        if (defaultRaceProfile == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not create a default character because the RaceProfileLibrary has no race profiles.",
                this
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(defaultRaceProfile.profileId))
        {
            Debug.LogWarning(
                "PlayerSpawner could not create a default character because the default RaceProfile has no profileId.",
                defaultRaceProfile
            );

            return false;
        }

        profile =
            CharacterSelection.CreateCharacter(
                defaultCharacterName,
                defaultRaceProfile.profileId
            );

        return profile != null;
    }

    private RaceProfile GetRaceProfileFor(
        CharacterProfileData profile)
    {
        if (profile == null)
            return null;

        if (raceProfileLibrary.TryGetRaceProfile(
            profile.raceProfileId,
            out RaceProfile raceProfile))
        {
            return raceProfile;
        }

        RaceProfile fallbackRaceProfile =
            raceProfileLibrary.GetDefaultRaceProfile();

        if (fallbackRaceProfile == null)
        {
            Debug.LogWarning(
                $"PlayerSpawner could not find RaceProfile '{profile.raceProfileId}' and no default RaceProfile exists.",
                this
            );

            return null;
        }

        Debug.LogWarning(
            $"PlayerSpawner could not find RaceProfile '{profile.raceProfileId}'. Using default RaceProfile '{fallbackRaceProfile.profileId}'.",
            fallbackRaceProfile
        );

        return fallbackRaceProfile;
    }
}