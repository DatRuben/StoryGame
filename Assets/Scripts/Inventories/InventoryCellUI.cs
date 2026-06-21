using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryCellUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI quantityText;

    private Vector2Int coordinate;

    private Action<Vector2Int> onClicked;
    private Action<Vector2Int> onRightClicked;
    private Action<Vector2Int> onPointerEntered;
    private Action<Vector2Int> onPointerExited;
    private Action<Vector2Int> onPointerDown;
    private Action<Vector2Int> onPointerUp;

    public void Initialize(
        Vector2Int coordinate,
        Color startColor,
        Action<Vector2Int> onClicked,
        Action<Vector2Int> onPointerEntered,
        Action<Vector2Int> onPointerExited,
        Action<Vector2Int> onPointerDown = null,
        Action<Vector2Int> onPointerUp = null,
        Action<Vector2Int> onRightClicked = null)
    {
        this.coordinate = coordinate;
        this.onClicked = onClicked;
        this.onRightClicked = onRightClicked;
        this.onPointerEntered = onPointerEntered;
        this.onPointerExited = onPointerExited;
        this.onPointerDown = onPointerDown;
        this.onPointerUp = onPointerUp;

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

        if (quantityText == null)
            quantityText = GetComponentInChildren<TextMeshProUGUI>(true);

        SetQuantityText("");
    }

    public void SetColor(Color color)
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image != null)
            image.color = color;
    }

    public void SetQuantityText(string text)
    {
        if (quantityText == null)
            quantityText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (quantityText == null)
            return;

        quantityText.text = text;

        bool hasText =
            !string.IsNullOrEmpty(text);

        quantityText.gameObject.SetActive(hasText);

        if (hasText)
        {
            quantityText.transform.SetAsLastSibling();

            quantityText.raycastTarget = false;

            Canvas quantityCanvas =
                quantityText.GetComponent<Canvas>();

            if (quantityCanvas == null)
                quantityCanvas = quantityText.gameObject.AddComponent<Canvas>();

            quantityCanvas.overrideSorting = true;
            quantityCanvas.sortingOrder = 10;
        }
    }

    private void Click()
    {
        onClicked?.Invoke(coordinate);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        onRightClicked?.Invoke(coordinate);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEntered?.Invoke(coordinate);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExited?.Invoke(coordinate);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        onPointerDown?.Invoke(coordinate);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        onPointerUp?.Invoke(coordinate);
    }
}