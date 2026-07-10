using System;

[Serializable]
public class CharacterStatPreview
{
    public CharacterBaseStats baseStats;
    public CharacterBaseStats attributeBonuses;
    public CharacterBaseStats finalStats;

    public static CharacterStatPreview Create(
        CharacterBaseStats baseStats,
        CharacterBaseStats attributeBonuses)
    {
        return new CharacterStatPreview
        {
            baseStats = baseStats,
            attributeBonuses = attributeBonuses,
            finalStats = CharacterBaseStats.Add(
                baseStats,
                attributeBonuses
            )
        };
    }
}