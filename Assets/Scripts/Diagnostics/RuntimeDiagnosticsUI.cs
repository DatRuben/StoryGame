using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class RuntimeDiagnosticsUI :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private RuntimeDiagnostics diagnostics;

    [SerializeField]
    private CanvasGroup panel;

    [SerializeField]
    private TMP_Text outputText;

    [Header("Display")]

    [SerializeField]
    [Min(0.1f)]
    private float refreshInterval =
        0.5f;

    [SerializeField]
    private bool visibleOnStart =
        false;

    [Header("Status Colors")]

    [SerializeField]
    private Color readyColor =
        new Color32(
            80,
            220,
            100,
            255
        );

    [SerializeField]
    private Color errorColor =
        new Color32(
            235,
            75,
            75,
            255
        );

    [SerializeField]
    private Color warningColor =
        new Color32(
            240,
            190,
            70,
            255
        );

    [SerializeField]
    private Color blockedColor =
        new Color32(
            230,
            145,
            60,
            255
        );

    [SerializeField]
    private Color optionalColor =
        new Color32(
            155,
            155,
            155,
            255
        );

    private PlayerInputRouter inputRouter;

    private bool inputSubscribed;

    private bool isVisible;

    private float nextRefreshTime;

    private readonly StringBuilder builder =
        new StringBuilder();

    private readonly List<string>
        categoryOrder =
            new List<string>();

    public bool IsVisible =>
        isVisible;

    private void Awake()
    {
        if (diagnostics == null)
        {
            diagnostics =
                FindFirstObjectByType<
                    RuntimeDiagnostics>(
                    FindObjectsInactive
                        .Include
                );
        }

        if (panel == null)
        {
            panel =
                GetComponent<
                    CanvasGroup>();
        }

        if (outputText != null)
        {
            outputText.richText =
                true;
        }

        SetVisible(
            visibleOnStart
        );
    }

    private void OnEnable()
    {
        SubscribeInput();
    }

    private void OnDisable()
    {
        UnsubscribeInput();
    }

    private void Start()
    {
        if (isVisible)
        {
            RefreshNow();
        }
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (Time.unscaledTime <
            nextRefreshTime)
        {
            return;
        }

        RefreshNow();
    }

    public void BindInputRouter(
        PlayerInputRouter newInputRouter)
    {
        if (ReferenceEquals(
                inputRouter,
                newInputRouter))
        {
            return;
        }

        UnsubscribeInput();

        inputRouter =
            newInputRouter;

        if (isActiveAndEnabled)
        {
            SubscribeInput();
        }
    }

    public void Show()
    {
        SetVisible(true);
        RefreshNow();
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void Toggle()
    {
        if (isVisible)
        {
            Hide();
            return;
        }

        Show();
    }

    public void RefreshNow()
    {
        nextRefreshTime =
            Time.unscaledTime +
            refreshInterval;

        if (diagnostics == null ||
            outputText == null)
        {
            return;
        }

        diagnostics.Refresh();

        BuildOutput();
    }

    private void SubscribeInput()
    {
        if (inputSubscribed ||
            inputRouter == null)
        {
            return;
        }

        inputRouter
            .ToggleDiagnosticsAction
            .performed +=
                HandleToggleDiagnostics;

        inputSubscribed =
            true;
    }

    private void UnsubscribeInput()
    {
        if (!inputSubscribed ||
            inputRouter == null)
        {
            inputSubscribed =
                false;

            return;
        }

        inputRouter
            .ToggleDiagnosticsAction
            .performed -=
                HandleToggleDiagnostics;

        inputSubscribed =
            false;
    }

    private void HandleToggleDiagnostics(
        InputAction.CallbackContext context)
    {
        Toggle();
    }

    private void BuildOutput()
    {
        builder.Clear();
        categoryOrder.Clear();

        IReadOnlyList<
            RuntimeDiagnosticCheck>
            checks =
                diagnostics.Checks;

        int problemCount = 0;
        int warningCount = 0;
        int blockedCount = 0;

        for (int i = 0;
             i < checks.Count;
             i++)
        {
            RuntimeDiagnosticCheck check =
                checks[i];

            if (!categoryOrder.Contains(
                    check.Category))
            {
                categoryOrder.Add(
                    check.Category
                );
            }

            switch (check.Status)
            {
                case RuntimeDiagnosticStatus.Missing:
                case RuntimeDiagnosticStatus.Misbound:
                case RuntimeDiagnosticStatus.Invalid:
                    problemCount++;
                    break;

                case RuntimeDiagnosticStatus.Warning:
                    warningCount++;
                    break;

                case RuntimeDiagnosticStatus.Blocked:
                    blockedCount++;
                    break;
            }
        }

        builder.AppendLine(
            "<b>Runtime Diagnostics</b>"
        );

        builder.Append(
            "Scene: "
        );

        builder.Append(
            SceneManager
                .GetActiveScene()
                .name
        );

        builder.Append(
            "    Expected: "
        );

        builder.AppendLine(
            diagnostics
                .RequiredSystems
                .ToString()
        );

        builder.Append(
            "Problems: "
        );

        AppendColored(
            problemCount.ToString(),
            problemCount > 0
                ? errorColor
                : readyColor
        );

        builder.Append(
            "    Warnings: "
        );

        AppendColored(
            warningCount.ToString(),
            warningCount > 0
                ? warningColor
                : readyColor
        );

        builder.Append(
            "    Blocked: "
        );

        AppendColored(
            blockedCount.ToString(),
            blockedCount > 0
                ? blockedColor
                : readyColor
        );

        builder.AppendLine();

        for (int categoryIndex = 0;
             categoryIndex <
             categoryOrder.Count;
             categoryIndex++)
        {
            string category =
                categoryOrder[
                    categoryIndex
                ];

            builder.AppendLine();

            builder.Append(
                "<b>"
            );

            builder.Append(
                category
            );

            builder.AppendLine(
                "</b>"
            );

            bool firstCheck =
                true;

            for (int checkIndex = 0;
                 checkIndex <
                 checks.Count;
                 checkIndex++)
            {
                RuntimeDiagnosticCheck check =
                    checks[
                        checkIndex
                    ];

                if (check.Category !=
                    category)
                {
                    continue;
                }

                if (!firstCheck)
                {
                    builder.Append(
                        "   |   "
                    );
                }

                AppendInlineCheck(
                    check
                );

                firstCheck =
                    false;
            }

            builder.AppendLine();

            AppendCategoryProblems(
                checks,
                category
            );
        }

        outputText.text =
            builder.ToString();
    }

    private void AppendInlineCheck(
        RuntimeDiagnosticCheck check)
    {
        Color color =
            GetStatusColor(
                check.Status
            );

        builder.Append(
            "<color=#"
        );

        builder.Append(
            ColorUtility
                .ToHtmlStringRGB(
                    color
                )
        );

        builder.Append(
            ">["
        );

        builder.Append(
            check.Status
                .ToString()
                .ToUpperInvariant()
        );

        builder.Append(
            "] "
        );

        builder.Append(
            check.Label
        );

        builder.Append(
            "</color>"
        );
    }

    private void AppendCategoryProblems(
        IReadOnlyList<
            RuntimeDiagnosticCheck>
            checks,
        string category)
    {
        for (int i = 0;
             i < checks.Count;
             i++)
        {
            RuntimeDiagnosticCheck check =
                checks[i];

            if (check.Category !=
                category)
            {
                continue;
            }

            if (check.Status ==
                    RuntimeDiagnosticStatus.Ready ||
                check.Status ==
                    RuntimeDiagnosticStatus.Optional ||
                string.IsNullOrWhiteSpace(
                    check.Details))
            {
                continue;
            }

            builder.Append(
                "    "
            );

            AppendColored(
                check.Label + ": ",
                GetStatusColor(
                    check.Status
                )
            );

            builder.AppendLine(
                check.Details
            );
        }
    }

    private Color GetStatusColor(
        RuntimeDiagnosticStatus status)
    {
        switch (status)
        {
            case RuntimeDiagnosticStatus.Ready:
                return readyColor;

            case RuntimeDiagnosticStatus.Missing:
            case RuntimeDiagnosticStatus.Misbound:
            case RuntimeDiagnosticStatus.Invalid:
                return errorColor;

            case RuntimeDiagnosticStatus.Warning:
                return warningColor;

            case RuntimeDiagnosticStatus.Blocked:
                return blockedColor;

            case RuntimeDiagnosticStatus.Optional:
                return optionalColor;

            default:
                return optionalColor;
        }
    }

    private void AppendColored(
        string text,
        Color color)
    {
        builder.Append(
            "<color=#"
        );

        builder.Append(
            ColorUtility
                .ToHtmlStringRGB(
                    color
                )
        );

        builder.Append(
            ">"
        );

        builder.Append(
            text
        );

        builder.Append(
            "</color>"
        );
    }

    private void SetVisible(
        bool visible)
    {
        isVisible =
            visible;

        if (panel == null)
            return;

        panel.alpha =
            visible
                ? 1f
                : 0f;

        panel.interactable =
            false;

        panel.blocksRaycasts =
            false;
    }
}