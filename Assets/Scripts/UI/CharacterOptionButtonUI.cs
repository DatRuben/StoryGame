using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterOptionButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text optionNameText;
    [SerializeField] private Image optionImage;
    [SerializeField] private GameObject selectedOption;

    public Button Button => button;

    public void SetText(string text)
    {
        if (optionNameText != null)
            optionNameText.text = text;
    }

    public void SetImage(Sprite sprite)
    {
        if (optionImage == null)
            return;

        optionImage.sprite = sprite;
        optionImage.enabled = sprite != null;
    }

    public void SetSelected(bool selected)
    {
        if (selectedOption != null)
            selectedOption.SetActive(selected);
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void Reset()
    {
        button = GetComponent<Button>();
        optionNameText = transform.Find("OptionName")?.GetComponent<TMP_Text>();
        optionImage = transform.Find("OptionImage")?.GetComponent<Image>();
        selectedOption = transform.Find("SelectedOption")?.gameObject;
    }
}