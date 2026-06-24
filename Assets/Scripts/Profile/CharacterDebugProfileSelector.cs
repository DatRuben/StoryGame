using UnityEngine;

public class CharacterDebugProfileSelector : MonoBehaviour
{
    [SerializeField] private bool createDebugProfileOnAwake = true;
    [SerializeField] private string debugCharacterName = "Debug Character";

    private void Awake()
    {
        if (!createDebugProfileOnAwake)
            return;

        if (SelectedCharacterProfile.HasProfile)
            return;

        CharacterProfileData profile =
            CharacterProfileData.CreateNew(debugCharacterName);

        SelectedCharacterProfile.Select(profile);

        Debug.Log(
            $"Selected debug character profile: {profile.characterName}"
        );
    }
}