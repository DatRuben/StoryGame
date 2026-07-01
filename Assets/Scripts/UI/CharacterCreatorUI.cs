using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorUI : MonoBehaviour
{
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField characterNameInput;

    [Header("Output")]
    [SerializeField] private TMP_Text messageText;

    [Header("Create")]
    [SerializeField] private Button createButton;

    [Header("Race Buttons")]
    [SerializeField] private List<RaceButtonBinding> raceButtons = new();

    [Header("Lineage Buttons")]
    [SerializeField] private List<LineageButtonBinding> lineageButtons = new();

    private void Awake()
    {
        ConnectButtons();
        RefreshCreateButton();
    }

    private void OnEnable()
    {
        RefreshCreateButton();
    }

    private void ConnectButtons()
    {
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.AddListener(
                _ => RefreshCreateButton()
            );
        }

        if (createButton != null)
        {
            createButton.onClick.AddListener(
                CreateCharacter
            );
        }

        foreach (RaceButtonBinding binding in raceButtons)
        {
            if (binding == null ||
                binding.button == null)
            {
                continue;
            }

            string raceProfileId = binding.raceProfileId;

            binding.button.onClick.AddListener(
                () => SelectRace(raceProfileId)
            );
        }

        foreach (LineageButtonBinding binding in lineageButtons)
        {
            if (binding == null ||
                binding.button == null)
            {
                continue;
            }

            string lineageId = binding.lineageId;

            binding.button.onClick.AddListener(
                () => ToggleLineage(lineageId)
            );
        }
    }

    private void SelectRace(string raceProfileId)
    {
        if (characterCreator == null)
        {
            ShowMessage("CharacterCreator is missing.");
            return;
        }

        if (!characterCreator.SelectRace(
            raceProfileId,
            out string errorMessage))
        {
            ShowMessage(errorMessage);
            RefreshCreateButton();
            return;
        }

        ShowMessage($"Selected race: {raceProfileId}");
        RefreshCreateButton();
    }

    private void ToggleLineage(string lineageId)
    {
        if (characterCreator == null)
        {
            ShowMessage("CharacterCreator is missing.");
            return;
        }

        if (!characterCreator.ToggleLineage(
            lineageId,
            out string errorMessage))
        {
            ShowMessage(errorMessage);
            RefreshCreateButton();
            return;
        }

        if (IsLineageSelected(lineageId))
            ShowMessage($"Selected lineage: {lineageId}");
        else
            ShowMessage($"Removed lineage: {lineageId}");

        RefreshCreateButton();
    }

    private void CreateCharacter()
    {
        if (characterCreator == null)
        {
            ShowMessage("CharacterCreator is missing.");
            return;
        }

        string characterName =
            characterNameInput != null
                ? characterNameInput.text
                : "";

        if (!characterCreator.TryCreateCharacter(
            characterName,
            out CharacterProfileData profile,
            out string errorMessage))
        {
            ShowMessage(errorMessage);
            RefreshCreateButton();
            return;
        }

        ShowMessage($"Created character: {profile.characterName}");
        RefreshCreateButton();
    }

    private void RefreshCreateButton()
    {
        if (createButton == null)
            return;

        bool hasName =
            characterNameInput != null &&
            !string.IsNullOrWhiteSpace(characterNameInput.text);

        bool canCreate =
            characterCreator != null &&
            characterCreator.CanCreateCharacter(out _);

        createButton.interactable =
            hasName &&
            canCreate;
    }

    private bool IsLineageSelected(string lineageId)
    {
        if (characterCreator == null)
            return false;

        IReadOnlyList<string> selectedLineageIds =
            characterCreator.SelectedLineageIds;

        for (int i = 0; i < selectedLineageIds.Count; i++)
        {
            if (selectedLineageIds[i] == lineageId)
                return true;
        }

        return false;
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        Debug.Log(message, this);
    }
}

[Serializable]
public class RaceButtonBinding
{
    public Button button;
    public string raceProfileId;
}

[Serializable]
public class LineageButtonBinding
{
    public Button button;
    public string lineageId;
}