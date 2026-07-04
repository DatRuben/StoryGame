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
        bool selected)
    {
        profile = characterProfile;

        if (profile == null)
        {
            ShowEmpty(selected);
            return;
        }

        RaceProfile raceProfile = GetRaceProfile(dataLibrary, profile.raceProfileId);

        SetText(characterNameText, profile.characterName);
        SetText(levelText, $"Level:\n{profile.level}");

        if (raceProfile != null)
        {
            SetText(raceNameText, $"Race:\n{raceProfile.baseRace}");
            SetText(subraceNameText, $"Subrace:\n{raceProfile.subraceName}");
        }
        else
        {
            SetText(raceNameText, $"Race:\n{profile.raceProfileId}");
            SetText(subraceNameText, "Subrace:\nUnknown");
        }

        SetActive(emptyText, false);
        SetActive(characterPortrait, true);
        SetActive(deleteCharacterButton, true);

        SetSelected(selected);
    }

    public void ShowEmpty(bool selected)
    {
        profile = null;

        SetText(characterNameText, "");
        SetText(raceNameText, "");
        SetText(subraceNameText, "");
        SetText(levelText, "");
        SetText(emptyText, "Empty");

        SetActive(emptyText, true);
        SetActive(characterPortrait, false);
        SetActive(deleteCharacterButton, false);

        SetSelected(selected);
    }

    public void SetSelected(bool selected)
    {
        if (selectedCharacter != null)
            selectedCharacter.SetActive(selected);
    }

    private RaceProfile GetRaceProfile(
        CharacterDataLibrary dataLibrary,
        string raceProfileId)
    {
        if (dataLibrary == null)
            return null;

        if (string.IsNullOrWhiteSpace(raceProfileId))
            return null;

        dataLibrary.TryGetRaceProfile(
            raceProfileId,
            out RaceProfile raceProfile
        );

        return raceProfile;
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
    }
}