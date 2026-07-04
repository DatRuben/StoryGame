using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;

    [Header("Menu")]
    [SerializeField] private Menus menus;

    [Header("Slots")]
    [SerializeField] private CharacterSlotButtonUI[] characterSlots;

    [Header("Start / Continue")]
    [SerializeField] private Button startOrContinueButton;
    [SerializeField] private TMP_Text startOrContinueText;

    [Header("Messages")]
    [SerializeField] private TMP_Text messageText;

    private List<CharacterProfileData> profiles = new();
    private int selectedSlotIndex;

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        profiles = CharacterSelection.GetProfiles();

        selectedSlotIndex = PickDefaultSlot();

        for (int i = 0; i < characterSlots.Length; i++)
        {
            int capturedIndex = i;
            CharacterSlotButtonUI slot = characterSlots[i];

            if (slot == null)
                continue;

            if (slot.SlotButton != null)
            {
                slot.SlotButton.onClick.RemoveAllListeners();
                slot.SlotButton.onClick.AddListener(() => SelectSlot(capturedIndex));
            }

            if (slot.DeleteCharacterButton != null)
                slot.DeleteCharacterButton.onClick.RemoveAllListeners();

            CharacterProfileData profile = GetProfileForSlot(i);

            if (profile != null)
                slot.ShowCharacter(
                    profile,
                    characterDataLibrary,
                    i == selectedSlotIndex,
                    IsStoryComplete(profile)
                );
            else
                slot.ShowEmpty(i == selectedSlotIndex);
        }

        SelectSlot(selectedSlotIndex);
    }

    public void SelectSlot(int slotIndex)
    {
        if (characterSlots == null || characterSlots.Length == 0)
            return;

        selectedSlotIndex = Mathf.Clamp(
            slotIndex,
            0,
            characterSlots.Length - 1
        );

        for (int i = 0; i < characterSlots.Length; i++)
        {
            if (characterSlots[i] != null)
                characterSlots[i].SetSelected(i == selectedSlotIndex);
        }

        CharacterProfileData selectedProfile =
            GetProfileForSlot(selectedSlotIndex);

        if (selectedProfile != null)
            CharacterSelection.SelectProfile(selectedProfile.profileId);

        RefreshStartOrContinueButton();
    }

    public void StartOrContinue()
    {
        CharacterProfileData selectedProfile =
            GetProfileForSlot(selectedSlotIndex);

        if (selectedProfile == null)
        {
            if (menus != null)
                menus.ShowCharacterCreator();

            return;
        }

        CharacterSelection.SelectProfile(selectedProfile.profileId);

        if (menus != null)
            menus.StartGame();
    }

    private int PickDefaultSlot()
    {
        if (characterSlots == null || characterSlots.Length == 0)
            return 0;

        if (CharacterSelection.TryGetSelectedProfile(
            out CharacterProfileData selectedProfile))
        {
            int selectedIndex = profiles.FindIndex(
                profile => profile != null &&
                           profile.profileId == selectedProfile.profileId
            );

            if (selectedIndex >= 0 &&
                selectedIndex < characterSlots.Length &&
                !IsStoryComplete(selectedProfile))
            {
                return selectedIndex;
            }
        }

        for (int i = 0; i < characterSlots.Length; i++)
        {
            CharacterProfileData profile = GetProfileForSlot(i);

            if (profile != null && !IsStoryComplete(profile))
                return i;
        }

        for (int i = 0; i < characterSlots.Length; i++)
        {
            if (GetProfileForSlot(i) == null)
                return i;
        }

        return 0;
    }

    private CharacterProfileData GetProfileForSlot(int slotIndex)
    {
        if (profiles == null)
            return null;

        if (slotIndex < 0 || slotIndex >= profiles.Count)
            return null;

        return profiles[slotIndex];
    }

    private void RefreshStartOrContinueButton()
    {
        CharacterProfileData selectedProfile =
            GetProfileForSlot(selectedSlotIndex);

        bool shouldStart =
            selectedProfile == null ||
            IsStoryComplete(selectedProfile);

        if (startOrContinueText != null)
            startOrContinueText.text = shouldStart ? "Start" : "Continue";

        if (messageText != null)
        {
            messageText.text = shouldStart
                ? "Start a new character."
                : $"Continue {selectedProfile.characterName}.";
        }

        if (startOrContinueButton != null)
        {
            startOrContinueButton.onClick.RemoveAllListeners();
            startOrContinueButton.onClick.AddListener(StartOrContinue);
        }
    }

    private bool IsStoryComplete(CharacterProfileData profile)
    {
        return profile != null && profile.storyCompleted;
    }
}