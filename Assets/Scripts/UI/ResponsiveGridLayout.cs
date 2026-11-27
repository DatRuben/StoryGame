using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(GridLayoutGroup))]
public sealed class ResponsiveGridLayout : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float cellAspectRatio = 1f;

    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        RefreshCellSize();
    }

    private void OnRectTransformDimensionsChange()
    {
        RefreshCellSize();
    }

    private void RefreshCellSize()
    {
        if (!isActiveAndEnabled || grid == null || rectTransform == null)
            return;

        if (grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
            return;

        int columns = Mathf.Max(1, grid.constraintCount);
        float availableWidth = rectTransform.rect.width - grid.padding.horizontal - grid.spacing.x * (columns - 1);

        if (availableWidth <= 0f)
            return;

        float cellWidth = availableWidth / columns;
        float cellHeight = cellWidth / Mathf.Max(0.01f, cellAspectRatio);

        Vector2 cellSize = new Vector2(cellWidth, cellHeight);

        if (grid.cellSize != cellSize)
            grid.cellSize = cellSize;
    }
}