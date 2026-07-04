using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCreatorRaceUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Base Race UI")]
    [SerializeField] private Transform baseRaceButtonParent;
    [SerializeField] private CharacterOptionButtonUI optionButtonPrefab;
    [SerializeField] private TMP_Text baseRaceDescriptionText;

    private readonly List<CharacterOptionButtonUI> baseRaceButtons = new();

    private void OnEnable()
    {
        BuildBaseRaceButtons();
    }

    private void OnDisable()
    {
        ClearBaseRaceButtons();
    }

    public void BuildBaseRaceButtons()
    {
        ClearBaseRaceButtons();

        if (characterDataLibrary == null)
        {
            ShowBaseRaceDescription("CharacterDataLibrary is missing.");
            return;
        }

        if (characterCreator == null)
        {
            ShowBaseRaceDescription("CharacterCreator is missing.");
            return;
        }

        if (baseRaceButtonParent == null)
        {
            ShowBaseRaceDescription("BaseRaceSelection is missing.");
            return;
        }

        if (optionButtonPrefab == null)
        {
            ShowBaseRaceDescription("CharacterOptionButton prefab is missing.");
            return;
        }

        List<BaseRace> baseRaces = GetBaseRaces();

        foreach (BaseRace baseRace in baseRaces)
        {
            CharacterOptionButtonUI button = Instantiate(
                optionButtonPrefab,
                baseRaceButtonParent
            );

            button.name = $"{baseRace}OptionButton";
            button.SetText(FormatBaseRaceName(baseRace));
            button.SetSelected(false);

            BaseRace capturedRace = baseRace;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() => SelectBaseRace(capturedRace));

            baseRaceButtons.Add(button);
        }

        if (baseRaces.Count > 0)
            SelectBaseRace(baseRaces[0]);
    }

    private List<BaseRace> GetBaseRaces()
    {
        List<BaseRace> baseRaces = new();

        foreach (RaceProfile profile in characterDataLibrary.RaceProfiles)
        {
            if (profile == null)
                continue;

            if (!baseRaces.Contains(profile.baseRace))
                baseRaces.Add(profile.baseRace);
        }

        baseRaces.Sort();

        return baseRaces;
    }

    private void SelectBaseRace(BaseRace baseRace)
    {
        RaceProfile profile = GetFirstProfileForBaseRace(baseRace);

        if (profile == null)
        {
            ShowBaseRaceDescription(
                $"{FormatBaseRaceName(baseRace)} has no race profiles."
            );

            return;
        }

        bool selected = characterCreator.SelectRace(
            profile.profileId,
            out string errorMessage
        );

        if (!selected)
        {
            ShowBaseRaceDescription(errorMessage);
            return;
        }

        ShowBaseRaceDescription(profile.description);
        RefreshSelectedBaseRace(baseRace);
    }

    private RaceProfile GetFirstProfileForBaseRace(BaseRace baseRace)
    {
        foreach (RaceProfile profile in characterDataLibrary.RaceProfiles)
        {
            if (profile == null)
                continue;

            if (profile.baseRace == baseRace)
                return profile;
        }

        return null;
    }

    private void RefreshSelectedBaseRace(BaseRace selectedBaseRace)
    {
        foreach (CharacterOptionButtonUI button in baseRaceButtons)
        {
            if (button == null)
                continue;

            button.SetSelected(
                button.name == $"{selectedBaseRace}OptionButton"
            );
        }
    }

    private void ClearBaseRaceButtons()
    {
        for (int i = baseRaceButtons.Count - 1; i >= 0; i--)
        {
            if (baseRaceButtons[i] != null)
                Destroy(baseRaceButtons[i].gameObject);
        }

        baseRaceButtons.Clear();
    }

    private void ShowBaseRaceDescription(string message)
    {
        if (baseRaceDescriptionText != null)
            baseRaceDescriptionText.text = message;
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