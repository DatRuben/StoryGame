using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterCreatorAppearanceDetailsUI :
    MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharacterCreator characterCreator;

    [Header("Display")]
    [SerializeField]
    private TMP_Text categoryTitleText;

    [SerializeField]
    private GameObject bodyScaleControl;

    [SerializeField]
    private GameObject colorControls;

    [Header("Shared Sliders")]
    [SerializeField]
    private Slider bodyScaleSlider;

    [SerializeField]
    private Slider hueSlider;

    [SerializeField]
    private Slider saturationSlider;

    [SerializeField]
    private Slider valueSlider;

    private CharacterAppearanceCategory selectedCategory =
        CharacterAppearanceCategory.Body;

    private void OnEnable()
    {
        HookUI();
        SubscribeToCreator();
        ShowCategory(selectedCategory);
    }

    private void OnDisable()
    {
        UnhookUI();
        UnsubscribeFromCreator();
    }

    public void ShowCategory(
        CharacterAppearanceCategory category)
    {
        selectedCategory = category;

        RefreshControls();
        Refresh();
    }

    // Temporary compatibility with the current left-side
    // controller. Both methods now use the same details panel.
    public void ShowOptionCategory(
        CharacterAppearanceCategory category)
    {
        ShowCategory(category);
    }

    private void HookUI()
    {
        HookSlider(
            bodyScaleSlider,
            OnBodyScaleChanged
        );

        HookSlider(
            hueSlider,
            OnHueChanged
        );

        HookSlider(
            saturationSlider,
            OnSaturationChanged
        );

        HookSlider(
            valueSlider,
            OnValueChanged
        );
    }

    private void UnhookUI()
    {
        UnhookSlider(
            bodyScaleSlider,
            OnBodyScaleChanged
        );

        UnhookSlider(
            hueSlider,
            OnHueChanged
        );

        UnhookSlider(
            saturationSlider,
            OnSaturationChanged
        );

        UnhookSlider(
            valueSlider,
            OnValueChanged
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

    private void RefreshControls()
    {
        if (categoryTitleText != null)
        {
            categoryTitleText.text =
                GetCategoryLabel(selectedCategory);
        }

        SetActive(
            bodyScaleControl,
            selectedCategory ==
                CharacterAppearanceCategory.Body
        );

        SetActive(
            colorControls,
            HasColorControls(selectedCategory)
        );
    }

    private bool HasColorControls(
        CharacterAppearanceCategory category)
    {
        return category ==
                   CharacterAppearanceCategory.Body ||
               category ==
                   CharacterAppearanceCategory.Hair ||
               category ==
                   CharacterAppearanceCategory.Eyes;
    }

    private void Refresh()
    {
        if (characterCreator == null)
            return;

        CharacterAppearanceData appearance =
            characterCreator.SelectedAppearance;

        if (appearance == null)
            return;

        if (selectedCategory ==
            CharacterAppearanceCategory.Body)
        {
            SetSlider(
                bodyScaleSlider,
                appearance.bodyScale
            );
        }

        switch (selectedCategory)
        {
            case CharacterAppearanceCategory.Body:
                SetColorSliders(
                    appearance.hue,
                    appearance.saturation,
                    appearance.value
                );
                break;

            case CharacterAppearanceCategory.Hair:
                SetColorSliders(
                    appearance.hairHue,
                    appearance.hairSaturation,
                    appearance.hairValue
                );
                break;

            case CharacterAppearanceCategory.Eyes:
                SetColorSliders(
                    appearance.eyeHue,
                    appearance.eyeSaturation,
                    appearance.eyeValue
                );
                break;
        }
    }

    private void SetColorSliders(
        float hue,
        float saturation,
        float value)
    {
        SetSlider(hueSlider, hue);
        SetSlider(saturationSlider, saturation);
        SetSlider(valueSlider, value);
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

    private string GetCategoryLabel(
        CharacterAppearanceCategory category)
    {
        if (category ==
            CharacterAppearanceCategory.BodyPattern)
        {
            return "Body Pattern";
        }

        return category.ToString();
    }

    private void OnBodyScaleChanged(
        float value)
    {
        if (characterCreator == null ||
            selectedCategory !=
                CharacterAppearanceCategory.Body)
        {
            return;
        }

        characterCreator.SetBodyScale(value);
    }

    private void OnHueChanged(
        float value)
    {
        if (characterCreator == null)
            return;

        switch (selectedCategory)
        {
            case CharacterAppearanceCategory.Body:
                characterCreator.SetHue(value);
                break;

            case CharacterAppearanceCategory.Hair:
                characterCreator.SetHairHue(value);
                break;

            case CharacterAppearanceCategory.Eyes:
                characterCreator.SetEyeHue(value);
                break;
        }
    }

    private void OnSaturationChanged(
        float value)
    {
        if (characterCreator == null)
            return;

        switch (selectedCategory)
        {
            case CharacterAppearanceCategory.Body:
                characterCreator.SetSaturation(value);
                break;

            case CharacterAppearanceCategory.Hair:
                characterCreator.SetHairSaturation(value);
                break;

            case CharacterAppearanceCategory.Eyes:
                characterCreator.SetEyeSaturation(value);
                break;
        }
    }

    private void OnValueChanged(
        float value)
    {
        if (characterCreator == null)
            return;

        switch (selectedCategory)
        {
            case CharacterAppearanceCategory.Body:
                characterCreator.SetValue(value);
                break;

            case CharacterAppearanceCategory.Hair:
                characterCreator.SetHairValue(value);
                break;

            case CharacterAppearanceCategory.Eyes:
                characterCreator.SetEyeValue(value);
                break;
        }
    }
}