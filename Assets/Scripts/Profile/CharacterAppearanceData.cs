using System;
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

    [Header("Placeholder Options")]
    [Min(0)]
    public int headType;

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

            hairHue = source.hairHue,
            hairSaturation = source.hairSaturation,
            hairValue = source.hairValue,

            eyeHue = source.eyeHue,
            eyeSaturation = source.eyeSaturation,
            eyeValue = source.eyeValue,

            headType = source.headType
        };
    }
}