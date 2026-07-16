using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorPanelNavigationUI : MonoBehaviour
{
    [Header("Top Navigation Buttons")]
    [SerializeField] private Button raceButton;
    [SerializeField] private Button appearanceButton;
    [SerializeField] private Button traitsButton;
    [SerializeField] private Button finalizeButton;

    [Header("Race Panels")]
    [SerializeField] private GameObject raceLeftPanel;
    [SerializeField] private GameObject raceRightPanel;

    [Header("Appearance Panels")]
    [SerializeField] private GameObject appearanceLeftPanel;
    [SerializeField] private GameObject appearanceRightPanel;

    [Header("Background / Traits Panels")]
    [SerializeField] private GameObject traitsLeftPanel;
    [SerializeField] private GameObject traitsRightPanel;

    [Header("Finalize Panels")]
    [SerializeField] private GameObject finalizeLeftPanel;
    [SerializeField] private GameObject finalizeRightPanel;

    [Header("Menu Navigation")]
    [SerializeField] private Menus menus;

    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private CharacterCreator characterCreator;

    private CreatorPanel currentPanel;

    private enum CreatorPanel
    {
        Race,
        Appearance,
        Traits,
        Finalize
    }

    public void ShowPreviousPanel()
    {
        switch (currentPanel)
        {
            case CreatorPanel.Race:
                if (characterCreator != null)
                    characterCreator.ResetCreator();

                if (menus != null)
                    menus.ShowCharacterSelect();
                break;

            case CreatorPanel.Appearance:
                ShowRacePanels();
                break;

            case CreatorPanel.Traits:
                ShowAppearancePanels();
                break;

            case CreatorPanel.Finalize:
                ShowTraitsPanels();
                break;
        }
    }

    public void ShowNextPanel()
    {
        switch (currentPanel)
        {
            case CreatorPanel.Race:
                ShowAppearancePanels();
                break;

            case CreatorPanel.Appearance:
                ShowTraitsPanels();
                break;

            case CreatorPanel.Traits:
                ShowFinalizePanels();
                break;

            case CreatorPanel.Finalize:
                // Do nothing for now.
                // Later this can create/finalize the character.
                break;
        }
    }

    private void RefreshFlowButtons()
    {
        if (backButton != null)
            backButton.interactable = true;

        if (nextButton != null)
            nextButton.interactable = currentPanel != CreatorPanel.Finalize;
    }

    private void OnEnable()
    {
        HookButtons();
        ShowRacePanels();
    }

    private void OnDisable()
    {
        UnhookButtons();
    }

    private void HookButtons()
    {
        AddButtonListener(raceButton, ShowRacePanels);
        AddButtonListener(appearanceButton, ShowAppearancePanels);
        AddButtonListener(traitsButton, ShowTraitsPanels);
        AddButtonListener(finalizeButton, ShowFinalizePanels);
        AddButtonListener(backButton, ShowPreviousPanel);
        AddButtonListener(nextButton, ShowNextPanel);
    }

    private void UnhookButtons()
    {
        RemoveButtonListener(raceButton, ShowRacePanels);
        RemoveButtonListener(appearanceButton, ShowAppearancePanels);
        RemoveButtonListener(traitsButton, ShowTraitsPanels);
        RemoveButtonListener(finalizeButton, ShowFinalizePanels);
        RemoveButtonListener(backButton, ShowPreviousPanel);
        RemoveButtonListener(nextButton, ShowNextPanel);
    }

    public void ShowRacePanels()
    {
        currentPanel = CreatorPanel.Race;
        ShowOnly(raceLeftPanel, raceRightPanel);
        SetSelectedButton(raceButton);
        RefreshFlowButtons();
    }

    public void ShowAppearancePanels()
    {
        currentPanel = CreatorPanel.Appearance;
        ShowOnly(appearanceLeftPanel, appearanceRightPanel);
        SetSelectedButton(appearanceButton);
        RefreshFlowButtons();
    }

    public void ShowTraitsPanels()
    {
        currentPanel = CreatorPanel.Traits;
        ShowOnly(traitsLeftPanel, traitsRightPanel);
        SetSelectedButton(traitsButton);
        RefreshFlowButtons();
    }

    public void ShowFinalizePanels()
    {
        currentPanel = CreatorPanel.Finalize;
        ShowOnly(finalizeLeftPanel, finalizeRightPanel);
        SetSelectedButton(finalizeButton);
        RefreshFlowButtons();
    }

    private void ShowOnly(
        GameObject leftPanelToShow,
        GameObject rightPanelToShow)
    {
        SetPanelActive(raceLeftPanel, raceLeftPanel == leftPanelToShow);
        SetPanelActive(raceRightPanel, raceRightPanel == rightPanelToShow);

        SetPanelActive(appearanceLeftPanel, appearanceLeftPanel == leftPanelToShow);
        SetPanelActive(appearanceRightPanel, appearanceRightPanel == rightPanelToShow);

        SetPanelActive(traitsLeftPanel, traitsLeftPanel == leftPanelToShow);
        SetPanelActive(traitsRightPanel, traitsRightPanel == rightPanelToShow);

        SetPanelActive(finalizeLeftPanel, finalizeLeftPanel == leftPanelToShow);
        SetPanelActive(finalizeRightPanel, finalizeRightPanel == rightPanelToShow);
    }

    private void SetSelectedButton(
        Button selectedButton)
    {
        SetButtonInteractable(raceButton, raceButton != selectedButton);
        SetButtonInteractable(appearanceButton, appearanceButton != selectedButton);
        SetButtonInteractable(traitsButton, traitsButton != selectedButton);
        SetButtonInteractable(finalizeButton, finalizeButton != selectedButton);
    }

    private void AddButtonListener(
        Button button,
        UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void RemoveButtonListener(
        Button button,
        UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
    }

    private void SetButtonInteractable(
        Button button,
        bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void SetPanelActive(
        GameObject panel,
        bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}