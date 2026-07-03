using TMPro;
using UnityEngine;

public class Menus : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject characterCreatorPanel;
    [SerializeField] private GameObject gameplayHudPanel;

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

        ShowMessage("");
    }

    public void ShowCharacterSelect()
    {
        SetPanel(startMenuPanel, false);
        SetPanel(characterSelectPanel, true);
        SetPanel(characterCreatorPanel, false);
        SetPanel(gameplayHudPanel, false);

        ShowMessage("");
    }

    public void ShowCharacterCreator()
    {
        SetPanel(startMenuPanel, false);
        SetPanel(characterSelectPanel, false);
        SetPanel(characterCreatorPanel, true);
        SetPanel(gameplayHudPanel, false);

        ShowMessage("");
    }

    public void StartGame()
    {
        if (!CharacterSelection.HasSelectedProfile())
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
            playerSpawner.SpawnSelectedCharacter();

        if (!spawned)
        {
            ShowMessage("Could not spawn selected character.");
            ShowCharacterSelect();
            return;
        }

        SetPanel(startMenuPanel, false);
        SetPanel(characterSelectPanel, false);
        SetPanel(characterCreatorPanel, false);
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