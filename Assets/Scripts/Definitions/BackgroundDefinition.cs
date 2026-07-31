using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Background Definition")]
public class BackgroundDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector] public string backgroundId;
    public string displayName;

    [TextArea]
    public string description;

    [Header("Attribute Trade Modifiers")]
    public CharacterAttributeModifiers modifiers =
        CharacterAttributeModifiers.CreateZero();

    [Header("Future Theme")]
    public string skillTheme;

    public bool HasNeutralModifierTotal()
    {
        return modifiers == null ||
               modifiers.Total() == 0;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        backgroundId = MakeId(displayName);
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