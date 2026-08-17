using System.Text;
using TMPro;
using UnityEngine;

public sealed class InteractionPromptUI :
    MonoBehaviour
{
    [SerializeField]
    private GameObject promptRoot;

    [SerializeField]
    private TMP_Text promptText;

    [Header("World Item Options")]
    [SerializeField]
    private GameObject itemOptionsRoot;

    [SerializeField]
    private TMP_Text itemOptionsText;

    [SerializeField]
    private Color disabledOptionColor =
        Color.gray;

    [SerializeField]
    private PlayerStorageContainerInteract
        interactionSource;

    private readonly StringBuilder
        optionTextBuilder =
            new StringBuilder();

    public void Bind(
        PlayerStorageContainerInteract source)
    {
        interactionSource = source;

        Refresh();
    }

    private void Start()
    {
        ResolveSource();
        Refresh();
    }

    private void Update()
    {
        if (interactionSource == null)
        {
            ResolveSource();
        }

        Refresh();
    }

    private void ResolveSource()
    {
        if (interactionSource != null)
            return;

        interactionSource =
            FindFirstObjectByType<
                PlayerStorageContainerInteract>();
    }

    private void Refresh()
    {
        bool visible =
            interactionSource != null &&
            interactionSource.HasInteraction &&
            !string.IsNullOrWhiteSpace(
                interactionSource
                    .CurrentInteractionText
            );

        if (promptRoot != null &&
            promptRoot.activeSelf != visible)
        {
            promptRoot.SetActive(
                visible
            );
        }

        RefreshItemOptions();

        if (!visible ||
            promptText == null)
        {
            return;
        }

        promptText.text =
            interactionSource
                .CurrentInteractionText;
    }

    private void RefreshItemOptions()
    {
        bool visible =
            interactionSource != null &&
            interactionSource.CurrentWorldItem != null &&
            interactionSource.WorldItemOptions.Count > 0;

        if (itemOptionsRoot != null &&
            itemOptionsRoot.activeSelf != visible)
        {
            itemOptionsRoot.SetActive(
                visible
            );
        }

        if (!visible ||
            itemOptionsText == null)
        {
            return;
        }

        optionTextBuilder.Clear();

        string disabledColor =
            ColorUtility.ToHtmlStringRGB(
                disabledOptionColor
            );

        for (int i = 0;
             i <
             interactionSource
                 .WorldItemOptions.Count;
             i++)
        {
            WorldItemInteractionOption option =
                interactionSource
                    .WorldItemOptions[i];

            bool selected =
                i ==
                interactionSource
                    .SelectedWorldItemOptionIndex;

            if (!option.IsAvailable)
            {
                optionTextBuilder.Append(
                    "<color=#" +
                    disabledColor +
                    ">"
                );
            }

            optionTextBuilder.Append(
                selected
                    ? "> "
                    : "  "
            );

            optionTextBuilder.Append(
                option.Label
            );

            if (!option.IsAvailable &&
                !string.IsNullOrWhiteSpace(
                    option.DisabledReason))
            {
                optionTextBuilder.Append(
                    "  ["
                );

                optionTextBuilder.Append(
                    option.DisabledReason
                );

                optionTextBuilder.Append(
                    "]"
                );
            }

            if (!option.IsAvailable)
            {
                optionTextBuilder.Append(
                    "</color>"
                );
            }

            if (i <
                interactionSource
                    .WorldItemOptions.Count - 1)
            {
                optionTextBuilder.AppendLine();
            }
        }

        itemOptionsText.text =
            optionTextBuilder.ToString();
    }
}