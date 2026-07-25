using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterCreatorAppearanceDetailsUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharacterDataLibrary characterDataLibrary;

    [SerializeField]
    private CharacterCreator characterCreator;

    [Header("Option Prefab")]
    [SerializeField]
    private CharacterOptionButtonUI optionButtonPrefab;

    [Header("Category Panels")]
    [SerializeField] private GameObject bodyDetails;
    [SerializeField] private GameObject headDetails;
    [SerializeField] private GameObject hairDetails;
    [SerializeField] private GameObject eyesDetails;
    [SerializeField] private GameObject skinDetails;

    [Header("Body")]
    [SerializeField] private Slider bodyScaleSlider;

    [Header("Head")]
    [SerializeField] private Transform headButtonParent;

    [Header("Skin Color")]
    [SerializeField] private Slider skinHueSlider;
    [SerializeField] private Slider skinSaturationSlider;
    [SerializeField] private Slider skinValueSlider;

    [Header("Hair Color")]
    [SerializeField] private Slider hairHueSlider;
    [SerializeField] private Slider hairSaturationSlider;
    [SerializeField] private Slider hairValueSlider;

    [Header("Eye Color")]
    [SerializeField] private Slider eyeHueSlider;
    [SerializeField] private Slider eyeSaturationSlider;
    [SerializeField] private Slider eyeValueSlider;

    private readonly List<CharacterOptionButtonUI>
        headButtons = new();

    private readonly List<CharacterAppearanceOptionDefinition>
        headOptions = new();

    private CharacterAppearanceCategory selectedCategory =
        CharacterAppearanceCategory.Body;

    private void OnEnable()
    {
        HookUI();
        SubscribeToCreator();

        BuildHeadButtons();
        ShowCategory(selectedCategory);
        Refresh();
    }

    private void OnDisable()
    {
        UnhookUI();
        UnsubscribeFromCreator();
        ClearHeadButtons();
    }

    public void ShowCategory(
        CharacterAppearanceCategory category)
    {
        selectedCategory = category;

        SetActive(
            bodyDetails,
            category == CharacterAppearanceCategory.Body
        );

        SetActive(
            headDetails,
            category == CharacterAppearanceCategory.Head
        );

        SetActive(
            hairDetails,
            category == CharacterAppearanceCategory.Hair
        );

        SetActive(
            eyesDetails,
            category == CharacterAppearanceCategory.Eyes
        );

        SetActive(
            skinDetails,
            category == CharacterAppearanceCategory.Skin
        );

        Refresh();
    }

    private void HookUI()
    {
        HookSlider(
            bodyScaleSlider,
            OnBodyScaleChanged
        );

        HookSlider(
            skinHueSlider,
            OnSkinHueChanged
        );

        HookSlider(
            skinSaturationSlider,
            OnSkinSaturationChanged
        );

        HookSlider(
            skinValueSlider,
            OnSkinValueChanged
        );

        HookSlider(
            hairHueSlider,
            OnHairHueChanged
        );

        HookSlider(
            hairSaturationSlider,
            OnHairSaturationChanged
        );

        HookSlider(
            hairValueSlider,
            OnHairValueChanged
        );

        HookSlider(
            eyeHueSlider,
            OnEyeHueChanged
        );

        HookSlider(
            eyeSaturationSlider,
            OnEyeSaturationChanged
        );

        HookSlider(
            eyeValueSlider,
            OnEyeValueChanged
        );
    }

    private void UnhookUI()
    {
        UnhookSlider(
            bodyScaleSlider,
            OnBodyScaleChanged
        );

        UnhookSlider(
            skinHueSlider,
            OnSkinHueChanged
        );

        UnhookSlider(
            skinSaturationSlider,
            OnSkinSaturationChanged
        );

        UnhookSlider(
            skinValueSlider,
            OnSkinValueChanged
        );

        UnhookSlider(
            hairHueSlider,
            OnHairHueChanged
        );

        UnhookSlider(
            hairSaturationSlider,
            OnHairSaturationChanged
        );

        UnhookSlider(
            hairValueSlider,
            OnHairValueChanged
        );

        UnhookSlider(
            eyeHueSlider,
            OnEyeHueChanged
        );

        UnhookSlider(
            eyeSaturationSlider,
            OnEyeSaturationChanged
        );

        UnhookSlider(
            eyeValueSlider,
            OnEyeValueChanged
        );
    }

    private void HookSlider(
        Slider slider,
        UnityAction<float> action)
    {
        if (slider == null)
            return;

        slider.onValueChanged.RemoveListener(action);
        slider.onValueChanged.AddListener(action);
    }

    private void UnhookSlider(
        Slider slider,
        UnityAction<float> action)
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(action);
    }

    private void SubscribeToCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= Refresh;
        characterCreator.SelectionChanged += Refresh;
    }

    private void UnsubscribeFromCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= Refresh;
    }

    private void BuildHeadButtons()
    {
        ClearHeadButtons();

        if (characterDataLibrary == null ||
            optionButtonPrefab == null ||
            headButtonParent == null)
        {
            return;
        }

        foreach (CharacterAppearanceOptionDefinition option
                 in characterDataLibrary.AppearanceOptionDefinitions)
        {
            if (option == null ||
                option.category !=
                    CharacterAppearanceOptionCategory.Head)
            {
                continue;
            }

            CharacterOptionButtonUI button =
                Instantiate(
                    optionButtonPrefab,
                    headButtonParent
                );

            button.name =
                $"{option.optionId}HeadOptionButton";

            button.SetText(option.displayName);
            button.SetImage(option.optionImage);
            button.SetSelected(false);

            CharacterAppearanceOptionDefinition
                capturedOption = option;

            if (button.Button != null)
            {
                button.Button.onClick.RemoveAllListeners();

                button.Button.onClick.AddListener(() =>
                    SelectHeadOption(capturedOption)
                );
            }

            headButtons.Add(button);
            headOptions.Add(option);
        }
    }

    private void ClearHeadButtons()
    {
        foreach (CharacterOptionButtonUI button
                 in headButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        headButtons.Clear();
        headOptions.Clear();
    }

    private void SelectHeadOption(
        CharacterAppearanceOptionDefinition option)
    {
        if (characterCreator == null ||
            option == null)
        {
            return;
        }

        if (!characterCreator.SelectHeadOption(
            option.optionId,
            out string errorMessage))
        {
            Debug.LogWarning(
                errorMessage,
                this
            );
        }
    }

    private void Refresh()
    {
        if (characterCreator == null)
            return;

        CharacterAppearanceData appearance =
            characterCreator.SelectedAppearance;

        if (appearance == null)
            return;

        SetSlider(
            bodyScaleSlider,
            appearance.bodyScale
        );

        SetSlider(
            skinHueSlider,
            appearance.hue
        );

        SetSlider(
            skinSaturationSlider,
            appearance.saturation
        );

        SetSlider(
            skinValueSlider,
            appearance.value
        );

        SetSlider(
            hairHueSlider,
            appearance.hairHue
        );

        SetSlider(
            hairSaturationSlider,
            appearance.hairSaturation
        );

        SetSlider(
            hairValueSlider,
            appearance.hairValue
        );

        SetSlider(
            eyeHueSlider,
            appearance.eyeHue
        );

        SetSlider(
            eyeSaturationSlider,
            appearance.eyeSaturation
        );

        SetSlider(
            eyeValueSlider,
            appearance.eyeValue
        );

        RefreshHeadButtons(
            appearance.headOptionId
        );
    }

    private void RefreshHeadButtons(
        string selectedOptionId)
    {
        for (int i = 0;
             i < headButtons.Count;
             i++)
        {
            CharacterOptionButtonUI button =
                headButtons[i];

            CharacterAppearanceOptionDefinition option =
                i < headOptions.Count
                    ? headOptions[i]
                    : null;

            if (button == null ||
                option == null)
            {
                continue;
            }

            CharacterAppearanceOptionAvailability availability =
                characterCreator.GetAppearanceOptionAvailability(
                    option
                );

            bool shown =
                availability !=
                CharacterAppearanceOptionAvailability.Hidden;

            button.gameObject.SetActive(shown);

            if (!shown)
                continue;

            bool available =
                availability ==
                CharacterAppearanceOptionAvailability.Available;

            bool selected =
                available &&
                string.Equals(
                    selectedOptionId,
                    option.optionId,
                    System.StringComparison.OrdinalIgnoreCase
                );

            button.SetInteractable(available);
            button.SetSelected(selected);
        }
    }

    private void SetSlider(
        Slider slider,
        float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void SetActive(
        GameObject target,
        bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private void OnBodyScaleChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetBodyScale(value);
    }

    private void OnSkinHueChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetHue(value);
    }

    private void OnSkinSaturationChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetSaturation(value);
    }

    private void OnSkinValueChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetValue(value);
    }

    private void OnHairHueChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetHairHue(value);
    }

    private void OnHairSaturationChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetHairSaturation(value);
    }

    private void OnHairValueChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetHairValue(value);
    }

    private void OnEyeHueChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetEyeHue(value);
    }

    private void OnEyeSaturationChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetEyeSaturation(value);
    }

    private void OnEyeValueChanged(
        float value)
    {
        if (characterCreator != null)
            characterCreator.SetEyeValue(value);
    }
}