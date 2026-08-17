using TMPro;
using UnityEngine;

public sealed class InteractionPromptUI :
    MonoBehaviour
{
    [SerializeField]
    private GameObject promptRoot;

    [SerializeField]
    private TMP_Text promptText;

    [SerializeField]
    private PlayerStorageContainerInteract
        interactionSource;

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

        if (!visible ||
            promptText == null)
        {
            return;
        }

        promptText.text =
            interactionSource
                .CurrentInteractionText;
    }
}