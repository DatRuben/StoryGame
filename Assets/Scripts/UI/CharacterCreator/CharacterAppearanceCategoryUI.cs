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

    public Transform OptionButtonParent =>
        optionButtonParent;

    public bool IsExpanded { get; private set; }

    public void Setup(
        string title,
        UnityAction clicked)
    {
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

        SetExpanded(false);
    }

    public void SetExpanded(bool expanded)
    {
        IsExpanded = expanded;

        if (optionsContainer != null)
            optionsContainer.SetActive(expanded);

        if (selectedObject != null)
            selectedObject.SetActive(expanded);

        if (arrowText != null)
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