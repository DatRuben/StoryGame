using System;
using UnityEngine;

[Serializable]
public class CharacterAppearanceData
{
    [Range(0.8f, 1.2f)]
    public float bodyScale = 1f;

    [Range(0f, 1f)]
    public float hue = 0f;

    [Range(0f, 1f)]
    public float saturation = 0.75f;

    [Range(0f, 1f)]
    public float value = 0.9f;

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
            value = source.value
        };
    }
}