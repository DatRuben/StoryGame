using System.Collections.Generic;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{
    [SerializeField] private CharacterDataLibrary characterDataLibrary;

    [Header("Current Selection")]
    [SerializeField] private string selectedRaceId;
    [SerializeField] private string selectedSubraceId;
    [SerializeField] private List<string> selectedLineageIds = new();
    [SerializeField] private string selectedBackgroundId;
    [SerializeField] private List<string> selectedTraitIds = new();
    [SerializeField] private CharacterGender selectedGender = CharacterGender.Male;
    [SerializeField] private string selectedCharacterName = "";
    [SerializeField] private CharacterAppearanceData selectedAppearance = CharacterAppearanceData.CreateDefault();

    public string SelectedRaceId => selectedRaceId;
    public string SelectedSubraceId => selectedSubraceId;
    public IReadOnlyList<string> SelectedLineageIds => selectedLineageIds;
    public CharacterGender SelectedGender => selectedGender;
    public string SelectedCharacterName => selectedCharacterName;
    public event System.Action SelectionChanged;
    public CharacterAppearanceData SelectedAppearance => CharacterAppearanceData.Copy(selectedAppearance);
    public string SelectedBackgroundId => selectedBackgroundId;
    public IReadOnlyList<string> SelectedTraitIds => selectedTraitIds;

    public void SelectGender(
        CharacterGender gender)
    {
        if (selectedGender == gender)
            return;

        selectedGender = gender;
        NotifySelectionChanged();
    }

    public void SetCharacterName(
        string characterName)
    {
        string cleanedName =
            string.IsNullOrWhiteSpace(characterName)
                ? ""
                : characterName.Trim();

        if (selectedCharacterName == cleanedName)
            return;

        selectedCharacterName = cleanedName;
        NotifySelectionChanged();
    }

    public void SetBodyScale(
    float bodyScale)
    {
        selectedAppearance.bodyScale =
            ClampBodyScaleForSelectedRaceSize(bodyScale);

        NotifySelectionChanged();
    }

    public void SetHue(
        float hue)
    {
        selectedAppearance.hue =
            Mathf.Repeat(hue, 1f);

        NotifySelectionChanged();
    }

    public void SetSaturation(
        float saturation)
    {
        selectedAppearance.saturation =
            Mathf.Clamp01(saturation);

        NotifySelectionChanged();
    }

    public void SetValue(
        float value)
    {
        selectedAppearance.value =
            Mathf.Clamp01(value);

        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        if (SelectionChanged != null)
            SelectionChanged.Invoke();
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

        ClampSelectedAppearance();

        NotifySelectionChanged();
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

        ClampSelectedAppearance();

        NotifySelectionChanged();
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
            NotifySelectionChanged();
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

        NotifySelectionChanged();
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

        if (!TryGetResolvedStats(
            out ResolvedCharacterStats resolvedStats,
            out errorMessage))
        {
            return false;
        }

        return CharacterSelection.TryCreateCharacter(
            characterName,
            selectedGender,
            raceDefinition,
            subraceDefinition,
            lineageDefinitions,
            backgroundDefinition,
            traitDefinitions,
            CharacterAppearanceData.Copy(selectedAppearance),
            CharacterAttributes.Copy(resolvedStats.finalAttributes),
            CharacterBaseStats.Copy(resolvedStats.totalBaseStats),
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
        if (selectedLineageIds.Count == 0)
            return;

        selectedLineageIds.Clear();
        NotifySelectionChanged();
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

    private BackgroundDefinition GetSelectedBackgroundDefinition()
    {
        if (characterDataLibrary == null)
            return null;

        if (string.IsNullOrWhiteSpace(selectedBackgroundId))
            return null;

        characterDataLibrary.TryGetBackgroundDefinition(
            selectedBackgroundId,
            out BackgroundDefinition backgroundDefinition
        );

        return backgroundDefinition;
    }

    private List<TraitDefinition> GetSelectedTraitDefinitions()
    {
        if (characterDataLibrary == null)
            return new List<TraitDefinition>();

        return characterDataLibrary.GetTraitDefinitions(
            selectedTraitIds
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

    private void ClampSelectedAppearance()
    {
        if (selectedAppearance == null)
            selectedAppearance = CharacterAppearanceData.CreateDefault();

        selectedAppearance.bodyScale =
            ClampBodyScaleForSelectedRaceSize(
                selectedAppearance.bodyScale
            );

        selectedAppearance.hue =
            Mathf.Repeat(
                selectedAppearance.hue,
                1f
            );

        selectedAppearance.saturation =
            Mathf.Clamp01(
                selectedAppearance.saturation
            );

        selectedAppearance.value =
            Mathf.Clamp01(
                selectedAppearance.value
            );
    }

    private float ClampBodyScaleForSelectedRaceSize(
        float value)
    {
        RaceSize raceSize =
            GetSelectedRaceSize();

        Vector2 range =
            GetBodyScaleRange(raceSize);

        return Mathf.Clamp(
            value,
            range.x,
            range.y
        );
    }

    private RaceSize GetSelectedRaceSize()
    {
        if (TryGetSelectedSubrace(
            out SubraceDefinition subraceDefinition))
        {
            return subraceDefinition.size;
        }

        return RaceSize.Size2;
    }

    private Vector2 GetBodyScaleRange(
        RaceSize raceSize)
    {
        switch (raceSize)
        {
            case RaceSize.Size1:
            case RaceSize.Size1Feral:
                return new Vector2(0.9f, 1.1f);

            case RaceSize.TallerSize2:
                return new Vector2(0.9f, 1.1f);

            case RaceSize.Size3:
            case RaceSize.Size3Feral:
                return new Vector2(0.9f, 1.1f);

            case RaceSize.Dragon:
            case RaceSize.BigDragon:
                return new Vector2(0.9f, 1.1f);

            case RaceSize.Size2:
            case RaceSize.Size2Feral:
            default:
                return new Vector2(0.9f, 1.1f);
        }
    }

    public bool TryGetResolvedStats(
        out ResolvedCharacterStats resolvedStats,
        out string errorMessage)
    {
        resolvedStats = null;
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

        resolvedStats =
                CharacterStatsResolver.ResolveCharacter(
                    raceDefinition,
                    subraceDefinition,
                    GetSelectedLineageDefinitions(),
                    GetSelectedBackgroundDefinition(),
                    GetSelectedTraitDefinitions()
                );

        BackgroundDefinition backgroundDefinition =
            GetSelectedBackgroundDefinition();

        List<TraitDefinition> traitDefinitions =
            GetSelectedTraitDefinitions();

        return true;
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