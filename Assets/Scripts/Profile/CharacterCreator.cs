using System.Collections.Generic;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{
    [SerializeField] private CharacterDataLibrary characterDataLibrary;

    [Header("Current Selection")]
    [SerializeField] private string selectedRaceProfileId;
    [SerializeField] private List<string> selectedLineageIds = new();

    public string SelectedRaceProfileId => selectedRaceProfileId;
    public IReadOnlyList<string> SelectedLineageIds => selectedLineageIds;

    public bool SelectRace(
        string raceProfileId,
        out string errorMessage)
    {
        errorMessage = "";

        if (characterDataLibrary == null)
        {
            errorMessage = "CharacterDataLibrary is missing.";
            return false;
        }

        if (!characterDataLibrary.TryGetRaceProfile(
            raceProfileId,
            out RaceProfile raceProfile))
        {
            errorMessage =
                $"RaceProfile '{raceProfileId}' was not found.";

            return false;
        }

        selectedRaceProfileId = raceProfile.profileId;

        CleanSelectedLineages(raceProfile);

        return true;
    }

    public bool ToggleLineage(
        string lineageId,
        out string errorMessage)
    {
        errorMessage = "";

        if (string.IsNullOrWhiteSpace(lineageId))
        {
            errorMessage = "Lineage ID is missing.";
            return false;
        }

        if (!TryGetSelectedRace(out RaceProfile raceProfile))
        {
            errorMessage = "No race is selected.";
            return false;
        }

        if (selectedLineageIds.Contains(lineageId))
        {
            selectedLineageIds.Remove(lineageId);
            return true;
        }

        if (!raceProfile.CanUseLineages())
        {
            errorMessage =
                $"{raceProfile.displayName} cannot use lineages.";

            return false;
        }

        if (selectedLineageIds.Count >= raceProfile.maxLineages)
        {
            errorMessage =
                $"{raceProfile.displayName} can only use up to {raceProfile.maxLineages} lineages.";

            return false;
        }

        if (!raceProfile.IsLineageAllowed(lineageId))
        {
            errorMessage =
                $"{raceProfile.displayName} cannot use lineage '{lineageId}'.";

            return false;
        }

        selectedLineageIds.Add(lineageId);

        if (!raceProfile.AreLineagesValid(
            selectedLineageIds,
            out errorMessage))
        {
            selectedLineageIds.Remove(lineageId);
            return false;
        }

        return true;
    }

    public bool TryCreateCharacter(
        string characterName,
        out CharacterProfileData profile,
        out string errorMessage)
    {
        profile = null;
        errorMessage = "";

        if (string.IsNullOrWhiteSpace(characterName))
        {
            errorMessage = "Character name is required.";
            return false;
        }

        if (!TryGetSelectedRace(out RaceProfile raceProfile))
        {
            errorMessage = "No race is selected.";
            return false;
        }

        return CharacterSelection.TryCreateCharacter(
            characterName,
            raceProfile,
            selectedLineageIds,
            out profile,
            out errorMessage
        );
    }

    public bool CanCreateCharacter(
        out string errorMessage)
    {
        errorMessage = "";

        if (!TryGetSelectedRace(out RaceProfile raceProfile))
        {
            errorMessage = "No race is selected.";
            return false;
        }

        return raceProfile.AreLineagesValid(
            selectedLineageIds,
            out errorMessage
        );
    }

    public void ClearLineages()
    {
        selectedLineageIds.Clear();
    }

    private bool TryGetSelectedRace(
        out RaceProfile raceProfile)
    {
        raceProfile = null;

        if (characterDataLibrary == null)
            return false;

        if (string.IsNullOrWhiteSpace(selectedRaceProfileId))
            return false;

        return characterDataLibrary.TryGetRaceProfile(
            selectedRaceProfileId,
            out raceProfile
        );
    }

    private void CleanSelectedLineages(
        RaceProfile raceProfile)
    {
        if (raceProfile == null)
        {
            selectedLineageIds.Clear();
            return;
        }

        if (!raceProfile.CanUseLineages())
        {
            selectedLineageIds.Clear();
            return;
        }

        for (int i = selectedLineageIds.Count - 1; i >= 0; i--)
        {
            if (!raceProfile.IsLineageAllowed(selectedLineageIds[i]))
                selectedLineageIds.RemoveAt(i);
        }

        while (selectedLineageIds.Count > raceProfile.maxLineages)
        {
            selectedLineageIds.RemoveAt(
                selectedLineageIds.Count - 1
            );
        }
    }
}