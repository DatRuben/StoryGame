using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlotButtonUI : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Button slotButton;
    [SerializeField] private Image slotImage;

    [Header("Character Text")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text raceNameText;
    [SerializeField] private TMP_Text subraceNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text emptyText;

    [Header("Visuals")]
    [SerializeField] private Image characterPortrait;
    [SerializeField] private GameObject selectedCharacter;
    [SerializeField] private Image storyProgressImage;
    [SerializeField] private Sprite storyIncompleteSprite;
    [SerializeField] private Sprite storyCompleteSprite;

    [Header("Delete")]
    [SerializeField] private Button deleteCharacterButton;

    private CharacterProfileData profile;

    public Button SlotButton => slotButton;
    public Button DeleteCharacterButton => deleteCharacterButton;
    public CharacterProfileData Profile => profile;
    public bool HasCharacter => profile != null;

    public void ShowCharacter(
        CharacterProfileData characterProfile,
        CharacterDataLibrary dataLibrary,
        bool selected,
        bool storyComplete)
    {
        profile = characterProfile;

        if (profile == null)
        {
            ShowEmpty(selected);
            return;
        }

        RaceDefinition raceDefinition =
            GetRaceDefinition(dataLibrary, profile.raceId);

        SubraceDefinition subraceDefinition =
            GetSubraceDefinition(dataLibrary, profile.subraceId);

        SetText(characterNameText, profile.characterName);
        SetText(levelText, $"Level:\n{profile.level}");

        SetText(
            raceNameText,
            $"Race:\n{GetRaceName(raceDefinition, profile.raceId)}"
        );

        SetText(
            subraceNameText,
            $"Subrace:\n{GetSubraceName(subraceDefinition, profile.subraceId)}"
        );

        SetActive(emptyText, false);
        SetActive(characterPortrait, true);
        SetActive(deleteCharacterButton, true);
        SetStoryProgress(storyComplete);

        SetSelected(selected);
    }

    public void ShowEmpty(bool selected)
    {
        profile = null;

        SetText(characterNameText, "");
        SetText(raceNameText, "");
        SetText(subraceNameText, "");
        SetText(levelText, "");
        SetText(emptyText, "New Character");

        SetActive(emptyText, true);
        SetActive(characterPortrait, false);
        SetActive(deleteCharacterButton, false);
        SetActive(storyProgressImage, false);

        SetSelected(selected);
    }

    public void SetSelected(bool selected)
    {
        if (selectedCharacter != null)
            selectedCharacter.SetActive(selected);
    }

    private void SetStoryProgress(bool storyComplete)
    {
        if (storyProgressImage == null)
            return;

        Sprite sprite = storyComplete
            ? storyCompleteSprite
            : storyIncompleteSprite;

        storyProgressImage.gameObject.SetActive(sprite != null);

        if (sprite != null)
            storyProgressImage.sprite = sprite;
    }

    private RaceDefinition GetRaceDefinition(
        CharacterDataLibrary dataLibrary,
        string raceId)
    {
        if (dataLibrary == null)
            return null;

        if (string.IsNullOrWhiteSpace(raceId))
            return null;

        dataLibrary.TryGetRaceDefinition(
            raceId,
            out RaceDefinition raceDefinition
        );

        return raceDefinition;
    }

    private SubraceDefinition GetSubraceDefinition(
        CharacterDataLibrary dataLibrary,
        string subraceId)
    {
        if (dataLibrary == null)
            return null;

        if (string.IsNullOrWhiteSpace(subraceId))
            return null;

        dataLibrary.TryGetSubraceDefinition(
            subraceId,
            out SubraceDefinition subraceDefinition
        );

        return subraceDefinition;
    }

    private string GetRaceName(
        RaceDefinition raceDefinition,
        string fallback)
    {
        if (raceDefinition == null)
            return string.IsNullOrWhiteSpace(fallback)
                ? "Unknown"
                : fallback;

        if (!string.IsNullOrWhiteSpace(raceDefinition.displayName))
            return raceDefinition.displayName;

        return raceDefinition.raceId;
    }

    private string GetSubraceName(
        SubraceDefinition subraceDefinition,
        string fallback)
    {
        if (subraceDefinition == null)
            return string.IsNullOrWhiteSpace(fallback)
                ? "Unknown"
                : fallback;

        if (!string.IsNullOrWhiteSpace(subraceDefinition.displayName))
            return subraceDefinition.displayName;

        return subraceDefinition.subraceId;
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private void SetActive(Graphic graphic, bool active)
    {
        if (graphic != null)
            graphic.gameObject.SetActive(active);
    }

    private void SetActive(Selectable selectable, bool active)
    {
        if (selectable != null)
            selectable.gameObject.SetActive(active);
    }

    private void Reset()
    {
        slotButton = GetComponent<Button>();
        slotImage = GetComponent<Image>();

        characterNameText = transform.Find("CharacterNameText")?.GetComponent<TMP_Text>();
        raceNameText = transform.Find("RaceNameText")?.GetComponent<TMP_Text>();
        subraceNameText = transform.Find("SubraceNameText")?.GetComponent<TMP_Text>();
        levelText = transform.Find("LevelText")?.GetComponent<TMP_Text>();
        emptyText = transform.Find("EmptyText")?.GetComponent<TMP_Text>();
        characterPortrait = transform.Find("CharacterPortrait")?.GetComponent<Image>();
        selectedCharacter = transform.Find("SelectedCharacter")?.gameObject;
        deleteCharacterButton = transform.Find("DeleteCharacter")?.GetComponent<Button>();
        storyProgressImage = transform.Find("StoryProgress")?.GetComponent<Image>();
    }
}