using System;
using UnityEngine;

[Serializable]
public class CharacterAttributeOutput
{
    [Min(0f)] public float strength;
    [Min(0f)] public float dexterity;
    [Min(0f)] public float agility;
    [Min(0f)] public float vitality;
    [Min(0f)] public float endurance;
    [Min(0f)] public float intelligence;
    [Min(0f)] public float willpower;
    [Min(0f)] public float spirit;
    [Min(0f)] public float perception;

    public static CharacterAttributeOutput CreateDefault(
        float value = 10f)
    {
        value = Mathf.Max(0f, value);

        return new CharacterAttributeOutput
        {
            strength = value,
            dexterity = value,
            agility = value,
            vitality = value,
            endurance = value,
            intelligence = value,
            willpower = value,
            spirit = value,
            perception = value
        };
    }
}

public static class CharacterAttributeOutputResolver
{
    public const float BaselineAttribute = 10f;

    public static CharacterAttributeOutput Resolve(
        CharacterAttributes attributes,
        CharacterAttributeScaling scaling)
    {
        attributes =
            CharacterAttributes.ClampMinimum(
                attributes,
                1
            );

        if (scaling == null)
        {
            scaling =
                CharacterAttributeScaling.CreateDefault();
        }

        return new CharacterAttributeOutput
        {
            strength = ResolveValue(
                attributes.strength,
                scaling.strength
            ),

            dexterity = ResolveValue(
                attributes.dexterity,
                scaling.dexterity
            ),

            agility = ResolveValue(
                attributes.agility,
                scaling.agility
            ),

            vitality = ResolveValue(
                attributes.vitality,
                scaling.vitality
            ),

            endurance = ResolveValue(
                attributes.endurance,
                scaling.endurance
            ),

            intelligence = ResolveValue(
                attributes.intelligence,
                scaling.intelligence
            ),

            willpower = ResolveValue(
                attributes.willpower,
                scaling.willpower
            ),

            spirit = ResolveValue(
                attributes.spirit,
                scaling.spirit
            ),

            perception = ResolveValue(
                attributes.perception,
                scaling.perception
            )
        };
    }

    private static float ResolveValue(
        int attribute,
        float scaling)
    {
        attribute = Mathf.Max(1, attribute);
        scaling = Mathf.Max(0f, scaling);

        float differenceFromBaseline =
            attribute - BaselineAttribute;

        return Mathf.Max(
            0f,
            BaselineAttribute +
            differenceFromBaseline * scaling
        );
    }
}