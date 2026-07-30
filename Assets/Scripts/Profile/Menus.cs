using TMPro;
using UnityEngine;

public class Menus : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject characterCreatorPanel;
    [SerializeField] private GameObject gameplayHudPanel;
    [SerializeField] private GameObject characterCreatorStage;
    [SerializeField] private CharacterSelectUI characterSelectUI;

    [Header("Character Creator")]
    [SerializeField] private CharacterCreator characterCreator;
    [SerializeField] private GameObject creatorPlayer;

    [Header("Gameplay")]
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private CharacterRuntimeBinder characterRuntimeBinder;

    [Header("Output")]
    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        ShowStartMenu();
    }

    public void ShowStartMenu()
    {
        SetPanel(startMenuPanel, true);
        SetPanel(characterSelectPanel, false);
        SetPanel(characterCreatorPanel, false);
        SetPanel(gameplayHudPanel, false);
        SetPanel(characterCreatorStage, false);

        ShowMessage("");
    }

    public void ShowCharacterSelect()
    {
        SetPanel(startMenuPanel, false);
        SetPanel(characterSelectPanel, true);
        SetPanel(characterCreatorPanel, false);
        SetPanel(gameplayHudPanel, false);
        SetPanel(characterCreatorStage, false);

        if (characterSelectUI != null)
            characterSelectUI.Refresh();
    }

    public void ShowCharacterCreator()
    {
        if (characterCreator != null)
            characterCreator.ResetCreator();

        SetPanel(startMenuPanel, false);
        SetPanel(characterSelectPanel, false);
        SetPanel(characterCreatorStage, true);
        SetPanel(characterCreatorPanel, true);
        SetPanel(gameplayHudPanel, false);

        ShowMessage("");
    }

    public void StartGame()
    {
        if (!CharacterSelection.TryGetSelectedProfile(
            out CharacterProfileData profile))
        {
            ShowMessage("Select a character first.");
            ShowCharacterSelect();
            return;
        }

        if (playerSpawner == null)
        {
            ShowMessage("PlayerSpawner is missing.");
            return;
        }

        bool spawned =
            creatorPlayer != null
                ? playerSpawner.UsePlayer(
                    creatorPlayer,
                    profile
                )
                : playerSpawner.SpawnSelectedCharacter();

        if (!spawned)
        {
            ShowMessage(
                "Could not use the selected character."
            );

            ShowCharacterSelect();
            return;
        }

        SetPanel(startMenuPanel, false);
        SetPanel(characterSelectPanel, false);
        SetPanel(characterCreatorPanel, false);
        SetPanel(characterCreatorStage, false);
        SetPanel(gameplayHudPanel, true);

        if (characterRuntimeBinder != null)
        {
            characterRuntimeBinder.Bind(
                playerSpawner.SpawnedPlayer
            );
        }

        ShowMessage("");
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SetPanel(
        GameObject panel,
        bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}