using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterAppearanceCategoryUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField]
    private Button headerButton;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text arrowText;

    [SerializeField]
    private GameObject selectedObject;

    [Header("Options")]
    [SerializeField]
    private GameObject optionsContainer;

    [SerializeField]
    private Transform optionButtonParent;

    private bool hasOptions;

    public Transform OptionButtonParent =>
        optionButtonParent;

    public bool IsExpanded { get; private set; }

    public void Setup(
        string title,
        bool containsOptions,
        UnityAction clicked)
    {
        hasOptions = containsOptions;

        if (titleText != null)
            titleText.text = title;

        if (headerButton != null)
        {
            headerButton.onClick.RemoveAllListeners();

            if (clicked != null)
            {
                headerButton.onClick.AddListener(
                    clicked
                );
            }
        }

        if (arrowText != null)
            arrowText.gameObject.SetActive(hasOptions);

        SetSelected(false);
        SetExpanded(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedObject != null)
            selectedObject.SetActive(selected);
    }

    public void SetExpanded(bool expanded)
    {
        expanded = hasOptions && expanded;
        IsExpanded = expanded;

        if (optionsContainer != null)
            optionsContainer.SetActive(expanded);

        if (arrowText != null && hasOptions)
        {
            arrowText.text =
                expanded ? "v" : ">";
        }
    }

    public void ClearOptions()
    {
        if (optionButtonParent == null)
            return;

        for (int i =
                 optionButtonParent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                optionButtonParent
                    .GetChild(i)
                    .gameObject
            );
        }
    }
}