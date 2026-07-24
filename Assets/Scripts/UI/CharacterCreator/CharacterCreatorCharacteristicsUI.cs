using System.Text;
using TMPro;
using UnityEngine;

public class CharacterCreatorCharacteristicsUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Identity UI")]
    [SerializeField] private TMP_InputField characterNameInput;
    [SerializeField] private CharacterOptionButtonUI maleGenderButton;
    [SerializeField] private CharacterOptionButtonUI femaleGenderButton;

    [Header("Preview UI")]
    [SerializeField] private TMP_Text attributePreviewText;
    [SerializeField] private TMP_Text derivedStatsPreviewText;

    private void OnEnable()
    {
        HookUI();
        SubscribeToCreator();
        PushCurrentInputToCreator();
        Refresh();
    }

    private void OnDisable()
    {
        UnhookUI();
        UnsubscribeFromCreator();
    }

    private void HookUI()
    {
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.RemoveListener(
                OnCharacterNameChanged
            );

            characterNameInput.onValueChanged.AddListener(
                OnCharacterNameChanged
            );
        }

        HookGenderButton(
            maleGenderButton,
            "Male",
            CharacterGender.Male
        );

        HookGenderButton(
            femaleGenderButton,
            "Female",
            CharacterGender.Female
        );
    }

    private void UnhookUI()
    {
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.RemoveListener(
                OnCharacterNameChanged
            );
        }
    }

    private void HookGenderButton(
        CharacterOptionButtonUI button,
        string label,
        CharacterGender gender)
    {
        if (button == null)
            return;

        button.SetText(label);

        if (button.Button == null)
            return;

        button.Button.onClick.RemoveAllListeners();
        button.Button.onClick.AddListener(() =>
            SelectGender(gender)
        );
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

    private void PushCurrentInputToCreator()
    {
        if (characterCreator == null ||
            characterNameInput == null)
        {
            return;
        }

        characterCreator.SetCharacterName(
            characterNameInput.text
        );
    }

    private void OnCharacterNameChanged(
        string characterName)
    {
        if (characterCreator != null)
            characterCreator.SetCharacterName(characterName);
    }

    private void SelectGender(
        CharacterGender gender)
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectGender(gender);
    }

    private void Refresh()
    {
        RefreshName();
        RefreshGenderButtons();
        RefreshPreview();
    }

    private void RefreshName()
    {
        if (characterNameInput == null ||
            characterCreator == null)
        {
            return;
        }

        if (characterNameInput.text != characterCreator.SelectedCharacterName)
        {
            characterNameInput.SetTextWithoutNotify(
                characterCreator.SelectedCharacterName
            );
        }
    }

    private void RefreshGenderButtons()
    {
        CharacterGender selectedGender =
            characterCreator != null
                ? characterCreator.SelectedGender
                : CharacterGender.Male;

        if (maleGenderButton != null)
        {
            maleGenderButton.SetSelected(
                selectedGender == CharacterGender.Male
            );
        }

        if (femaleGenderButton != null)
        {
            femaleGenderButton.SetSelected(
                selectedGender == CharacterGender.Female
            );
        }
    }

    private void RefreshPreview()
    {
        if (characterCreator == null)
        {
            ShowAttributePreview("CharacterCreator is missing.");
            ShowDerivedStatsPreview("");
            return;
        }

        if (!characterCreator.TryGetResolvedStats(
            out ResolvedCharacterStats resolvedStats,
            out string errorMessage))
        {
            ShowAttributePreview(errorMessage);
            ShowDerivedStatsPreview("");
            return;
        }

        ShowAttributePreview(
            GetAttributePreviewText(
                resolvedStats.attributePreview
            )
        );

        ShowDerivedStatsPreview(
            GetDerivedStatsPreviewText(
                resolvedStats
            )
        );
    }

    private void ShowAttributePreview(
        string message)
    {
        if (attributePreviewText != null)
            attributePreviewText.text = message;
    }

    private void ShowDerivedStatsPreview(
        string message)
    {
        if (derivedStatsPreviewText != null)
            derivedStatsPreviewText.text = message;
    }

    private string GetAttributePreviewText(
        CharacterAttributePreview attributePreview)
    {
        if (attributePreview == null)
            return "";

        CharacterAttributes baseRace =
            attributePreview.baseRaceAttributes;

        CharacterAttributeModifiers subrace =
            attributePreview.SubraceModifiers;

        CharacterAttributeModifiers lineage =
            attributePreview.LineageModifiers;

        CharacterAttributeModifiers postAncestry =
            attributePreview.PostAncestryModifiers;

        CharacterAttributes finalAttributes =
            attributePreview.levelOneAttributes;

        if (baseRace == null ||
            subrace == null ||
            lineage == null ||
            postAncestry == null ||
            finalAttributes == null)
        {
            return "Attribute preview is incomplete.";
        }

        bool showPostAncestry =
            postAncestry.HasAny();

        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine(
            "<b>Attribute Calculation</b>"
        );

        builder.AppendLine(
            $"Ancestry Total: " +
            $"{attributePreview.AncestryTotal}"
        );

        builder.AppendLine(
            $"Level 1 Total: " +
            $"{attributePreview.LevelOneTotal}"
        );

        builder.AppendLine();

        AppendAttributeHeader(
            builder,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Strength",
            baseRace.strength,
            subrace.strength,
            lineage.strength,
            postAncestry.strength,
            finalAttributes.strength,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Dexterity",
            baseRace.dexterity,
            subrace.dexterity,
            lineage.dexterity,
            postAncestry.dexterity,
            finalAttributes.dexterity,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Agility",
            baseRace.agility,
            subrace.agility,
            lineage.agility,
            postAncestry.agility,
            finalAttributes.agility,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Vitality",
            baseRace.vitality,
            subrace.vitality,
            lineage.vitality,
            postAncestry.vitality,
            finalAttributes.vitality,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Endurance",
            baseRace.endurance,
            subrace.endurance,
            lineage.endurance,
            postAncestry.endurance,
            finalAttributes.endurance,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Intelligence",
            baseRace.intelligence,
            subrace.intelligence,
            lineage.intelligence,
            postAncestry.intelligence,
            finalAttributes.intelligence,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Willpower",
            baseRace.willpower,
            subrace.willpower,
            lineage.willpower,
            postAncestry.willpower,
            finalAttributes.willpower,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Spirit",
            baseRace.spirit,
            subrace.spirit,
            lineage.spirit,
            postAncestry.spirit,
            finalAttributes.spirit,
            showPostAncestry
        );

        AppendAttributeRow(
            builder,
            "Perception",
            baseRace.perception,
            subrace.perception,
            lineage.perception,
            postAncestry.perception,
            finalAttributes.perception,
            showPostAncestry
        );

        AppendModifierSourceDetails(
            builder,
            attributePreview
        );

        return builder.ToString();
    }

    private void AppendAttributeHeader(
        StringBuilder builder,
        bool showPostAncestry)
    {
        if (builder == null)
            return;

        if (showPostAncestry)
        {
            builder.AppendLine(
                "<size=75%><b>" +
                "Attribute" +
                "<pos=27%>Base Race" +
                "<pos=42%>Subrace" +
                "<pos=55%>Lineage" +
                "<pos=68%>Background/Traits" +
                "<pos=93%>Final" +
                "</b></size>"
            );

            return;
        }

        builder.AppendLine(
            "<size=80%><b>" +
            "Attribute" +
            "<pos=33%>Base Race" +
            "<pos=52%>Subrace" +
            "<pos=70%>Lineage" +
            "<pos=93%>Final" +
            "</b></size>"
        );
    }

    private void AppendAttributeRow(
        StringBuilder builder,
        string label,
        int baseValue,
        int subraceValue,
        int lineageValue,
        int postAncestryValue,
        int finalValue,
        bool showPostAncestry)
    {
        if (builder == null)
            return;

        builder.Append("<size=90%>");
        builder.Append(label);

        if (showPostAncestry)
        {
            builder.Append(
                $"<pos=27%>{baseValue}"
            );

            builder.Append(
                $"<pos=42%>{GetModifierText(subraceValue)}"
            );

            builder.Append(
                $"<pos=55%>{GetModifierText(lineageValue)}"
            );

            builder.Append(
                $"<pos=68%>{GetModifierText(postAncestryValue)}"
            );

            builder.Append(
                $"<pos=93%>{finalValue}"
            );
        }
        else
        {
            builder.Append(
                $"<pos=33%>{baseValue}"
            );

            builder.Append(
                $"<pos=52%>{GetModifierText(subraceValue)}"
            );

            builder.Append(
                $"<pos=70%>{GetModifierText(lineageValue)}"
            );

            builder.Append(
                $"<pos=93%>{finalValue}"
            );
        }

        builder.AppendLine("</size>");
    }

    private void AppendModifierSourceDetails(
        StringBuilder builder,
        CharacterAttributePreview attributePreview)
    {
        if (builder == null ||
            attributePreview?.modifierSources == null)
        {
            return;
        }

        bool addedHeader = false;

        foreach (AttributeModifierPreview source
                 in attributePreview.modifierSources)
        {
            if (source == null ||
                source.modifiers == null ||
                !source.modifiers.HasAny())
            {
                continue;
            }

            if (!addedHeader)
            {
                builder.AppendLine();
                builder.AppendLine(
                    "<b>Background / Trait Details</b>"
                );

                addedHeader = true;
            }

            string sourceName =
                !string.IsNullOrWhiteSpace(
                    source.displayName)
                    ? source.displayName
                    : source.sourceId;

            builder.AppendLine(
                $"{GetModifierSourceLabel(source.sourceType)}: " +
                $"{sourceName}"
            );

            AppendModifierDetail(
                builder,
                "Strength",
                source.modifiers.strength
            );

            AppendModifierDetail(
                builder,
                "Dexterity",
                source.modifiers.dexterity
            );

            AppendModifierDetail(
                builder,
                "Agility",
                source.modifiers.agility
            );

            AppendModifierDetail(
                builder,
                "Vitality",
                source.modifiers.vitality
            );

            AppendModifierDetail(
                builder,
                "Endurance",
                source.modifiers.endurance
            );

            AppendModifierDetail(
                builder,
                "Intelligence",
                source.modifiers.intelligence
            );

            AppendModifierDetail(
                builder,
                "Willpower",
                source.modifiers.willpower
            );

            AppendModifierDetail(
                builder,
                "Spirit",
                source.modifiers.spirit
            );

            AppendModifierDetail(
                builder,
                "Perception",
                source.modifiers.perception
            );

            builder.AppendLine();
        }
    }

    private void AppendModifierDetail(
        StringBuilder builder,
        string label,
        int value)
    {
        if (builder == null ||
            value == 0)
        {
            return;
        }

        builder.AppendLine(
            $"  {label}: {GetModifierText(value)}"
        );
    }

    private string GetModifierSourceLabel(
        AttributeModifierSourceType sourceType)
    {
        switch (sourceType)
        {
            case AttributeModifierSourceType.Background:
                return "Background";

            case AttributeModifierSourceType.Trait:
                return "Trait";

            case AttributeModifierSourceType.RacialPassive:
                return "Racial Passive";

            default:
                return "Modifier";
        }
    }

    private string GetModifierText(
        int value)
    {
        if (value > 0)
            return $"+{value}";

        if (value < 0)
            return value.ToString();

        return "—";
    }

    private void AppendAttributes(
        StringBuilder builder,
        CharacterAttributes attributes)
    {
        if (builder == null ||
            attributes == null)
        {
            return;
        }

        builder.AppendLine($"Strength: {attributes.strength}");
        builder.AppendLine($"Dexterity: {attributes.dexterity}");
        builder.AppendLine($"Agility: {attributes.agility}");
        builder.AppendLine($"Vitality: {attributes.vitality}");
        builder.AppendLine($"Endurance: {attributes.endurance}");
        builder.AppendLine($"Intelligence: {attributes.intelligence}");
        builder.AppendLine($"Willpower: {attributes.willpower}");
        builder.AppendLine($"Spirit: {attributes.spirit}");
        builder.AppendLine($"Perception: {attributes.perception}");
    }

    private string GetDerivedStatsPreviewText(
        ResolvedCharacterStats resolvedStats)
    {
        if (resolvedStats == null ||
            resolvedStats.baseStats == null ||
            resolvedStats.attributeBonuses == null ||
            resolvedStats.totalBaseStats == null ||
            resolvedStats.finalStats == null)
        {
            return "";
        }

        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine("Base Stat Breakdown");

        AppendBaseStatLine(
            builder,
            "Health",
            resolvedStats.baseStats.health,
            resolvedStats.totalBaseStats.health,
            resolvedStats.attributeBonuses.health
        );

        AppendBaseStatLine(
            builder,
            "Stamina",
            resolvedStats.baseStats.stamina,
            resolvedStats.totalBaseStats.stamina,
            resolvedStats.attributeBonuses.stamina
        );

        AppendBaseStatLine(
            builder,
            "Mana",
            resolvedStats.baseStats.mana,
            resolvedStats.totalBaseStats.mana,
            resolvedStats.attributeBonuses.mana
        );

        AppendBaseStatLine(
            builder,
            "Stagger Resist",
            resolvedStats.baseStats.staggerResist,
            resolvedStats.totalBaseStats.staggerResist,
            resolvedStats.attributeBonuses.staggerResist
        );

        AppendBaseStatLine(
            builder,
            "Carry Weight",
            resolvedStats.baseStats.carryWeight,
            resolvedStats.totalBaseStats.carryWeight,
            resolvedStats.attributeBonuses.carryWeight
        );

        builder.AppendLine();
        builder.AppendLine("Runtime Final Stats");
        builder.AppendLine($"Max Health: {resolvedStats.finalStats.maxHealth:0}");
        builder.AppendLine($"Soul Barrier: {resolvedStats.finalStats.maxSoulBarrier:0}");
        builder.AppendLine($"Max Stamina: {resolvedStats.finalStats.maxStamina:0}");
        builder.AppendLine($"Max Aether: {resolvedStats.finalStats.maxAether:0}");
        builder.AppendLine($"Poise: {resolvedStats.finalStats.poise:0}");
        builder.AppendLine($"Mass: {resolvedStats.finalStats.mass:0}");

        return builder.ToString();
    }

    private void AppendBaseStatLine(
        StringBuilder builder,
        string label,
        int baseValue,
        int finalValue,
        int attributeBonus)
    {
        builder.AppendLine(
            $"{label}: {baseValue} -> {finalValue} ({GetSignedNumber(attributeBonus)})"
        );
    }

    private string GetSignedNumber(
        int value)
    {
        if (value > 0)
            return $"+{value}";

        return value.ToString();
    }
}