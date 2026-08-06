using System;
using UnityEngine;

[Serializable]
public class CharacterAttributeScaling
{
    [Min(0f)]
    public float strength = 1f;

    [Min(0f)]
    public float dexterity = 1f;

    [Min(0f)]
    public float agility = 1f;

    [Min(0f)]
    public float vitality = 1f;

    [Min(0f)]
    public float endurance = 1f;

    [Min(0f)]
    public float intelligence = 1f;

    [Min(0f)]
    public float willpower = 1f;

    [Min(0f)]
    public float spirit = 1f;

    [Min(0f)]
    public float perception = 1f;

    public static CharacterAttributeScaling CreateDefault(
        float value = 1f)
    {
        value = Mathf.Max(0f, value);

        return new CharacterAttributeScaling
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

    public static CharacterAttributeScaling Copy(
        CharacterAttributeScaling source)
    {
        if (source == null)
            return CreateDefault();

        return new CharacterAttributeScaling
        {
            strength = source.strength,
            dexterity = source.dexterity,
            agility = source.agility,
            vitality = source.vitality,
            endurance = source.endurance,
            intelligence = source.intelligence,
            willpower = source.willpower,
            spirit = source.spirit,
            perception = source.perception
        };
    }

    public void ClampMinimum(
        float minimum = 0f)
    {
        minimum = Mathf.Max(0f, minimum);

        strength =
            Mathf.Max(minimum, strength);

        dexterity =
            Mathf.Max(minimum, dexterity);

        agility =
            Mathf.Max(minimum, agility);

        vitality =
            Mathf.Max(minimum, vitality);

        endurance =
            Mathf.Max(minimum, endurance);

        intelligence =
            Mathf.Max(minimum, intelligence);

        willpower =
            Mathf.Max(minimum, willpower);

        spirit =
            Mathf.Max(minimum, spirit);

        perception =
            Mathf.Max(minimum, perception);
    }
}