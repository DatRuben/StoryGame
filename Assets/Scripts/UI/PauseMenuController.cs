using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PauseMenuController :
    MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    private CanvasGroup pausePanel;

    [SerializeField]
    private RuntimeDiagnosticsUI
        runtimeDiagnosticsUI;

    [SerializeField]
    private CursorController cursorController;

    private PlayerInputActions inputActions;

    private PlayerGameplayState gameplayState;

    private float previousTimeScale =
        1f;

    private const PlayerGameplayCapability
        PauseRestrictions =
            PlayerGameplayCapability.Movement |
            PlayerGameplayCapability.Combat |
            PlayerGameplayCapability.Inventory |
            PlayerGameplayCapability.WorldInteraction |
            PlayerGameplayCapability.ItemHandling;

    public bool IsPaused
    {
        get;
        private set;
    }

    private void Awake()
    {
        inputActions =
            new PlayerInputActions();

        SetVisible(false);
    }

    private void OnEnable()
    {
        inputActions.Player.Menu.performed +=
            HandlePause;

        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Menu.performed -=
                HandlePause;

            inputActions.Player.Disable();
        }

        if (IsPaused)
        {
            Resume();
        }
    }

    private void OnDestroy()
    {
        if (inputActions == null)
            return;

        inputActions.Dispose();
        inputActions = null;
    }

    public void BindPlayerGameplayState(
        PlayerGameplayState newGameplayState)
    {
        if (ReferenceEquals(
                gameplayState,
                newGameplayState))
        {
            return;
        }

        if (IsPaused &&
            gameplayState != null)
        {
            gameplayState.ClearRestriction(
                this
            );
        }

        gameplayState =
            newGameplayState;

        if (IsPaused &&
            gameplayState != null)
        {
            gameplayState.SetRestriction(
                this,
                PauseRestrictions
            );
        }
    }

    public void Pause()
    {
        if (IsPaused)
            return;

        IsPaused = true;

        if (gameplayState != null)
        {
            gameplayState.SetRestriction(
                this,
                PauseRestrictions
            );
        }

        previousTimeScale =
            Time.timeScale;

        Time.timeScale =
            0f;

        SetVisible(true);

        if (cursorController != null)
        {
            cursorController.SetUnlocked(
                this,
                true
            );
        }
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;

        if (gameplayState != null)
        {
            gameplayState.ClearRestriction(
                this
            );
        }

        Time.timeScale =
            previousTimeScale;

        SetVisible(false);

        if (cursorController != null)
        {
            cursorController.ClearRequest(
                this
            );
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void OpenDiagnostics()
    {
        if (runtimeDiagnosticsUI != null)
        {
            runtimeDiagnosticsUI.Show();
        }
    }

    private void HandlePause(
        InputAction.CallbackContext context)
    {
        TogglePause();
    }

    private void SetVisible(
        bool visible)
    {
        if (pausePanel == null)
            return;

        pausePanel.alpha =
            visible ? 1f : 0f;

        pausePanel.interactable =
            visible;

        pausePanel.blocksRaycasts =
            visible;
    }
}