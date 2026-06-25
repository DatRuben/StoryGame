using System;
using UnityEngine;

public enum RaceBodyFamily
{
    Humanoid,
    Feral,
    LargeCreature,
    Dragon,
    Griffin
}

[Serializable]
public class RaceBodyProfile
{
    [Header("Body Category")]
    public RaceBodyFamily bodyFamily = RaceBodyFamily.Humanoid;
    public RaceSize raceSize = RaceSize.Size2;

    [Tooltip("1.0 means the reference body for this family. Human/Size 2 Animali/Size 2 Drakken for humanoids. Size 2 Feralmali for ferals.")]
    public float bodyScale = 1f;

    [Header("Body Baselines")]
    public float baseHealth = 100f;
    public float baseStamina = 100f;
    public float baseMana = 50f;
    public float baseMass = 1f;
    public float basePoise = 1f;

    [Header("Risk / Cost")]
    public float hitboxRisk = 1f;
    public float movementCostMultiplier = 1f;
    public float dodgeCostMultiplier = 1f;
    public float equipmentWeightMultiplier = 1f;

    [Header("Attribute Scaling")]
    public float vitalityToHealth = 10f;
    public float enduranceToStamina = 10f;
    public float spiritToMana = 10f;
    public float vitalityToPoise = 0.05f;
    public float strengthToPoise = 0.05f;
}