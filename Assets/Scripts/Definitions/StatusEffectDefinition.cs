using System.Text;
using UnityEngine;

public enum StatusEffectStacking
{
    RefreshDuration,
    AddStack,
    Replace
}

public enum StatusEffectDisposition
{
    Neutral,
    Beneficial,
    Harmful,
    Mixed
}

[CreateAssetMenu(
    menuName = "Game/Status Effect Definition"
)]
public class StatusEffectDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector]
    public string effectId;

    public string displayName;

    [TextArea]
    public string description;

    public Sprite icon;

    [Header("Presentation")]
    [Tooltip(
        "Used for UI presentation only. " +
        "It does not control how the effect behaves."
    )]
    public StatusEffectDisposition disposition =
        StatusEffectDisposition.Neutral;

    [Header("Target Eligibility")]
    [Tooltip(
        "None means this effect can affect any entity. " +
        "Otherwise the target must have at least one matching entity trait."
    )]
    public EntityTrait allowedEntityTraits =
        EntityTrait.None;

    [Header("Duration")]
    [Tooltip(
        "A duration of 0 means the effect lasts until removed."
    )]
    [Min(0f)]
    public float duration;

    [Header("Stacking")]
    public StatusEffectStacking stacking =
        StatusEffectStacking.RefreshDuration;

    [Min(1)]
    public int maxStacks = 1;

    [Header("Attribute Modifiers")]
    public CharacterAttributeModifiers
        attributeModifiers =
            CharacterAttributeModifiers.CreateZero();

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        effectId = MakeId(displayName);

        duration =
            Mathf.Max(
                0f,
                duration
            );

        maxStacks =
            Mathf.Max(
                1,
                maxStacks
            );

        disposition =
            ResolveDisposition(
                attributeModifiers
            );
    }

    private StatusEffectDisposition ResolveDisposition(
        CharacterAttributeModifiers modifiers)
    {
        if (modifiers == null ||
            !modifiers.HasAny())
        {
            return StatusEffectDisposition.Neutral;
        }

        bool hasPositive = false;
        bool hasNegative = false;

        CheckModifier(
            modifiers.strength,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.dexterity,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.agility,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.vitality,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.endurance,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.intelligence,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.willpower,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.spirit,
            ref hasPositive,
            ref hasNegative
        );

        CheckModifier(
            modifiers.perception,
            ref hasPositive,
            ref hasNegative
        );

        if (hasPositive && hasNegative)
            return StatusEffectDisposition.Mixed;

        if (hasPositive)
            return StatusEffectDisposition.Beneficial;

        if (hasNegative)
            return StatusEffectDisposition.Harmful;

        return StatusEffectDisposition.Neutral;
    }

    private void CheckModifier(
        int value,
        ref bool hasPositive,
        ref bool hasNegative)
    {
        if (value > 0)
            hasPositive = true;
        else if (value < 0)
            hasNegative = true;
    }

    private string MakeId(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        StringBuilder builder =
            new StringBuilder();

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(
                    char.ToLowerInvariant(character)
                );
            }
            else if (builder.Length > 0 &&
                     builder[builder.Length - 1] != '_')
            {
                builder.Append('_');
            }
        }

        return builder
            .ToString()
            .Trim('_');
    }

    public bool CanApplyTo(
        EntityClassification target)
    {
        if (allowedEntityTraits ==
            EntityTrait.None)
        {
            return true;
        }

        return target != null &&
               target.HasAny(
                   allowedEntityTraits
               );
    }
}