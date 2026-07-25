using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterAppearanceData
{
    [Header("Body")]
    [Range(0.8f, 1.2f)]
    public float bodyScale = 1f;

    [Header("Skin Color")]
    [Range(0f, 1f)]
    public float hue = 0f;

    [Range(0f, 1f)]
    public float saturation = 0.75f;

    [Range(0f, 1f)]
    public float value = 0.9f;

    [Header("Hair Color")]
    [Range(0f, 1f)]
    public float hairHue = 0f;

    [Range(0f, 1f)]
    public float hairSaturation = 0.75f;

    [Range(0f, 1f)]
    public float hairValue = 0.9f;

    [Header("Eye Color")]
    [Range(0f, 1f)]
    public float eyeHue = 0f;

    [Range(0f, 1f)]
    public float eyeSaturation = 0.75f;

    [Range(0f, 1f)]
    public float eyeValue = 0.9f;

    [Header("Selected Appearance Options")]
    public string headOptionId = "";
    public string earOptionId = "";
    public string hornOptionId = "";
    public string tailOptionId = "";
    public string hairOptionId = "";
    public string eyeOptionId = "";

    public List<string> markingOptionIds =
        new List<string>();

    public string GetSingleOptionId(
        CharacterAppearanceOptionCategory category)
    {
        switch (category)
        {
            case CharacterAppearanceOptionCategory.Head:
                return headOptionId;

            case CharacterAppearanceOptionCategory.Ears:
                return earOptionId;

            case CharacterAppearanceOptionCategory.Horns:
                return hornOptionId;

            case CharacterAppearanceOptionCategory.Tail:
                return tailOptionId;

            case CharacterAppearanceOptionCategory.Hair:
                return hairOptionId;

            case CharacterAppearanceOptionCategory.Eyes:
                return eyeOptionId;

            default:
                return "";
        }
    }

    public bool SetSingleOptionId(
        CharacterAppearanceOptionCategory category,
        string optionId)
    {
        optionId =
            string.IsNullOrWhiteSpace(optionId)
                ? ""
                : optionId;

        switch (category)
        {
            case CharacterAppearanceOptionCategory.Head:
                headOptionId = optionId;
                return true;

            case CharacterAppearanceOptionCategory.Ears:
                earOptionId = optionId;
                return true;

            case CharacterAppearanceOptionCategory.Horns:
                hornOptionId = optionId;
                return true;

            case CharacterAppearanceOptionCategory.Tail:
                tailOptionId = optionId;
                return true;

            case CharacterAppearanceOptionCategory.Hair:
                hairOptionId = optionId;
                return true;

            case CharacterAppearanceOptionCategory.Eyes:
                eyeOptionId = optionId;
                return true;

            default:
                return false;
        }
    }

    public bool IsMarkingSelected(
        string optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId) ||
            markingOptionIds == null)
        {
            return false;
        }

        for (int i = 0;
             i < markingOptionIds.Count;
             i++)
        {
            if (string.Equals(
                markingOptionIds[i],
                optionId,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool AddMarkingOption(
        string optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId))
            return false;

        if (markingOptionIds == null)
        {
            markingOptionIds =
                new List<string>();
        }

        if (IsMarkingSelected(optionId))
            return false;

        markingOptionIds.Add(optionId);
        return true;
    }

    public bool RemoveMarkingOption(
        string optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId) ||
            markingOptionIds == null)
        {
            return false;
        }

        for (int i = 0;
             i < markingOptionIds.Count;
             i++)
        {
            if (!string.Equals(
                markingOptionIds[i],
                optionId,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            markingOptionIds.RemoveAt(i);
            return true;
        }

        return false;
    }

    public void ClearMarkings()
    {
        markingOptionIds?.Clear();
    }

    public static CharacterAppearanceData CreateDefault()
    {
        return new CharacterAppearanceData();
    }

    public static CharacterAppearanceData Copy(
        CharacterAppearanceData source)
    {
        if (source == null)
            return CreateDefault();

        CharacterAppearanceData copy =
            new CharacterAppearanceData
            {
                bodyScale = source.bodyScale,

                hue = source.hue,
                saturation = source.saturation,
                value = source.value,

                hairHue = source.hairHue,
                hairSaturation = source.hairSaturation,
                hairValue = source.hairValue,

                eyeHue = source.eyeHue,
                eyeSaturation = source.eyeSaturation,
                eyeValue = source.eyeValue,

                headOptionId = source.headOptionId,
                earOptionId = source.earOptionId,
                hornOptionId = source.hornOptionId,
                tailOptionId = source.tailOptionId,
                hairOptionId = source.hairOptionId,
                eyeOptionId = source.eyeOptionId
            };

        if (source.markingOptionIds != null)
        {
            copy.markingOptionIds.AddRange(
                source.markingOptionIds
            );
        }

        return copy;
    }
}