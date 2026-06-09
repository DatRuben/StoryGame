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

    public PlacedInventoryItem HeldItem { get; private set; }

    public bool IsHoldingItem => HeldItem != null;

    public event Action OnInventoryChanged;
    public event Action OnHeldItemChanged;

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

    public bool CanPlaceItem(
        ItemData item,
        int x,
        int y,
        bool rotated)
    {
        if (Grid == null)
            return false;

        return Grid.CanPlaceItem(
            item,
            x,
            y,
            rotated
        );
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

    public PlacedInventoryItem TryPickUpItemAt(
        int x,
        int y)
    {
        if (Grid == null)
            return null;

        if (HeldItem != null)
            return null;

        PlacedInventoryItem pickedItem =
            Grid.PickUpItemAt(x, y);

        if (pickedItem == null)
            return null;

        HeldItem = pickedItem;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return HeldItem;
    }

    public bool CanPlaceHeldItem(
        int x,
        int y)
    {
        if (Grid == null)
            return false;

        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        return Grid.CanPlaceItem(
            HeldItem.ItemData,
            x,
            y,
            HeldItem.Rotated
        );
    }

    public bool TryPlaceHeldItem(
        int x,
        int y)
    {
        if (Grid == null)
            return false;

        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        ItemData itemData =
            HeldItem.ItemData;

        bool rotated =
            HeldItem.Rotated;

        bool placed =
            Grid.PlaceItem(
                itemData,
                x,
                y,
                rotated
            );

        if (!placed)
            return false;

        HeldItem = null;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
    }
}