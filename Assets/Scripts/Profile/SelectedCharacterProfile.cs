public static class SelectedCharacterProfile
{
    public static CharacterProfileData Profile { get; private set; }

    public static bool HasProfile =>
        Profile != null;

    public static void Select(CharacterProfileData profile)
    {
        Profile = profile;
    }

    public static void Clear()
    {
        Profile = null;
    }
}