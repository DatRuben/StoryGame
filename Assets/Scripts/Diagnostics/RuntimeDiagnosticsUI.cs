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
    [SerializeField]
    private RuntimeDiagnostics diagnostics;

    [SerializeField]
    private CanvasGroup panel;

    [SerializeField]
    private TMP_Text outputText;

    [SerializeField]
    [Min(0.1f)]
    private float refreshInterval =
        0.5f;

    [SerializeField]
    private bool visibleOnStart =
        true;

    private bool isVisible;

    private float nextRefreshTime;

    private readonly StringBuilder builder =
        new StringBuilder();

    private readonly List<string>
        categoryOrder =
            new List<string>();

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

        SetVisible(
            visibleOnStart
        );
    }

    private void Start()
    {
        RefreshNow();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.f3Key
                .wasPressedThisFrame)
        {
            SetVisible(
                !isVisible
            );

            if (isVisible)
                RefreshNow();
        }

        if (!isVisible)
            return;

        if (Time.unscaledTime <
            nextRefreshTime)
        {
            return;
        }

        RefreshNow();
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

        builder.Append(
            "<b>Runtime Diagnostics</b>"
        );

        builder.AppendLine();

        builder.Append(
            "Scene: "
        );

        builder.AppendLine(
            SceneManager
                .GetActiveScene()
                .name
        );

        builder.Append(
            "Expected: "
        );

        builder.AppendLine(
            diagnostics
                .RequiredSystems
                .ToString()
        );

        builder.Append(
            "Problems: "
        );

        builder.Append(
            problemCount
        );

        builder.Append(
            "    Warnings: "
        );

        builder.Append(
            warningCount
        );

        builder.Append(
            "    Blocked: "
        );

        builder.AppendLine(
            blockedCount.ToString()
        );

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
            builder.Append("<b>");
            builder.Append(category);
            builder.AppendLine("</b>");

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

                builder.Append(
                    "["
                );

                builder.Append(
                    check.Status
                        .ToString()
                        .ToUpperInvariant()
                );

                builder.Append(
                    "] "
                );

                builder.AppendLine(
                    check.Label
                );

                if (string.IsNullOrWhiteSpace(
                        check.Details))
                {
                    continue;
                }

                builder.Append(
                    "    "
                );

                builder.AppendLine(
                    check.Details
                );
            }
        }

        outputText.text =
            builder.ToString();
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