using UnityEngine;

[CreateAssetMenu(menuName = "Game/Background Definition")]
public class BackgroundDefinition : GameDefinition
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

        if (string.IsNullOrWhiteSpace(backgroundId))
        {
            backgroundId = MakeId(displayName);
        }
    }
}