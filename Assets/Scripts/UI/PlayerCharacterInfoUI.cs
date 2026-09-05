using TMPro;
using UnityEngine;

public sealed class PlayerCharacterInfoUI :
    MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI characterNameText;

    [SerializeField]
    private TextMeshProUGUI attributesText;

    [SerializeField]
    private TextMeshProUGUI characterDetailsText;

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

            if (characterDetailsText != null)
                characterDetailsText.text = "";

            return;
        }

        if (characterNameText != null)
        {
            characterNameText.text =
                characterProfile.ProfileData.characterName;
        }

        if (characterDetailsText != null)
        {
            CharacterProfileData profile =
                characterProfile.ProfileData;

            string raceName =
                characterProfile.RaceDefinition != null
                    ? characterProfile.RaceDefinition.displayName
                    : profile.raceId;

            string subraceName =
                characterProfile.SubraceDefinition != null
                    ? characterProfile.SubraceDefinition.displayName
                    : profile.subraceId;

            string backgroundName =
                characterProfile.BackgroundDefinition != null
                    ? characterProfile.BackgroundDefinition.displayName
                    : profile.backgroundId;

            string lineageText = "None";

            LineageSelection[] lineages =
                characterProfile.LineageSelections;

            if (lineages != null &&
                lineages.Length > 0)
            {
                lineageText = "";

                for (int i = 0;
                     i < lineages.Length;
                     i++)
                {
                    LineageSelection lineage =
                        lineages[i];

                    if (lineage == null ||
                        string.IsNullOrWhiteSpace(
                            lineage.DisplayName))
                    {
                        continue;
                    }

                    if (lineageText.Length > 0)
                    {
                        lineageText += ", ";
                    }

                    lineageText += lineage.DisplayName;
                }

                if (lineageText.Length == 0)
                {
                    lineageText = "None";
                }
            }

            characterDetailsText.text =
                $"Details\n" +
                $"Level: {profile.level}\n" +
                $"Gender: {profile.gender}\n" +
                $"Race: {raceName}\n" +
                $"Subrace: {subraceName}\n" +
                $"Lineage: {lineageText}\n" +
                $"Background: {backgroundName}";
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
            "Attributes\n" +
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