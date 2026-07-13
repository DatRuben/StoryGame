using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorAppearanceUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Body Scale")]
    [SerializeField] private Slider bodyScaleSlider;

    [Header("Color")]
    [SerializeField] private Slider hueSlider;
    [SerializeField] private Slider saturationSlider;
    [SerializeField] private Slider valueSlider;

    [Header("Output")]
    [SerializeField] private TMP_Text previewText;

    private void OnEnable()
    {
        HookSliders();
        SubscribeToCreator();
        Refresh();
    }

    private void OnDisable()
    {
        UnhookSliders();
        UnsubscribeFromCreator();
    }

    private void HookSliders()
    {
        if (bodyScaleSlider != null)
        {
            bodyScaleSlider.onValueChanged.RemoveListener(
                OnBodyScaleChanged
            );

            bodyScaleSlider.onValueChanged.AddListener(
                OnBodyScaleChanged
            );
        }

        if (hueSlider != null)
        {
            hueSlider.onValueChanged.RemoveListener(
                OnHueChanged
            );

            hueSlider.onValueChanged.AddListener(
                OnHueChanged
            );
        }

        if (saturationSlider != null)
        {
            saturationSlider.onValueChanged.RemoveListener(
                OnSaturationChanged
            );

            saturationSlider.onValueChanged.AddListener(
                OnSaturationChanged
            );
        }

        if (valueSlider != null)
        {
            valueSlider.onValueChanged.RemoveListener(
                OnValueChanged
            );

            valueSlider.onValueChanged.AddListener(
                OnValueChanged
            );
        }
    }

    private void UnhookSliders()
    {
        if (bodyScaleSlider != null)
            bodyScaleSlider.onValueChanged.RemoveListener(OnBodyScaleChanged);

        if (hueSlider != null)
            hueSlider.onValueChanged.RemoveListener(OnHueChanged);

        if (saturationSlider != null)
            saturationSlider.onValueChanged.RemoveListener(OnSaturationChanged);

        if (valueSlider != null)
            valueSlider.onValueChanged.RemoveListener(OnValueChanged);
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

    private void Refresh()
    {
        if (characterCreator == null)
        {
            ShowPreview("CharacterCreator is missing.");
            return;
        }

        CharacterAppearanceData appearance =
            characterCreator.SelectedAppearance;

        SetSliderWithoutNotify(
            bodyScaleSlider,
            appearance.bodyScale
        );

        SetSliderWithoutNotify(
            hueSlider,
            appearance.hue
        );

        SetSliderWithoutNotify(
            saturationSlider,
            appearance.saturation
        );

        SetSliderWithoutNotify(
            valueSlider,
            appearance.value
        );

        ShowPreview(
            $"Body Scale: {appearance.bodyScale:0.00}\n" +
            $"Hue: {appearance.hue:0.00}\n" +
            $"Saturation: {appearance.saturation:0.00}\n" +
            $"Value: {appearance.value:0.00}"
        );
    }

    private void SetSliderWithoutNotify(
        Slider slider,
        float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void OnBodyScaleChanged(float value)
    {
        if (characterCreator != null)
            characterCreator.SetBodyScale(value);
    }

    private void OnHueChanged(float value)
    {
        if (characterCreator != null)
            characterCreator.SetHue(value);
    }

    private void OnSaturationChanged(float value)
    {
        if (characterCreator != null)
            characterCreator.SetSaturation(value);
    }

    private void OnValueChanged(float value)
    {
        if (characterCreator != null)
            characterCreator.SetValue(value);
    }

    private void ShowPreview(
        string message)
    {
        if (previewText != null)
            previewText.text = message;
    }
}