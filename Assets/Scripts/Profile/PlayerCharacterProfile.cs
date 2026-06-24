using UnityEngine;

public class PlayerCharacterProfile : MonoBehaviour
{
    public CharacterProfileData Profile { get; private set; }

    public void Initialize(CharacterProfileData profile)
    {
        Profile = profile;

        if (Profile == null)
        {
            Debug.LogWarning("Player spawned without a character profile.");
            return;
        }

        gameObject.name =
            $"Player_{Profile.characterName}";
    }
}