using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCreatorModelPreviewUI : MonoBehaviour
{
    [Serializable]
    private class RaceSizePreviewTransform
    {
        public RaceSize raceSize;

        public Vector3 localPosition;
        public Vector3 localEulerAngles;

        public Vector3 localScale =
            Vector3.one;
    }

    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Preview")]
    [SerializeField] private Transform previewParent;
    [SerializeField] private GameObject defaultPreviewPrefab;

    [Header("Capsule Race Size Transforms")]
    [SerializeField]
    private List<RaceSizePreviewTransform> raceSizeTransforms = new();

    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    private GameObject currentPreview;
    private GameObject currentSourcePrefab;

    private Vector3 prefabPosition;
    private Quaternion prefabRotation;
    private Vector3 prefabScale;

    private Renderer[] currentRenderers;

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock =
            new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        SubscribeToCreator();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromCreator();
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
        if (characterCreator == null ||
            characterDataLibrary == null ||
            previewParent == null ||
            defaultPreviewPrefab == null)
        {
            return;
        }

        SubraceDefinition subraceDefinition =
            GetSelectedSubrace();

        GameObject desiredPrefab =
            GetPreviewPrefab(subraceDefinition);

        if (currentPreview == null ||
            currentSourcePrefab != desiredPrefab)
        {
            SpawnPreview(desiredPrefab);
        }

        if (currentPreview == null)
            return;

        CharacterAppearanceData appearance =
            characterCreator.SelectedAppearance;

        bool usingDefaultPrefab =
            desiredPrefab == defaultPreviewPrefab;

        if (usingDefaultPrefab)
        {
            RaceSize raceSize =
                subraceDefinition != null
                    ? subraceDefinition.size
                    : RaceSize.Size2;

            ApplyCapsuleTransform(
                raceSize,
                appearance
            );
        }
        else
        {
            ApplyPrefabTransform(appearance);
        }

        ApplyColor(appearance);
    }

    private SubraceDefinition GetSelectedSubrace()
    {
        if (string.IsNullOrWhiteSpace(
            characterCreator.SelectedSubraceId))
        {
            return null;
        }

        characterDataLibrary.TryGetSubraceDefinition(
            characterCreator.SelectedSubraceId,
            out SubraceDefinition subraceDefinition
        );

        return subraceDefinition;
    }

    private GameObject GetPreviewPrefab(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition != null &&
            subraceDefinition.previewPrefab != null)
        {
            return subraceDefinition.previewPrefab;
        }

        return defaultPreviewPrefab;
    }

    private void SpawnPreview(
        GameObject previewPrefab)
    {
        if (currentPreview != null)
            Destroy(currentPreview);

        if (previewPrefab == null)
            return;

        currentPreview =
            Instantiate(
                previewPrefab,
                previewParent,
                false
            );

        currentSourcePrefab = previewPrefab;

        Transform previewTransform =
            currentPreview.transform;

        prefabPosition =
            previewTransform.localPosition;

        prefabRotation =
            previewTransform.localRotation;

        prefabScale =
            previewTransform.localScale;

        currentRenderers =
            currentPreview.GetComponentsInChildren<Renderer>(
                true
            );
    }

    private void ApplyCapsuleTransform(
        RaceSize raceSize,
        CharacterAppearanceData appearance)
    {
        if (currentPreview == null ||
            appearance == null)
        {
            return;
        }

        RaceSizePreviewTransform raceTransform =
            GetRaceSizeTransform(raceSize);

        Transform previewTransform =
            currentPreview.transform;

        if (raceTransform == null)
        {
            previewTransform.localPosition =
                prefabPosition;

            previewTransform.localRotation =
                prefabRotation;

            previewTransform.localScale =
                prefabScale * appearance.bodyScale;

            return;
        }

        previewTransform.localPosition =
            raceTransform.localPosition;

        previewTransform.localRotation =
            Quaternion.Euler(
                raceTransform.localEulerAngles
            );

        previewTransform.localScale =
            raceTransform.localScale *
            appearance.bodyScale;
    }

    private void ApplyPrefabTransform(
        CharacterAppearanceData appearance)
    {
        if (currentPreview == null ||
            appearance == null)
        {
            return;
        }

        Transform previewTransform =
            currentPreview.transform;

        previewTransform.localPosition =
            prefabPosition;

        previewTransform.localRotation =
            prefabRotation;

        previewTransform.localScale =
            prefabScale * appearance.bodyScale;
    }

    private RaceSizePreviewTransform GetRaceSizeTransform(
        RaceSize raceSize)
    {
        foreach (RaceSizePreviewTransform raceTransform
                 in raceSizeTransforms)
        {
            if (raceTransform != null &&
                raceTransform.raceSize == raceSize)
            {
                return raceTransform;
            }
        }

        return null;
    }

    private void ApplyColor(
        CharacterAppearanceData appearance)
    {
        if (appearance == null ||
            currentRenderers == null)
        {
            return;
        }

        Color color =
            Color.HSVToRGB(
                appearance.hue,
                appearance.saturation,
                appearance.value
            );

        foreach (Renderer targetRenderer
                 in currentRenderers)
        {
            ApplyColor(targetRenderer, color);
        }
    }

    private void ApplyColor(
        Renderer targetRenderer,
        Color color)
    {
        if (targetRenderer == null)
            return;

        Material material =
            targetRenderer.sharedMaterial;

        if (material == null)
            return;

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