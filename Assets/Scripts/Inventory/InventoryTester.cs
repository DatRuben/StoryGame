using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [SerializeField] private ItemData testItem;

    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 6;

    [SerializeField] private int placeX = 0;
    [SerializeField] private int placeY = 0;
    [SerializeField] private bool rotated = false;

    private void Start()
    {
        InventoryGrid grid =
            new InventoryGrid(gridWidth, gridHeight);

        bool placed =
            grid.PlaceItem(testItem, placeX, placeY, rotated);

        Debug.Log("Placed Item: " + placed);

        grid.DebugPrintGrid();
    }
}