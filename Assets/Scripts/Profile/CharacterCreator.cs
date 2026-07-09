using System.Collections.Generic;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{
    [SerializeField] private CharacterDataLibrary characterDataLibrary;

    [Header("Current Selection")]
    [SerializeField] private string selectedRaceId;
    [SerializeField] private string selectedSubraceId;
    [SerializeField] private List<string> selectedLineageIds = new();
    [SerializeField] private CharacterGender selectedGender = CharacterGender.Male;
    [SerializeField] private string selectedCharacterName = "";

    public string SelectedRaceId => selectedRaceId;
    public string SelectedSubraceId => selectedSubraceId;
    public IReadOnlyList<string> SelectedLineageIds => selectedLineageIds;
    public CharacterGender SelectedGender => selectedGender;
    public string SelectedCharacterName => selectedCharacterName;

    public void SelectGender(
        CharacterGender gender)
    {
        selectedGender = gender;
    }

    public void SetCharacterName(
    string characterName)
    {
        selectedCharacterName =
            string.IsNullOrWhiteSpace(characterName)
                ? ""
                : characterName.Trim();
    }

    public bool SelectRace(
        string raceId,
        out string errorMessage)
    {
        errorMessage = "";

        if (!TryGetRaceDefinition(
            raceId,
            out RaceDefinition raceDefinition,
            out errorMessage))
        {
            return false;
        }

        selectedRaceId = raceDefinition.raceId;

        SelectDefaultSubraceFor(raceDefinition);

        TryGetSelectedSubrace(
            out SubraceDefinition selectedSubraceDefinition
        );

        CleanSelectedLineages(
            raceDefinition,
            selectedSubraceDefinition
        );

        return true;
    }

    public bool SelectSubrace(
        string subraceId,
        out string errorMessage)
    {
        errorMessage = "";

        if (!TryGetSubraceDefinition(
            subraceId,
            out SubraceDefinition subraceDefinition,
            out errorMessage))
        {
            return false;
        }

        if (subraceDefinition.race == null)
        {
            errorMessage =
                $"{subraceDefinition.displayName} has no RaceDefinition assigned.";

            return false;
        }

        selectedRaceId = subraceDefinition.race.raceId;
        selectedSubraceId = subraceDefinition.subraceId;

        CleanSelectedLineages(
            subraceDefinition.race,
            subraceDefinition
        );

        return true;
    }

    public bool ToggleLineage(
        string lineageId,
        out string errorMessage)
    {
        errorMessage = "";

        if (!TryGetSelectedRace(out RaceDefinition raceDefinition))
        {
            errorMessage = "No race is selected.";
            return false;
        }

        if (!TryGetSelectedSubrace(out SubraceDefinition subraceDefinition))
        {
            errorMessage = "No subrace is selected.";
            return false;
        }

        if (!TryGetLineageDefinition(
            lineageId,
            out LineageDefinition lineageDefinition,
            out errorMessage))
        {
            return false;
        }

        if (selectedLineageIds.Contains(lineageDefinition.lineageId))
        {
            selectedLineageIds.Remove(lineageDefinition.lineageId);
            return true;
        }

        if (!raceDefinition.IsLineageAllowed(
            lineageDefinition,
            subraceDefinition))
        {
            errorMessage =
                $"{raceDefinition.displayName} cannot use lineage {lineageDefinition.displayName}.";

            return false;
        }

        selectedLineageIds.Add(lineageDefinition.lineageId);

        if (!AreSelectedLineagesValid(
            raceDefinition,
            subraceDefinition,
            out errorMessage))
        {
            selectedLineageIds.Remove(lineageDefinition.lineageId);
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

        if (!TryGetSelectedRace(out RaceDefinition raceDefinition))
        {
            errorMessage = "No race is selected.";
            return false;
        }

        if (!TryGetSelectedSubrace(out SubraceDefinition subraceDefinition))
        {
            errorMessage = "No subrace is selected.";
            return false;
        }

        List<LineageDefinition> lineageDefinitions =
            GetSelectedLineageDefinitions();

        return CharacterSelection.TryCreateCharacter(
                characterName,
                selectedGender,
                raceDefinition,
                subraceDefinition,
                lineageDefinitions,
                out profile,
                out errorMessage
        );
    }

    public bool CanCreateCharacter(
        out string errorMessage)
    {
        errorMessage = "";

        if (!TryGetSelectedRace(out RaceDefinition raceDefinition))
        {
            errorMessage = "No race is selected.";
            return false;
        }

        if (!TryGetSelectedSubrace(out SubraceDefinition subraceDefinition))
        {
            errorMessage = "No subrace is selected.";
            return false;
        }

        if (subraceDefinition.race == null ||
            subraceDefinition.race.raceId != raceDefinition.raceId)
        {
            errorMessage =
                $"{subraceDefinition.displayName} does not belong to {raceDefinition.displayName}.";

            return false;
        }

        return AreSelectedLineagesValid(
            raceDefinition,
            subraceDefinition,
            out errorMessage
        );
    }

    public void ClearLineages()
    {
        selectedLineageIds.Clear();
    }

    private void SelectDefaultSubraceFor(
        RaceDefinition raceDefinition)
    {
        selectedSubraceId = "";

        if (raceDefinition == null)
            return;

        if (raceDefinition.standardSubrace != null)
            selectedSubraceId = raceDefinition.standardSubrace.subraceId;
    }

    private bool TryGetRaceDefinition(
        string raceId,
        out RaceDefinition raceDefinition,
        out string errorMessage)
    {
        raceDefinition = null;
        errorMessage = "";

        if (characterDataLibrary == null)
        {
            errorMessage = "CharacterDataLibrary is missing.";
            return false;
        }

        if (!characterDataLibrary.TryGetRaceDefinition(
            raceId,
            out raceDefinition))
        {
            errorMessage =
                $"RaceDefinition '{raceId}' was not found.";

            return false;
        }

        return true;
    }

    private bool TryGetSubraceDefinition(
        string subraceId,
        out SubraceDefinition subraceDefinition,
        out string errorMessage)
    {
        subraceDefinition = null;
        errorMessage = "";

        if (characterDataLibrary == null)
        {
            errorMessage = "CharacterDataLibrary is missing.";
            return false;
        }

        if (!characterDataLibrary.TryGetSubraceDefinition(
            subraceId,
            out subraceDefinition))
        {
            errorMessage =
                $"SubraceDefinition '{subraceId}' was not found.";

            return false;
        }

        return true;
    }

    private bool TryGetLineageDefinition(
        string lineageId,
        out LineageDefinition lineageDefinition,
        out string errorMessage)
    {
        lineageDefinition = null;
        errorMessage = "";

        if (characterDataLibrary == null)
        {
            errorMessage = "CharacterDataLibrary is missing.";
            return false;
        }

        if (!characterDataLibrary.TryGetLineageDefinition(
            lineageId,
            out lineageDefinition))
        {
            errorMessage =
                $"LineageDefinition '{lineageId}' was not found.";

            return false;
        }

        return true;
    }

    private bool TryGetSelectedRace(
        out RaceDefinition raceDefinition)
    {
        raceDefinition = null;

        if (characterDataLibrary == null)
            return false;

        if (string.IsNullOrWhiteSpace(selectedRaceId))
            return false;

        return characterDataLibrary.TryGetRaceDefinition(
            selectedRaceId,
            out raceDefinition
        );
    }

    private bool TryGetSelectedSubrace(
        out SubraceDefinition subraceDefinition)
    {
        subraceDefinition = null;

        if (characterDataLibrary == null)
            return false;

        if (string.IsNullOrWhiteSpace(selectedSubraceId))
            return false;

        return characterDataLibrary.TryGetSubraceDefinition(
            selectedSubraceId,
            out subraceDefinition
        );
    }

    private List<LineageDefinition> GetSelectedLineageDefinitions()
    {
        return characterDataLibrary.GetLineageDefinitions(
            selectedLineageIds
        );
    }

    private bool AreSelectedLineagesValid(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        out string errorMessage)
    {
        return raceDefinition.AreLineagesValid(
            subraceDefinition,
            GetSelectedLineageDefinitions(),
            out errorMessage
        );
    }

    public bool TryGetFinalAttributesPreview(
        out CharacterAttributes finalAttributes,
        out int totalPoints,
        out string errorMessage)
    {
        finalAttributes = null;
        totalPoints = 0;
        errorMessage = "";

        if (!TryGetAttributePreview(
            out CharacterAttributePreview attributePreview,
            out errorMessage))
        {
            return false;
        }

        if (attributePreview == null ||
            attributePreview.levelOneAttributes == null)
        {
            errorMessage = "Attribute preview could not be created.";
            return false;
        }

        finalAttributes =
            CharacterAttributes.Copy(
                attributePreview.levelOneAttributes
            );

        totalPoints =
            attributePreview.LevelOneTotal;

        return true;
    }

    public bool TryGetAttributePreview(
        out CharacterAttributePreview attributePreview,
        out string errorMessage)
    {
        attributePreview = null;
        errorMessage = "";

        if (!TryGetSelectedRace(out RaceDefinition raceDefinition))
        {
            errorMessage = "No race is selected.";
            return false;
        }

        if (!TryGetSelectedSubrace(out SubraceDefinition subraceDefinition))
        {
            errorMessage = "No subrace is selected.";
            return false;
        }

        if (!AreSelectedLineagesValid(
            raceDefinition,
            subraceDefinition,
            out errorMessage))
        {
            return false;
        }

        attributePreview =
            CharacterAttributeResolver.CreatePreview(
                raceDefinition,
                subraceDefinition,
                GetSelectedLineageDefinitions()
            );

        return true;
    }

    private void CleanSelectedLineages(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition)
    {
        if (raceDefinition == null ||
            !raceDefinition.CanUseLineages())
        {
            selectedLineageIds.Clear();
            return;
        }

        for (int i = selectedLineageIds.Count - 1; i >= 0; i--)
        {
            if (!characterDataLibrary.TryGetLineageDefinition(
                selectedLineageIds[i],
                out LineageDefinition lineageDefinition))
            {
                selectedLineageIds.RemoveAt(i);
                continue;
            }

            if (!raceDefinition.IsLineageAllowed(
                lineageDefinition,
                subraceDefinition))
                selectedLineageIds.RemoveAt(i);
        }

        while (selectedLineageIds.Count > raceDefinition.maxLineages)
        {
            selectedLineageIds.RemoveAt(
                selectedLineageIds.Count - 1
            );
        }
    }

    public bool TryCreateSelectedCharacter(
        out CharacterProfileData profile,
        out string errorMessage)
    {
        return TryCreateCharacter(
            selectedCharacterName,
            out profile,
            out errorMessage
        );
    }
}