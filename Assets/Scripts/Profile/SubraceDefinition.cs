using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Subrace Definition")]
public class SubraceDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector] public string subraceId;
    public string displayName;
    public RaceDefinition race;

    [TextArea]
    public string description;

    [Header("Comparison")]
    public SubraceDefinition compareToSubrace;

    public CharacterAttributeModifiers modifiersFromComparison =
        CharacterAttributeModifiers.CreateZero();

    [Header("Calculated Preview")]
    [SerializeField]
    private CharacterAttributes finalAttributesPreview =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int totalAttributePointsPreview;

    public CharacterAttributes FinalAttributesPreview =>
        finalAttributesPreview;

    public int TotalAttributePointsPreview =>
        totalAttributePointsPreview;

    [Header("Body")]
    public RaceSize size;
    public BodyType bodyType;

    [Header("Equipment / Holding Rules")]
    public bool canHoldItemInMouth;
    public bool canUseMouthWeapons;
    public bool canEquipSaddles;

    public void RecalculatePreview()
    {
        CharacterAttributes baseAttributes =
            race != null
                ? race.FinalAttributesPreview
                : CharacterAttributes.CreateDefault(10);

        if (compareToSubrace != null &&
            compareToSubrace != this)
        {
            baseAttributes =
                compareToSubrace.FinalAttributesPreview;
        }

        finalAttributesPreview =
            CharacterAttributes.AddModifiers(
                baseAttributes,
                modifiersFromComparison
            );

        totalAttributePointsPreview =
            finalAttributesPreview.BasePoints();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        subraceId = MakeId(displayName);

        RecalculatePreview();
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