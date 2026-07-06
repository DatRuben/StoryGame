using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Race Definition")]
public class RaceDefinition : ScriptableObject
{
    [Header("Identity")]
    public string raceId;
    public string displayName;
    public BaseRace baseRace;

    [TextArea]
    public string description;

    [Header("Standard Subrace")]
    public SubraceDefinition standardSubrace;

    [Header("Compared To Human")]
    public CharacterAttributeModifiers modifiersFromHuman =
        CharacterAttributeModifiers.CreateZero();

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        raceId = MakeId(displayName);
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