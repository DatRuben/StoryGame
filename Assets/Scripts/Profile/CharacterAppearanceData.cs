using System;
using UnityEngine;

[Serializable]
public class CharacterAppearanceData
{
    public const float MinBodyScale = 0.8f;
    public const float MaxBodyScale = 1.2f;

    [Header("Body")]
    [Range(MinBodyScale, MaxBodyScale)]
    public float bodyScale = 1f;

    public float SafeBodyScale =>
        ClampBodyScale(bodyScale);

    public static float ClampBodyScale(float scale)
    {
        return Mathf.Clamp(
            scale,
            MinBodyScale,
            MaxBodyScale
        );
    }

    [Header("Skin Color")]
    [Range(0f, 1f)]
    public float hue = 0f;

    [Range(0f, 1f)]
    public float saturation = 0.75f;

    [Range(0f, 1f)]
    public float value = 0.9f;

	[Header("Tail Color")]
	[Range(0f, 1f)]
	public float tailHue = 0f;

	[Range(0f, 1f)]
	public float tailSaturation = 0.75f;

	[Range(0f, 1f)]
	public float tailValue = 0.9f;

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
	public string bodyOptionId = "";
	public string headOptionId = "";
    public string earOptionId = "";
    public string hornOptionId = "";
    public string tailOptionId = "";
    public string hairOptionId = "";
    public string eyeOptionId = "";
    public string markingOptionId = "";
    public string bodyPatternOptionId = "";

    public string GetSingleOptionId(
        CharacterAppearanceCategory category)
    {
        switch (category)
        {
			case CharacterAppearanceCategory.Body:
				return bodyOptionId;

			case CharacterAppearanceCategory.Head:
                return headOptionId;

            case CharacterAppearanceCategory.Ears:
                return earOptionId;

            case CharacterAppearanceCategory.Horns:
                return hornOptionId;

            case CharacterAppearanceCategory.Tail:
                return tailOptionId;

            case CharacterAppearanceCategory.Hair:
                return hairOptionId;

            case CharacterAppearanceCategory.Eyes:
                return eyeOptionId;

            case CharacterAppearanceCategory.Marking:
                return markingOptionId;

            case CharacterAppearanceCategory.BodyPattern:
                return bodyPatternOptionId;

            default:
                return "";
        }
    }

    public bool SetSingleOptionId(
        CharacterAppearanceCategory category,
        string optionId)
    {
        optionId =
            string.IsNullOrWhiteSpace(optionId)
                ? ""
                : optionId;

        switch (category)
        {
			case CharacterAppearanceCategory.Body:
				bodyOptionId = optionId;
				return true;

			case CharacterAppearanceCategory.Head:
                headOptionId = optionId;
                return true;

            case CharacterAppearanceCategory.Ears:
                earOptionId = optionId;
                return true;

            case CharacterAppearanceCategory.Horns:
                hornOptionId = optionId;
                return true;

            case CharacterAppearanceCategory.Tail:
                tailOptionId = optionId;
                return true;

            case CharacterAppearanceCategory.Hair:
                hairOptionId = optionId;
                return true;

            case CharacterAppearanceCategory.Eyes:
                eyeOptionId = optionId;
                return true;

            case CharacterAppearanceCategory.Marking:
                markingOptionId = optionId;
                return true;

            case CharacterAppearanceCategory.BodyPattern:
                bodyPatternOptionId = optionId;
                return true;

            default:
                return false;
        }
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

        return new CharacterAppearanceData
        {
            bodyScale = source.bodyScale,

            hue = source.hue,
            saturation = source.saturation,
            value = source.value,

			tailHue = source.tailHue,
			tailSaturation = source.tailSaturation,
			tailValue = source.tailValue,

			hairHue = source.hairHue,
            hairSaturation = source.hairSaturation,
            hairValue = source.hairValue,

            eyeHue = source.eyeHue,
            eyeSaturation = source.eyeSaturation,
            eyeValue = source.eyeValue,

			bodyOptionId = source.bodyOptionId,
			headOptionId = source.headOptionId,
            earOptionId = source.earOptionId,
            hornOptionId = source.hornOptionId,
            tailOptionId = source.tailOptionId,
            hairOptionId = source.hairOptionId,
            eyeOptionId = source.eyeOptionId,
            markingOptionId = source.markingOptionId,
            bodyPatternOptionId =
                source.bodyPatternOptionId
        };
    }
}