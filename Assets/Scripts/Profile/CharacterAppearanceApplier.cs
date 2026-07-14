using UnityEngine;

public class CharacterAppearanceApplier : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Renderer[] renderers;

    private MaterialPropertyBlock propertyBlock;

    public void ApplyAppearance(
        CharacterAppearanceData appearance)
    {
        if (appearance == null)
            appearance = CharacterAppearanceData.CreateDefault();

        ApplyBodyScale(appearance.bodyScale);
        ApplyColor(appearance);
    }

    private void ApplyBodyScale(
        float bodyScale)
    {
        Transform target =
            visualRoot != null
                ? visualRoot
                : transform;

        target.localScale =
            Vector3.one * Mathf.Max(
                0.01f,
                bodyScale
            );
    }

    private void ApplyColor(
        CharacterAppearanceData appearance)
    {
        Color color =
            Color.HSVToRGB(
                Mathf.Repeat(appearance.hue, 1f),
                Mathf.Clamp01(appearance.saturation),
                Mathf.Clamp01(appearance.value)
            );

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        Renderer[] targetRenderers =
            GetRenderers();

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
                continue;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private Renderer[] GetRenderers()
    {
        if (renderers != null &&
            renderers.Length > 0)
        {
            return renderers;
        }

        Transform target =
            visualRoot != null
                ? visualRoot
                : transform;

        return target.GetComponentsInChildren<Renderer>();
    }
}