using System;
using UnityEngine;

[Serializable]
public class LineageInfluencePreview
{
    public string selectionId = "";
    public string displayName = "";

    [Range(0f, 1f)]
    public float influence;

    public CharacterAttributeModifiers effectiveModifiers =
        CharacterAttributeModifiers.CreateZero();

    public static LineageInfluencePreview Create(
        LineageSelection lineage,
        float influence)
    {
        return new LineageInfluencePreview
        {
            selectionId =
                lineage != null
                    ? lineage.SelectionId
                    : "",

            displayName =
                lineage != null
                    ? lineage.DisplayName
                    : "",

            influence =
                Mathf.Clamp01(influence),

            effectiveModifiers =
                CharacterAttributeModifiers.CreateZero()
        };
    }
}