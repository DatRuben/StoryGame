using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CharacterDataLibrary characterDataLibrary;

    [Header("Fallback")]
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private bool createDefaultProfileIfNone = false;
    [SerializeField] private string defaultCharacterName = "New Character";

    private GameObject spawnedPlayer;

    public GameObject SpawnedPlayer => spawnedPlayer;

    private void Start()
    {
        if (spawnOnStart)
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

        if (characterDataLibrary == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not spawn because CharacterDataLibrary is missing.",
                this
            );

            return false;
        }

        if (!TryGetProfileToSpawn(out CharacterProfileData profile))
            return false;

        if (!TryGetRuntimeDefinitions(
            profile,
            out RaceDefinition raceDefinition,
            out SubraceDefinition subraceDefinition,
            out LineageSelection[] lineageSelections))
        {
            return false;
        }

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

        InitializePlayer(
            spawnedPlayer,
            profile,
            raceDefinition,
            subraceDefinition,
            lineageSelections
        );

        return true;
    }

    public bool UsePlayer(
        GameObject player,
        CharacterProfileData profile)
    {
        if (player == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not use the existing player because it is missing.",
                this
            );

            return false;
        }

        if (characterDataLibrary == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not use the existing player because CharacterDataLibrary is missing.",
                this
            );

            return false;
        }

        if (profile == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not use the existing player because its character profile is missing.",
                this
            );

            return false;
        }

        if (!TryGetRuntimeDefinitions(
            profile,
            out RaceDefinition raceDefinition,
            out SubraceDefinition subraceDefinition,
            out LineageSelection[] lineageSelections))
        {
            return false;
        }

        spawnedPlayer =
            player;

        InitializePlayer(
            spawnedPlayer,
            profile,
            raceDefinition,
            subraceDefinition,
            lineageSelections
        );

        PrepareForGameplay(
            spawnedPlayer
        );

        return true;
    }

    private void PrepareForGameplay(
        GameObject player)
    {
        if (player == null)
            return;

        player.SetActive(false);

        player.transform.SetParent(
            null,
            true
        );

        Vector3 position =
            spawnPoint != null
                ? spawnPoint.position
                : transform.position;

        Quaternion rotation =
            spawnPoint != null
                ? spawnPoint.rotation
                : transform.rotation;

        player.transform.SetPositionAndRotation(
            position,
            rotation
        );

        CapsuleCollider capsuleCollider =
            player.GetComponent<CapsuleCollider>();

        if (capsuleCollider != null)
            capsuleCollider.enabled = true;

        Rigidbody rigidbody =
            player.GetComponent<Rigidbody>();

        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
        }

        PlayerInput playerInput =
            player.GetComponent<PlayerInput>();

        if (playerInput != null)
            playerInput.enabled = true;

        PlayerStorageContainerInteract storageInteract =
            player.GetComponent<
                PlayerStorageContainerInteract>();

        if (storageInteract != null)
            storageInteract.enabled = true;

        player.SetActive(true);
    }

    private void InitializePlayer(
        GameObject player,
        CharacterProfileData profile,
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        LineageSelection[] lineageSelections)
    {
        PlayerCharacterProfile playerCharacterProfile =
            player.GetComponent<PlayerCharacterProfile>();

        if (playerCharacterProfile == null)
        {
            playerCharacterProfile =
                player.AddComponent<
                    PlayerCharacterProfile>();
        }

        playerCharacterProfile.Initialize(
            profile,
            raceDefinition,
            subraceDefinition,
            lineageSelections
        );
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

        RaceDefinition defaultRaceDefinition =
            characterDataLibrary.GetDefaultRaceDefinition();

        if (defaultRaceDefinition == null)
        {
            Debug.LogWarning(
                "PlayerSpawner could not create a default character because the CharacterDataLibrary has no race definitions.",
                this
            );

            return false;
        }

        if (defaultRaceDefinition.standardSubrace == null)
        {
            Debug.LogWarning(
                $"PlayerSpawner could not create a default character because {defaultRaceDefinition.displayName} has no standard subrace.",
                defaultRaceDefinition
            );

            return false;
        }

        ResolvedCharacterStats resolvedStats =
                CharacterStatsResolver.ResolveCharacter(
                    defaultRaceDefinition,
                    defaultRaceDefinition.standardSubrace,
                    new List<LineageSelection>()
                );

        profile =
            CharacterSelection.CreateCharacter(
                defaultCharacterName,
                CharacterGender.Male,
                defaultRaceDefinition,
                defaultRaceDefinition.standardSubrace,
                new List<string>(),
                "",
                new List<string>(),
                CharacterAppearanceData.CreateDefault(),

                CharacterAttributes.Copy(
                    resolvedStats.finalAttributes
                ),

                CharacterBaseStats.Copy(
                    resolvedStats.totalBaseStats
                ),

                FinalMovementStats.Copy(
                    resolvedStats.movementStats
                )
            );

        return profile != null;
    }

    private bool TryGetRuntimeDefinitions(
        CharacterProfileData profile,
        out RaceDefinition raceDefinition,
        out SubraceDefinition subraceDefinition,
        out LineageSelection[] lineageSelections)
    {
        raceDefinition = null;
        subraceDefinition = null;
        lineageSelections = null;

        if (profile == null)
            return false;

        if (!characterDataLibrary.TryGetRaceDefinition(
            profile.raceId,
            out raceDefinition))
        {
            Debug.LogWarning(
                $"PlayerSpawner could not find RaceDefinition '{profile.raceId}'.",
                this
            );

            return false;
        }

        if (!characterDataLibrary.TryGetSubraceDefinition(
            profile.subraceId,
            out subraceDefinition))
        {
            Debug.LogWarning(
                $"PlayerSpawner could not find SubraceDefinition '{profile.subraceId}'.",
                this
            );

            return false;
        }

        List<LineageSelection> selectionList =
            characterDataLibrary.GetLineageSelections(
                profile.lineageIds
            );

        lineageSelections =
            selectionList.ToArray();

        return true;
    }
}