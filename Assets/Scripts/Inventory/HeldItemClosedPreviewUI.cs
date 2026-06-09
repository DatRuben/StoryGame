using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeldItemClosedPreviewUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject cellPrefab;

    [Header("Top Right Position")]
    [SerializeField] private Vector2 topRightOffset = new Vector2(-24f, -24f);
    [SerializeField] private float panelWidth = 320f;
    [SerializeField] private float panelHeight = 200f;

    [Header("Preview Settings")]
    [SerializeField] private Vector2 previewCellSize = new Vector2(24f, 24f);
    [SerializeField] private Vector2 previewSpacing = new Vector2(1f, 1f);
    [SerializeField] private Color previewColor = new Color(1f, 1f, 1f, 0.7f);

    [Header("Text Settings")]
    [SerializeField] private string holdingPrefix = "Holding: ";
    [SerializeField] private int fontSize = 22;
    [SerializeField] private Color textColor = Color.white;

    private Canvas rootCanvas;

    private RectTransform containerRoot;
    private CanvasGroup containerCanvasGroup;

    private TextMeshProUGUI heldItemNameText;

    private RectTransform previewGridRoot;
    private GridLayoutGroup previewLayoutGroup;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        CreatePreviewUI();
    }

    private void Start()
    {
        if (playerInventory != null)
        {
            playerInventory.OnHeldItemChanged += HandleHeldItemChanged;
        }

        HandleHeldItemChanged();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnHeldItemChanged -= HandleHeldItemChanged;
        }
    }

    private void HandleHeldItemChanged()
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
            containerObject.GetComponent<RectTransform>();

        containerRoot.anchorMin = new Vector2(1f, 1f);
        containerRoot.anchorMax = new Vector2(1f, 1f);
        containerRoot.pivot = new Vector2(1f, 1f);
        containerRoot.anchoredPosition = topRightOffset;
        containerRoot.sizeDelta = new Vector2(panelWidth, panelHeight);

        containerCanvasGroup =
            containerObject.GetComponent<CanvasGroup>();

        containerCanvasGroup.blocksRaycasts = false;
        containerCanvasGroup.interactable = false;

        VerticalLayoutGroup verticalLayout =
            containerObject.GetComponent<VerticalLayoutGroup>();

        verticalLayout.childAlignment = TextAnchor.UpperRight;
        verticalLayout.spacing = 6f;
        verticalLayout.childControlWidth = false;
        verticalLayout.childControlHeight = false;
        verticalLayout.childForceExpandWidth = false;
        verticalLayout.childForceExpandHeight = false;

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
            textObject.GetComponent<TextMeshProUGUI>();

        heldItemNameText.fontSize = fontSize;
        heldItemNameText.color = textColor;
        heldItemNameText.alignment = TextAlignmentOptions.TopRight;
        heldItemNameText.raycastTarget = false;
        heldItemNameText.text = "";

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.sizeDelta = new Vector2(panelWidth, 32f);

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
            previewObject.GetComponent<RectTransform>();

        previewGridRoot.sizeDelta =
            new Vector2(previewCellSize.x, previewCellSize.y);

        previewLayoutGroup =
            previewObject.GetComponent<GridLayoutGroup>();

        previewLayoutGroup.startCorner =
            GridLayoutGroup.Corner.UpperLeft;

        previewLayoutGroup.startAxis =
            GridLayoutGroup.Axis.Horizontal;

        previewLayoutGroup.childAlignment =
            TextAnchor.UpperLeft;

        previewLayoutGroup.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

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

        if (playerInventory == null ||
            playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemData == null)
        {
            heldItemNameText.text = "";
            return;
        }

        heldItemNameText.text =
            holdingPrefix +
            playerInventory.HeldItem.ItemData.itemName;
    }

    private void BuildPreview()
    {
        if (previewGridRoot == null ||
            previewLayoutGroup == null ||
            cellPrefab == null)
        {
            return;
        }

        foreach (Transform child in previewGridRoot)
        {
            Destroy(child.gameObject);
        }

        if (playerInventory == null ||
            playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemData == null)
        {
            return;
        }

        PlacedInventoryItem heldItem =
            playerInventory.HeldItem;

        previewLayoutGroup.constraintCount =
            heldItem.Width;

        float previewWidth =
            heldItem.Width * previewCellSize.x +
            Mathf.Max(0, heldItem.Width - 1) * previewSpacing.x;

        float previewHeight =
            heldItem.Height * previewCellSize.y +
            Mathf.Max(0, heldItem.Height - 1) * previewSpacing.y;

        previewGridRoot.sizeDelta =
            new Vector2(previewWidth, previewHeight);

        ItemData itemData =
            heldItem.ItemData;

        for (int y = heldItem.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < heldItem.Width; x++)
            {
                GameObject cellObject =
                    Instantiate(
                        cellPrefab,
                        previewGridRoot
                    );

                InventoryCellUI cellUI =
                    cellObject.GetComponent<InventoryCellUI>();

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
                    bool occupied =
                        itemData.IsCellOccupied(
                            x,
                            y,
                            heldItem.Rotated
                        );

                    image.raycastTarget = false;

                    image.color =
                        occupied
                        ? previewColor
                        : new Color(0f, 0f, 0f, 0f);
                }
            }
        }
    }

    private void UpdateVisibility()
    {
        if (containerRoot == null)
            return;

        bool shouldShow =
            playerInventory != null &&
            playerInventory.HeldItem != null &&
            !InventoryMenuController.IsInventoryOpen;

        if (containerRoot.gameObject.activeSelf != shouldShow)
        {
            containerRoot.gameObject.SetActive(shouldShow);
        }
    }
}