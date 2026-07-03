using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorRaceUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Base Race Buttons")]
    [SerializeField] private Transform baseRaceButtonParent;
    [SerializeField] private Button baseRaceButtonTemplate;

    [Header("Text")]
    [SerializeField] private TMP_Text baseRaceDescriptionText;

    private readonly List<Button> createdButtons = new();

    private void Start()
    {
        BuildBaseRaceButtons();
    }

    public void BuildBaseRaceButtons()
    {
        ClearButtons();

        if (characterDataLibrary == null)
        {
            ShowDescription("CharacterDataLibrary is missing.");
            return;
        }

        if (baseRaceButtonParent == null)
        {
            ShowDescription("BaseRaceSelection is missing.");
            return;
        }

        if (baseRaceButtonTemplate == null)
        {
            ShowDescription("BaseRaceButton_Template is missing.");
            return;
        }

        baseRaceButtonTemplate.gameObject.SetActive(false);

        List<BaseRace> baseRaces = GetBaseRaces();

        foreach (BaseRace baseRace in baseRaces)
        {
            Button button = Instantiate(
                baseRaceButtonTemplate,
                baseRaceButtonParent
            );

            button.name = $"{baseRace}Button";

            TMP_Text raceNameText =
                button.transform.Find("RaceName")?.GetComponent<TMP_Text>();

            if (raceNameText != null)
                raceNameText.text = FormatBaseRaceName(baseRace);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectBaseRace(baseRace));

            button.gameObject.SetActive(true);

            createdButtons.Add(button);
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
            ShowDescription($"{FormatBaseRaceName(baseRace)} has no profiles.");
            return;
        }

        ShowDescription(profile.description);

        if (characterCreator == null)
            return;

        bool selected = characterCreator.SelectRace(
            profile.profileId,
            out string errorMessage
        );

        if (!selected)
            ShowDescription(errorMessage);
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

    private void ClearButtons()
    {
        for (int i = createdButtons.Count - 1; i >= 0; i--)
        {
            if (createdButtons[i] != null)
                Destroy(createdButtons[i].gameObject);
        }

        createdButtons.Clear();
    }

    private void ShowDescription(string message)
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