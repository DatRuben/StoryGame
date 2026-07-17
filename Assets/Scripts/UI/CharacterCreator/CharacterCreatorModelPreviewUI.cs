using UnityEngine;

public class CharacterCreatorModelPreviewUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Preview Model")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Renderer[] skinRenderers;

    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    private Vector3 originalScale;
    private bool hasOriginalScale;

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock =
            new MaterialPropertyBlock();

        CaptureOriginalScale();
    }

    private void OnEnable()
    {
        CaptureOriginalScale();
        SubscribeToCreator();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromCreator();
    }

    private void CaptureOriginalScale()
    {
        if (hasOriginalScale ||
            modelRoot == null)
        {
            return;
        }

        originalScale = modelRoot.localScale;
        hasOriginalScale = true;
    }

    private void SubscribeToCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= Refresh;
        characterCreator.SelectionChanged += Refresh;
    }

    private void UnsubscribeFromCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= Refresh;
    }

    private void Refresh()
    {
        if (characterCreator == null)
            return;

        CharacterAppearanceData appearance =
            characterCreator.SelectedAppearance;

        ApplyScale(appearance);
        ApplySkinColor(appearance);
    }

    private void ApplyScale(
        CharacterAppearanceData appearance)
    {
        if (modelRoot == null ||
            appearance == null ||
            !hasOriginalScale)
        {
            return;
        }

        modelRoot.localScale =
            originalScale * appearance.bodyScale;
    }

    private void ApplySkinColor(
        CharacterAppearanceData appearance)
    {
        if (appearance == null)
            return;

        Color skinColor =
            Color.HSVToRGB(
                appearance.hue,
                appearance.saturation,
                appearance.value
            );

        ApplyColor(
            skinRenderers,
            skinColor
        );
    }

    private void ApplyColor(
        Renderer[] renderers,
        Color color)
    {
        if (renderers == null)
            return;

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
                continue;

            Material material =
                targetRenderer.sharedMaterial;

            if (material == null)
                continue;

            targetRenderer.GetPropertyBlock(
                propertyBlock
            );

            if (material.HasProperty(BaseColorId))
            {
                propertyBlock.SetColor(
                    BaseColorId,
                    color
                );
            }
            else if (material.HasProperty(ColorId))
            {
                propertyBlock.SetColor(
                    ColorId,
                    color
                );
            }

            targetRenderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }
}