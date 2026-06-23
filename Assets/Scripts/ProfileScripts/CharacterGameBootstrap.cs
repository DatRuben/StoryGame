using UnityEngine;

public class CharacterGameBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject spawnedPlayer;

    private void Start()
    {
        SpawnSelectedCharacter();
    }

    private void SpawnSelectedCharacter()
    {
        if (!SelectedCharacterProfile.HasProfile)
        {
            Debug.LogWarning(
                "No character profile selected. Player will not be spawned."
            );

            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing.");
            return;
        }

        Vector3 spawnPosition =
            spawnPoint != null
                ? spawnPoint.position
                : transform.position;

        Quaternion spawnRotation =
            spawnPoint != null
                ? spawnPoint.rotation
                : Quaternion.identity;

        spawnedPlayer =
            Instantiate(
                playerPrefab,
                spawnPosition,
                spawnRotation
            );

        PlayerCharacterProfile playerProfile =
            spawnedPlayer.GetComponent<PlayerCharacterProfile>();

        if (playerProfile == null)
            playerProfile = spawnedPlayer.AddComponent<PlayerCharacterProfile>();

        playerProfile.Initialize(SelectedCharacterProfile.Profile);

        PlayerInput playerInput =
            spawnedPlayer.GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            Camera mainCamera =
                Camera.main;

            playerInput.SetRuntimeCameraReferences(
                mainCamera,
                mainCamera != null ? mainCamera.transform : null
            );
        }
    }
}