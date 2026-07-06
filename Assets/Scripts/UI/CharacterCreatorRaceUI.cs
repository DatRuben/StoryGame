using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCreatorRaceUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Option Prefab")]
    [SerializeField] private CharacterOptionButtonUI optionButtonPrefab;

    [Header("Base Race UI")]
    [SerializeField] private Transform baseRaceButtonParent;
    [SerializeField] private TMP_Text baseRaceDescriptionText;

    [Header("Subrace UI")]
    [SerializeField] private Transform subraceButtonParent;
    [SerializeField] private TMP_Text subraceDescriptionText;

    private readonly List<CharacterOptionButtonUI> baseRaceButtons = new();
    private readonly List<CharacterOptionButtonUI> subraceButtons = new();

    private BaseRace selectedBaseRace;
    private string selectedRaceProfileId;

    private static readonly BaseRace[] BaseRaceOrder =
    {
        BaseRace.Human,
        BaseRace.Animali,
        BaseRace.Eastern,
        BaseRace.WesternDragon,
        BaseRace.Drakken,
        BaseRace.Griffin,
        BaseRace.Canispar,
        BaseRace.SoulChip
    };

    private void OnEnable()
    {
        BuildBaseRaceButtons();
    }

    private void OnDisable()
    {
        ClearButtons(baseRaceButtons);
        ClearButtons(subraceButtons);
    }

    public void BuildBaseRaceButtons()
    {
        ClearButtons(baseRaceButtons);
        ClearButtons(subraceButtons);

        if (!HasRequiredReferences())
            return;

        List<BaseRace> baseRaces = GetBaseRaces();

        foreach (BaseRace baseRace in baseRaces)
        {
            CharacterOptionButtonUI button =
                Instantiate(optionButtonPrefab, baseRaceButtonParent);

            button.name = $"{baseRace}OptionButton";
            button.SetText(FormatBaseRaceName(baseRace));
            button.SetSelected(false);

            BaseRace capturedRace = baseRace;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() => SelectBaseRace(capturedRace));

            baseRaceButtons.Add(button);
        }

        if (baseRaces.Count > 0)
        {
            BaseRace startingRace = baseRaces.Contains(BaseRace.Human)
                ? BaseRace.Human
                : baseRaces[0];

            SelectBaseRace(startingRace);
        }
    }

    private bool HasRequiredReferences()
    {
        if (characterDataLibrary == null)
        {
            ShowBaseRaceDescription("CharacterDataLibrary is missing.");
            return false;
        }

        if (characterCreator == null)
        {
            ShowBaseRaceDescription("CharacterCreator is missing.");
            return false;
        }

        if (optionButtonPrefab == null)
        {
            ShowBaseRaceDescription("CharacterOptionButton prefab is missing.");
            return false;
        }

        if (baseRaceButtonParent == null)
        {
            ShowBaseRaceDescription("BaseRaceSelection is missing.");
            return false;
        }

        if (subraceButtonParent == null)
        {
            ShowSubraceDescription("SubraceSelection is missing.");
            return false;
        }

        return true;
    }

    private List<BaseRace> GetBaseRaces()
    {
        List<BaseRace> found = new();

        foreach (BaseRace baseRace in BaseRaceOrder)
        {
            if (HasAnyProfileForBaseRace(baseRace))
                found.Add(baseRace);
        }

        return found;
    }

    private bool HasAnyProfileForBaseRace(BaseRace baseRace)
    {
        foreach (RaceProfile profile in characterDataLibrary.RaceProfiles)
        {
            if (profile == null)
                continue;

            if (profile.baseRace == baseRace)
                return true;
        }

        return false;
    }

    private void SelectBaseRace(BaseRace baseRace)
    {
        selectedBaseRace = baseRace;

        RefreshSelectedBaseRace(baseRace);
        ShowBaseRaceDescription(FormatBaseRaceName(baseRace).Replace("\n", " "));

        BuildSubraceButtons(baseRace);
    }

    private void BuildSubraceButtons(BaseRace baseRace)
    {
        ClearButtons(subraceButtons);

        List<RaceProfile> profiles = GetRaceProfilesForBaseRace(baseRace);

        foreach (RaceProfile profile in profiles)
        {
            CharacterOptionButtonUI button =
                Instantiate(optionButtonPrefab, subraceButtonParent);

            button.name = $"{profile.profileId}OptionButton";
            button.SetText(GetRaceProfileButtonText(profile));
            button.SetSelected(false);

            RaceProfile capturedProfile = profile;
            CharacterOptionButtonUI capturedButton = button;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                SelectRaceProfile(capturedProfile, capturedButton)
            );

            subraceButtons.Add(button);
        }

        if (profiles.Count > 0 && subraceButtons.Count > 0)
            SelectRaceProfile(profiles[0], subraceButtons[0]);
        else
            ShowSubraceDescription("No subraces available.");
    }

    private List<RaceProfile> GetRaceProfilesForBaseRace(BaseRace baseRace)
    {
        List<RaceProfile> profiles = new();

        foreach (RaceProfile profile in characterDataLibrary.RaceProfiles)
        {
            if (profile == null)
                continue;

            if (profile.baseRace == baseRace)
                profiles.Add(profile);
        }

        return profiles;
    }

    private void SelectRaceProfile(
        RaceProfile profile,
        CharacterOptionButtonUI selectedButton)
    {
        if (profile == null)
            return;

        bool selected =
            characterCreator.SelectRace(
                profile.profileId,
                out string errorMessage
            );

        if (!selected)
        {
            ShowSubraceDescription(errorMessage);
            return;
        }

        selectedRaceProfileId = profile.profileId;

        ShowSubraceDescription(profile.description);
        RefreshSelectedSubrace(selectedButton);
    }

    private void RefreshSelectedBaseRace(BaseRace selectedBaseRaceValue)
    {
        foreach (CharacterOptionButtonUI button in baseRaceButtons)
        {
            if (button == null)
                continue;

            button.SetSelected(button.name == $"{selectedBaseRaceValue}OptionButton");
        }
    }

    private void RefreshSelectedSubrace(CharacterOptionButtonUI selectedButton)
    {
        foreach (CharacterOptionButtonUI button in subraceButtons)
        {
            if (button == null)
                continue;

            button.SetSelected(button == selectedButton);
        }
    }

    private void ClearButtons(List<CharacterOptionButtonUI> buttons)
    {
        for (int i = buttons.Count - 1; i >= 0; i--)
        {
            if (buttons[i] != null)
                Destroy(buttons[i].gameObject);
        }

        buttons.Clear();
    }

    private void ShowBaseRaceDescription(string message)
    {
        if (baseRaceDescriptionText != null)
            baseRaceDescriptionText.text = message;
    }

    private void ShowSubraceDescription(string message)
    {
        if (subraceDescriptionText != null)
            subraceDescriptionText.text = message;
    }

    private string GetRaceProfileButtonText(RaceProfile profile)
    {
        if (profile == null)
            return "";

        if (!string.IsNullOrWhiteSpace(profile.subraceName))
            return profile.subraceName;

        if (!string.IsNullOrWhiteSpace(profile.displayName))
            return profile.displayName;

        return profile.profileId;
    }

    private string FormatBaseRaceName(BaseRace baseRace)
    {
        return baseRace switch
        {
            BaseRace.WesternDragon => "Western\nDragon",
            BaseRace.SoulChip => "Soul-Chip",
            _ => SplitWords(baseRace.ToString())
        };
    }

    private string SplitWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string result = text[0].ToString();

        for (int i = 1; i < text.Length; i++)
        {
            char current = text[i];
            char previous = text[i - 1];

            if (char.IsUpper(current) && !char.IsUpper(previous))
                result += " ";

            result += current;
        }

        return result;
    }
}