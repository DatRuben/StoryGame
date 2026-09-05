using TMPro;
using UnityEngine;

public sealed class PlayerCharacterDetailsUI :
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

        CharacterAttributes attributes =
            characterProfile.EffectiveAttributes;

        if (attributesText == null ||
            attributes == null)
        {
            return;
        }

        attributesText.text =
            $"Strength: {attributes.strength}\n" +
            $"Dexterity: {attributes.dexterity}\n" +
            $"Agility: {attributes.agility}\n" +
            $"Vitality: {attributes.vitality}\n" +
            $"Endurance: {attributes.endurance}\n" +
            $"Intelligence: {attributes.intelligence}\n" +
            $"Willpower: {attributes.willpower}\n" +
            $"Spirit: {attributes.spirit}\n" +
            $"Perception: {attributes.perception}";
    }
}