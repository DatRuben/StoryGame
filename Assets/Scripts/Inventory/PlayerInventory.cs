using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 6;

    [Header("Test Starting Item")]
    [SerializeField] private ItemData startingItem;
    [SerializeField] private int startingX = 1;
    [SerializeField] private int startingY = 1;
    [SerializeField] private bool startingRotated = false;

    public InventoryGrid Grid { get; private set; }

    public event Action OnInventoryChanged;

    private void Awake()
    {
        Grid = new InventoryGrid(gridWidth, gridHeight);
    }

    private void Start()
    {
        if (startingItem != null)
        {
            TryPlaceItem(
                startingItem,
                startingX,
                startingY,
                startingRotated
            );
        }
    }

    public bool TryPlaceItem(
        ItemData item,
        int x,
        int y,
        bool rotated)
    {
        if (Grid == null)
            return false;

        bool placed =
            Grid.PlaceItem(item, x, y, rotated);

        if (placed)
        {
            OnInventoryChanged?.Invoke();
        }

        return placed;
    }
}