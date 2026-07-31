using System;

public enum AttributeModifierSourceType
{
    Background,
    Trait,
    RacialPassive
}

[Serializable]
public class AttributeModifierPreview
{
    public AttributeModifierSourceType sourceType;

    public string sourceId = "";
    public string displayName = "";

    public CharacterAttributeModifiers modifiers =
        CharacterAttributeModifiers.CreateZero();

    public static AttributeModifierPreview Create(
        AttributeModifierSourceType sourceType,
        string sourceId,
        string displayName,
        CharacterAttributeModifiers modifiers)
    {
        return new AttributeModifierPreview
        {
            sourceType = sourceType,
            sourceId = sourceId ?? "",
            displayName = displayName ?? "",

            modifiers =
                CharacterAttributeModifiers.Copy(
                    modifiers
                )
        };
    }
}