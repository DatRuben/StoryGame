using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Race Profile Library")]
public class RaceProfileLibrary : ScriptableObject
{
    [SerializeField] private List<RaceProfile> raceProfiles = new();

    public IReadOnlyList<RaceProfile> RaceProfiles => raceProfiles;

    public bool TryGetRaceProfile(
        string profileId,
        out RaceProfile raceProfile)
    {
        raceProfile = null;

        if (string.IsNullOrWhiteSpace(profileId))
            return false;

        if (raceProfiles == null)
            return false;

        foreach (RaceProfile profile in raceProfiles)
        {
            if (profile == null)
                continue;

            if (string.Equals(
                profile.profileId,
                profileId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                raceProfile = profile;
                return true;
            }
        }

        return false;
    }

    public RaceProfile GetDefaultRaceProfile()
    {
        if (raceProfiles == null)
            return null;

        foreach (RaceProfile profile in raceProfiles)
        {
            if (profile != null)
                return profile;
        }

        return null;
    }
}