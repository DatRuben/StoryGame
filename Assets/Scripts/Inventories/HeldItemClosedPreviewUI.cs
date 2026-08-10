using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeldItemClosedPreviewUI :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private InventoryInteractionController
        interactionController;

    [SerializeField]
    private GameObject cellPrefab;

    [Header("Top Right Position")]
    [SerializeField]
    private Vector2 topRightOffset =
        new Vector2(-24f, -24f);

    [SerializeField]
    private float panelWidth = 320f;

    [SerializeField]
    private float panelHeight = 200f;

    [Header("Preview")]
    [SerializeField]
    private Vector2 previewCellSize =
        new Vector2(24f, 24f);

    [SerializeField]
    private Vector2 previewSpacing =
        new Vector2(1f, 1f);

    [SerializeField]
    private Color previewColor =
        new Color(1f, 1f, 1f, 0.7f);

    [Header("Text")]
    [SerializeField]
    private string holdingPrefix =
        "Holding: ";

    [SerializeField]
    private int fontSize = 22;

    [SerializeField]
    private Color textColor =
        Color.white;

    private Canvas rootCanvas;
    private RectTransform containerRoot;
    private TextMeshProUGUI heldItemNameText;
    private RectTransform previewGridRoot;
    private GridLayoutGroup previewLayoutGroup;

    private void Awake()
    {
        rootCanvas =
            GetComponentInParent<Canvas>();

        CreatePreviewUI();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void BindPlayer(
        InventoryInteractionController
            newInteractionController)
    {
        Unsubscribe();

        interactionController =
            newInteractionController;

        if (isActiveAndEnabled)
            Subscribe();

        Refresh();
    }

    private void Subscribe()
    {
        if (interactionController != null)
        {
            interactionController.Changed +=
                Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (interactionController != null)
        {
            interactionController.Changed -=
                Refresh;
        }
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void Refresh()
    {
        UpdateHeldItemName();
        BuildPreview();
        UpdateVisibility();
    }

    private void CreatePreviewUI()
    {
        if (rootCanvas == null)
            return;

        GameObject containerObject =
            new GameObject(
                "ClosedInventoryHeldItemPanel",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(VerticalLayoutGroup)
            );

        containerObject.transform.SetParent(
            rootCanvas.transform,
            false
        );

        containerRoot =
            containerObject.GetComponent<
                RectTransform>();

        containerRoot.anchorMin =
            new Vector2(1f, 1f);

        containerRoot.anchorMax =
            new Vector2(1f, 1f);

        containerRoot.pivot =
            new Vector2(1f, 1f);

        containerRoot.anchoredPosition =
            topRightOffset;

        containerRoot.sizeDelta =
            new Vector2(
                panelWidth,
                panelHeight
            );

        CanvasGroup canvasGroup =
            containerObject.GetComponent<
                CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        VerticalLayoutGroup verticalLayout =
            containerObject.GetComponent<
                VerticalLayoutGroup>();

        verticalLayout.childAlignment =
            TextAnchor.UpperRight;

        verticalLayout.spacing = 6f;

        verticalLayout.childControlWidth =
            false;

        verticalLayout.childControlHeight =
            false;

        verticalLayout.childForceExpandWidth =
            false;

        verticalLayout.childForceExpandHeight =
            false;

        GameObject textObject =
            new GameObject(
                "HeldItemNameText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            containerRoot,
            false
        );

        heldItemNameText =
            textObject.GetComponent<
                TextMeshProUGUI>();

        heldItemNameText.fontSize =
            fontSize;

        heldItemNameText.color =
            textColor;

        heldItemNameText.alignment =
            TextAlignmentOptions.TopRight;

        heldItemNameText.raycastTarget =
            false;

        RectTransform textRect =
            textObject.GetComponent<
                RectTransform>();

        textRect.sizeDelta =
            new Vector2(
                panelWidth,
                32f
            );

        GameObject previewObject =
            new GameObject(
                "HeldItemShapePreview",
                typeof(RectTransform),
                typeof(GridLayoutGroup)
            );

        previewObject.transform.SetParent(
            containerRoot,
            false
        );

        previewGridRoot =
            previewObject.GetComponent<
                RectTransform>();

        previewLayoutGroup =
            previewObject.GetComponent<
                GridLayoutGroup>();

        previewLayoutGroup.startCorner =
            GridLayoutGroup.Corner.UpperLeft;

        previewLayoutGroup.startAxis =
            GridLayoutGroup.Axis.Horizontal;

        previewLayoutGroup.childAlignment =
            TextAnchor.UpperRight;

        previewLayoutGroup.constraint =
            GridLayoutGroup.Constraint
                .FixedColumnCount;

        previewLayoutGroup.cellSize =
            previewCellSize;

        previewLayoutGroup.spacing =
            previewSpacing;

        containerObject.SetActive(false);
    }

    private void UpdateHeldItemName()
    {
        if (heldItemNameText == null)
            return;

        if (interactionController == null ||
            !interactionController.HasSelection ||
            interactionController
                .SelectedDefinition == null)
        {
            heldItemNameText.text = "";
            return;
        }

        heldItemNameText.text =
            holdingPrefix +
            interactionController
                .SelectedDefinition
                .itemName;
    }

    private void BuildPreview()
    {
        if (previewGridRoot == null ||
            previewLayoutGroup == null ||
            cellPrefab == null)
        {
            return;
        }

        InventoryUIUtility.ClearChildren(
            previewGridRoot
        );

        if (interactionController == null ||
            !interactionController.HasSelection ||
            interactionController
                .SelectedDefinition == null)
        {
            return;
        }

        ItemDefinition definition =
            interactionController
                .SelectedDefinition;

        int rotation =
            interactionController
                .SelectedRotationSteps;

        int width =
            definition.GetWidth(
                rotation
            );

        int height =
            definition.GetHeight(
                rotation
            );

        previewLayoutGroup.constraintCount =
            width;

        previewGridRoot.sizeDelta =
            new Vector2(
                width * previewCellSize.x +
                Mathf.Max(0, width - 1) *
                    previewSpacing.x,

                height * previewCellSize.y +
                Mathf.Max(0, height - 1) *
                    previewSpacing.y
            );

        for (int y = height - 1;
             y >= 0;
             y--)
        {
            for (int x = 0;
                 x < width;
                 x++)
            {
                GameObject cellObject =
                    Instantiate(
                        cellPrefab,
                        previewGridRoot
                    );

                InventoryCellUI cellUI =
                    cellObject.GetComponent<
                        InventoryCellUI>();

                if (cellUI != null)
                    cellUI.enabled = false;

                Button button =
                    cellObject.GetComponent<Button>();

                if (button != null)
                    button.interactable = false;

                Image image =
                    cellObject.GetComponent<Image>();

                if (image != null)
                {
                    image.raycastTarget = false;

                    image.color =
                        definition.IsCellOccupied(
                            x,
                            y,
                            rotation
                        )
                            ? previewColor
                            : new Color(
                                0f,
                                0f,
                                0f,
                                0f
                            );
                }
            }
        }
    }

    private void UpdateVisibility()
    {
        if (containerRoot == null)
            return;

        bool shouldShow =
            interactionController != null &&
            interactionController.HasSelection &&
            !InventoryMenuController
                .IsInventoryOpen;

        if (containerRoot.gameObject
            .activeSelf != shouldShow)
        {
            containerRoot.gameObject
                .SetActive(
                    shouldShow
                );
        }
    }
}