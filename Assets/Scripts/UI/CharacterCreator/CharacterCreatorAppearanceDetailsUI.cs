using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterCreatorAppearanceDetailsUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Category Panels")]
    [SerializeField] private GameObject bodyDetails;
    [SerializeField] private GameObject headDetails;
    [SerializeField] private GameObject hairDetails;
    [SerializeField] private GameObject eyesDetails;
    [SerializeField] private GameObject skinDetails;

    [Header("Body")]
    [SerializeField] private Slider bodyScaleSlider;

    [Header("Head")]
    [SerializeField]
    private CharacterOptionButtonUI[] headTypeButtons;

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

    private CharacterAppearanceCategory selectedCategory =
        CharacterAppearanceCategory.Body;

    private void OnEnable()
    {
        HookUI();
        SubscribeToCreator();

        ShowCategory(selectedCategory);
        Refresh();
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

        HookHeadTypeButtons();
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

        UnhookHeadTypeButtons();
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

    private void HookHeadTypeButtons()
    {
        if (headTypeButtons == null)
            return;

        for (int i = 0; i < headTypeButtons.Length; i++)
        {
            CharacterOptionButtonUI button =
                headTypeButtons[i];

            if (button == null)
                continue;

            int headType = i;

            button.SetText($"Head {i + 1}");
            button.SetInteractable(true);

            if (button.Button == null)
                continue;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                SelectHeadType(headType)
            );
        }
    }

    private void UnhookHeadTypeButtons()
    {
        if (headTypeButtons == null)
            return;

        foreach (CharacterOptionButtonUI button
                 in headTypeButtons)
        {
            if (button != null &&
                button.Button != null)
            {
                button.Button.onClick.RemoveAllListeners();
            }
        }
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

        RefreshHeadTypeButtons(
            appearance.headType
        );
    }

    private void RefreshHeadTypeButtons(
        int selectedHeadType)
    {
        if (headTypeButtons == null)
            return;

        for (int i = 0; i < headTypeButtons.Length; i++)
        {
            if (headTypeButtons[i] != null)
            {
                headTypeButtons[i].SetSelected(
                    i == selectedHeadType
                );
            }
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

    private void SelectHeadType(
        int headType)
    {
        if (characterCreator != null)
            characterCreator.SetHeadType(headType);
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