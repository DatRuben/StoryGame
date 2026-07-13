using System;
using UnityEngine;

[Serializable]
public class ResolvedCharacterStats
{
    public CharacterAttributePreview attributePreview;
    public CharacterAttributes finalAttributes;

    public CharacterBaseStats baseStats;
    public CharacterBaseStats attributeBonuses;
    public CharacterBaseStats totalBaseStats;

    public FinalCharacterStats finalStats;
    public FinalMovementStats movementStats;
}