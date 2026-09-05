using TMPro;
using UnityEngine;

public sealed class PlayerCharacterInfoUI :
    MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI characterNameText;

    [SerializeField]
    private TextMeshProUGUI attributesText;

    private PlayerCharacterProfile characterProfile;

    public void BindPlayer(
        PlayerCharacterProfile newCharacterProfile)
    {
        if (characterProfile != null)
        {
            characterProfile.AttributesChanged -=
                Refresh;
        }

        characterProfile =
            newCharacterProfile;

        if (characterProfile != null)
        {
            characterProfile.AttributesChanged +=
                Refresh;
        }

        Refresh();
    }

    private void OnEnable()
    {
        if (characterProfile == null)
            return;

        characterProfile.AttributesChanged -=
            Refresh;

        characterProfile.AttributesChanged +=
            Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (characterProfile != null)
        {
            characterProfile.AttributesChanged -=
                Refresh;
        }
    }

    private void Refresh()
    {
        if (characterProfile == null ||
            characterProfile.ProfileData == null)
        {
            if (characterNameText != null)
                characterNameText.text = "";

            if (attributesText != null)
                attributesText.text = "";

            return;
        }

        if (characterNameText != null)
        {
            characterNameText.text =
                characterProfile.ProfileData.characterName;
        }

        CharacterAttributes permanent =
            characterProfile.PermanentAttributes;

        CharacterAttributes effective =
            characterProfile.EffectiveAttributes;

        if (attributesText == null ||
            permanent == null ||
            effective == null)
        {
            return;
        }

        attributesText.text =
            FormatAttribute(
                "Strength",
                permanent.strength,
                effective.strength
            ) + "\n" +
            FormatAttribute(
                "Dexterity",
                permanent.dexterity,
                effective.dexterity
            ) + "\n" +
            FormatAttribute(
                "Agility",
                permanent.agility,
                effective.agility
            ) + "\n" +
            FormatAttribute(
                "Vitality",
                permanent.vitality,
                effective.vitality
            ) + "\n" +
            FormatAttribute(
                "Endurance",
                permanent.endurance,
                effective.endurance
            ) + "\n" +
            FormatAttribute(
                "Intelligence",
                permanent.intelligence,
                effective.intelligence
            ) + "\n" +
            FormatAttribute(
                "Willpower",
                permanent.willpower,
                effective.willpower
            ) + "\n" +
            FormatAttribute(
                "Spirit",
                permanent.spirit,
                effective.spirit
            ) + "\n" +
            FormatAttribute(
                "Perception",
                permanent.perception,
                effective.perception
            );
    }

    private static string FormatAttribute(
        string label,
        int permanentValue,
        int effectiveValue)
    {
        if (permanentValue == effectiveValue)
        {
            return
                $"{label}: {permanentValue}";
        }

        return
            $"{label}: {permanentValue} ({effectiveValue})";
    }
}