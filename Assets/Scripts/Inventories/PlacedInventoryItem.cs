using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlacedInventoryItem
{
    [SerializeField]
    private InventoryItemInstance itemInstance;

    [SerializeField]
    private Vector2Int position;

    [SerializeField]
    private int rotationSteps;

    public InventoryItemInstance ItemInstance =>
        itemInstance;

    public ItemDefinition ItemDefinition =>
        itemInstance != null
            ? itemInstance.Definition
            : null;

    public Vector2Int Position =>
        position;

    public int RotationSteps =>
        rotationSteps;

    public int Quantity =>
        itemInstance != null
            ? itemInstance.Quantity
            : 0;

    public bool IsStackable =>
        itemInstance != null &&
        itemInstance.IsStackable;

    public int MaxStackSize =>
        itemInstance != null
            ? itemInstance.MaxStackSize
            : 1;

    public bool HasRoomInStack =>
        itemInstance != null &&
        itemInstance.HasRoomInStack;

    public bool IsEmpty =>
        itemInstance == null ||
        itemInstance.IsEmpty;

    public int Width
    {
        get
        {
            if (ItemDefinition == null)
                return 1;

            return ItemDefinition.GetWidth(
                rotationSteps
            );
        }
    }

    public int Height
    {
        get
        {
            if (ItemDefinition == null)
                return 1;

            return ItemDefinition.GetHeight(
                rotationSteps
            );
        }
    }

    public PlacedInventoryItem(
        ItemDefinition itemDefinition,
        Vector2Int position,
        int rotationSteps,
        int quantity = 1)
        : this(
            new InventoryItemInstance(
                itemDefinition,
                quantity
            ),
            position,
            rotationSteps
        )
    {
    }

    public PlacedInventoryItem(
        InventoryItemInstance itemInstance,
        Vector2Int position,
        int rotationSteps)
    {
        this.itemInstance =
            itemInstance;

        this.position =
            position;

        this.rotationSteps =
            global::ItemDefinition.NormalizeRotationSteps(
                rotationSteps
            );

        this.itemInstance?.EnsureValid();
    }

    public void SetPosition(
        Vector2Int newPosition)
    {
        position =
            newPosition;
    }

    public void RotateCounterClockwise()
    {
        rotationSteps =
            global::ItemDefinition.NormalizeRotationSteps(
                rotationSteps - 1
            );
    }

    public void SetRotationSteps(
        int newRotationSteps)
    {
        rotationSteps =
            global::ItemDefinition.NormalizeRotationSteps(
                newRotationSteps
            );
    }

    public List<Vector2Int> GetOccupiedCellsAt(
        Vector2Int origin)
    {
        List<Vector2Int> occupiedCells =
            new List<Vector2Int>();

        if (ItemDefinition == null)
            return occupiedCells;

        IReadOnlyList<Vector2Int>
            rotatedCells =
                ItemDefinition.GetOccupiedCells(
                    rotationSteps
                );

        for (int i = 0;
             i < rotatedCells.Count;
             i++)
        {
            occupiedCells.Add(
                origin + rotatedCells[i]
            );
        }

        return occupiedCells;
    }

    public bool OccupiesCellAt(
        Vector2Int cellCoordinate,
        Vector2Int origin)
    {
        if (ItemDefinition == null)
            return false;

        IReadOnlyList<Vector2Int>
            rotatedCells =
                ItemDefinition.GetOccupiedCells(
                    rotationSteps
                );

        for (int i = 0;
             i < rotatedCells.Count;
             i++)
        {
            if (origin +
                rotatedCells[i] ==
                cellCoordinate)
            {
                return true;
            }
        }

        return false;
    }
}