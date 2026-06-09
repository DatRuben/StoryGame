using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCellUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;

    private Vector2Int coordinate;

    private Action<Vector2Int> onClicked;
    private Action<Vector2Int> onPointerEntered;
    private Action<Vector2Int> onPointerExited;

    public void Initialize(
        Vector2Int coordinate,
        Color startColor,
        Action<Vector2Int> onClicked,
        Action<Vector2Int> onPointerEntered,
        Action<Vector2Int> onPointerExited)
    {
        this.coordinate = coordinate;
        this.onClicked = onClicked;
        this.onPointerEntered = onPointerEntered;
        this.onPointerExited = onPointerExited;

        if (image == null)
            image = GetComponent<Image>();

        if (button == null)
            button = GetComponent<Button>();

        if (image != null)
            image.color = startColor;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Click);
        }
    }

    public void SetColor(Color color)
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image != null)
            image.color = color;
    }

    private void Click()
    {
        onClicked?.Invoke(coordinate);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEntered?.Invoke(coordinate);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExited?.Invoke(coordinate);
    }
}