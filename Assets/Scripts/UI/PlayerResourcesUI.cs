using UnityEngine;
using UnityEngine.UI;

public class PlayerResourcesUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerResources playerResources;

    [Header("Layout")]
    [SerializeField] private Vector2 topLeftOffset = new Vector2(40f, -40f);
    [SerializeField] private Vector2 barSize = new Vector2(620f, 36f);
    [SerializeField] private float barSpacing = 10f;

    [Header("Colors")]
    [SerializeField] private Color healthColor = Color.red;
    [SerializeField] private Color staminaColor = Color.green;
    [SerializeField] private Color AetherColor = Color.cyan;
    [SerializeField] private Color barBackgroundColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] private Color borderColor = Color.black;

    [Header("Border")]
    [SerializeField] private float borderThickness = 4f;

    private RectTransform root;

    private RectTransform healthFill;
    private RectTransform staminaFill;
    private RectTransform AetherFill;

    private void Awake()
    {
        root = GetComponent<RectTransform>();

        if (root != null)
        {
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = topLeftOffset;
        }

        BuildUI();
    }

    private void Start()
    {
        if (playerResources != null)
        {
            playerResources.OnResourcesChanged += Refresh;
        }

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        if (playerResources != null)
        {
            playerResources.OnResourcesChanged -= Refresh;
        }
    }

    private void BuildUI()
    {
        float y = 0f;

        healthFill = CreateBar("HealthBar", y, healthColor);

        y -= barSize.y + barSpacing;

        staminaFill = CreateBar("StaminaBar", y, staminaColor);

        y -= barSize.y + barSpacing;

        AetherFill = CreateBar("AetherBar", y, AetherColor);
    }

    private RectTransform CreateBar(
        string name,
        float y,
        Color fillColor)
    {
        GameObject barObject =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

        barObject.transform.SetParent(transform, false);

        RectTransform barRect =
            barObject.GetComponent<RectTransform>();

        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 1f);
        barRect.anchoredPosition = new Vector2(0f, y);
        barRect.sizeDelta = barSize;

        Image background =
            barObject.GetComponent<Image>();

        background.color = barBackgroundColor;
        background.raycastTarget = false;

        CreateBorder(barRect);

        GameObject fillObject =
            new GameObject(
                "Fill",
                typeof(RectTransform),
                typeof(Image)
            );

        fillObject.transform.SetParent(barObject.transform, false);

        RectTransform fillRect =
            fillObject.GetComponent<RectTransform>();

        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(barSize.x, 0f);

        Image fillImage =
            fillObject.GetComponent<Image>();

        fillImage.color = fillColor;
        fillImage.raycastTarget = false;

        return fillRect;
    }

    private void Refresh()
    {
        if (playerResources == null)
        {
            SetFillPercent(healthFill, 1f);
            SetFillPercent(staminaFill, 1f);
            SetFillPercent(AetherFill, 1f);
            return;
        }

        SetFillPercent(healthFill, playerResources.HealthPercent);
        SetFillPercent(staminaFill, playerResources.StaminaPercent);
        SetFillPercent(AetherFill, playerResources.AetherPercent);
    }

    private void SetFillPercent(
        RectTransform fill,
        float percent)
    {
        if (fill == null)
            return;

        percent = Mathf.Clamp01(percent);

        fill.sizeDelta =
            new Vector2(
                barSize.x * percent,
                fill.sizeDelta.y
            );
    }

    private void CreateBorder(RectTransform parent)
    {
        CreateBorderPiece(
            parent,
            "BorderTop",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, 0f),
            new Vector2(0f, borderThickness)
        );

        CreateBorderPiece(
            parent,
            "BorderBottom",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, borderThickness)
        );

        CreateBorderPiece(
            parent,
            "BorderLeft",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(borderThickness, 0f)
        );

        CreateBorderPiece(
            parent,
            "BorderRight",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(borderThickness, 0f)
        );
    }

    public void BindPlayer(PlayerResources newPlayerResources)
    {
        if (playerResources != null)
            playerResources.OnResourcesChanged -= Refresh;

        playerResources = newPlayerResources;

        if (playerResources != null)
            playerResources.OnResourcesChanged += Refresh;

        Refresh();
    }

    private void CreateBorderPiece(
        RectTransform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject borderObject =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

        borderObject.transform.SetParent(parent, false);

        RectTransform rect =
            borderObject.GetComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image =
            borderObject.GetComponent<Image>();

        image.color = borderColor;
        image.raycastTarget = false;
    }
}