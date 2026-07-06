using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Subrace Definition")]
public class SubraceDefinition : ScriptableObject
{
    [Header("Identity")]
    public string subraceId;
    public string displayName;
    public RaceDefinition race;

    [TextArea]
    public string description;

    [Header("Comparison")]
    public SubraceDefinition compareToSubrace;

    public CharacterAttributeModifiers modifiersFromComparison =
        CharacterAttributeModifiers.CreateZero();

    [Header("Body")]
    public RaceSize size;
    public BodyType bodyType;

    [Header("Equipment / Holding Rules")]
    public bool canHoldItemInMouth;
    public bool canUseMouthWeapons;
    public bool canEquipSaddles;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        subraceId = MakeId(displayName);
    }

    private string MakeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        StringBuilder builder = new();

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length > 0 &&
                     builder[^1] != '_')
            {
                builder.Append('_');
            }
        }

        return builder.ToString().Trim('_');
    }
}