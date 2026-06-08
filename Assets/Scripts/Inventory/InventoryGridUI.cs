using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGridUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;

    private GridLayoutGroup gridLayoutGroup;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 0.85f);

    private readonly List<Image> cellImages = new List<Image>();
    private readonly List<Vector2Int> cellCoordinates = new List<Vector2Int>();
    private void Awake()
    {
        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
    }

    private void Start()
    {
        if (playerInventory == null)
            return;

        playerInventory.OnInventoryChanged += Refresh;

        BuildGrid();
        Refresh();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= Refresh;
        }
    }

    private void BuildGrid()
    {
        if (playerInventory.Grid == null ||
            cellParent == null ||
            cellPrefab == null)
        {
            return;
        }

        foreach (Transform child in cellParent)
        {
            Destroy(child.gameObject);
        }

        cellImages.Clear();
        cellCoordinates.Clear();

        InventoryGrid grid = playerInventory.Grid;

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = grid.Width;
        }

        // Build top row first so it visually matches the debug grid.
        for (int y = grid.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                GameObject cellObject =
                    Instantiate(cellPrefab, cellParent);

                Image image =
                    cellObject.GetComponent<Image>();

                cellImages.Add(image);
                cellCoordinates.Add(new Vector2Int(x, y));
            }
        }
    }

    private void Refresh()
    {
        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        InventoryGrid grid = playerInventory.Grid;

        for (int i = 0; i < cellImages.Count; i++)
        {
            Image image = cellImages[i];

            if (image == null)
                continue;

            Vector2Int coordinate = cellCoordinates[i];

            ItemData item =
                grid.GetCell(coordinate.x, coordinate.y);

            image.color =
                item == null
                ? emptyColor
                : occupiedColor;
        }
    }
}