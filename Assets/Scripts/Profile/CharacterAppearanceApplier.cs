using UnityEngine;

public class CharacterAppearanceApplier : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Renderer[] renderers;

    [Header("Automatic Reference Names")]
    [SerializeField] private string visualRootName = "VisualRoot";

    private MaterialPropertyBlock propertyBlock;

    private Transform cachedScaleTarget;
    private Vector3 baseVisualScale = Vector3.one;

    private void Awake()
    {
        ResolveTargets();
    }

    public void ApplyAppearance(
        CharacterAppearanceData appearance)
    {
        if (appearance == null)
        {
            appearance =
                CharacterAppearanceData.CreateDefault();
        }

        ResolveTargets();

        ApplyBodyScale(
            appearance.SafeBodyScale
        );

        ApplyColor(appearance);
    }

    private void ResolveTargets()
    {
        if (visualRoot == null)
        {
            visualRoot =
                FindChildRecursive(
                    transform,
                    visualRootName
                );
        }

        if (visualRoot == null ||
            visualRoot == cachedScaleTarget)
        {
            return;
        }

        cachedScaleTarget = visualRoot;
        baseVisualScale = visualRoot.localScale;
    }

    private void ApplyBodyScale(float bodyScale)
    {
        if (visualRoot == null)
        {
            Debug.LogWarning(
                "CharacterAppearanceApplier could not apply body scale because VisualRoot is missing.",
                this
            );

            return;
        }

        float safeScale =
            CharacterAppearanceData.ClampBodyScale(
                bodyScale
            );

        visualRoot.localScale =
            baseVisualScale * safeScale;
    }

    private void ApplyColor(
        CharacterAppearanceData appearance)
    {
        Color color =
            Color.HSVToRGB(
                Mathf.Clamp01(appearance.hue),
                Mathf.Clamp01(appearance.saturation),
                Mathf.Clamp01(appearance.value)
            );

        if (propertyBlock == null)
        {
            propertyBlock =
                new MaterialPropertyBlock();
        }

        Renderer[] targetRenderers =
            GetRenderers();

        for (int i = 0;
             i < targetRenderers.Length;
             i++)
        {
            Renderer targetRenderer =
                targetRenderers[i];

            if (targetRenderer == null)
                continue;

            targetRenderer.GetPropertyBlock(
                propertyBlock
            );

            propertyBlock.SetColor(
                "_BaseColor",
                color
            );

            propertyBlock.SetColor(
                "_Color",
                color
            );

            targetRenderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }

    private Renderer[] GetRenderers()
    {
        if (renderers != null &&
            renderers.Length > 0)
        {
            return renderers;
        }

        if (visualRoot == null)
            return new Renderer[0];

        return visualRoot.GetComponentsInChildren<Renderer>(
            true
        );
    }

    private Transform FindChildRecursive(
        Transform parent,
        string childName)
    {
        if (parent == null ||
            string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        for (int i = 0;
             i < parent.childCount;
             i++)
        {
            Transform child =
                parent.GetChild(i);

            if (child.name == childName)
                return child;

            Transform match =
                FindChildRecursive(
                    child,
                    childName
                );

            if (match != null)
                return match;
        }

        return null;
    }
}