using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeldItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerHolding playerHolding;

    [Header("Layout")]
    [SerializeField] private Vector2 topLeftOffset = new Vector2(40f, -190f);
    [SerializeField] private float cardSpacing = 12f;

    [Header("Card Sizes")]
    [SerializeField] private Vector2 oneHandCardSize = new Vector2(140f, 180f);
    [SerializeField] private Vector2 twoHandCardSize = new Vector2(292f, 180f);

    [Header("Colors")]
    [SerializeField] private Color cardColor = new Color(0.45f, 0.45f, 0.45f, 0.75f);
    [SerializeField] private Color emptyCardColor = new Color(0.2f, 0.2f, 0.2f, 0.45f);
    [SerializeField] private Color cardBorderColor = Color.black;
    [SerializeField] private Color itemIconFallbackColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color emptyTextColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("Text")]
    [SerializeField] private int itemNameFontSize = 18;
    [SerializeField] private int controlsFontSize = 15;

    [Header("Holding Symbols")]
    [SerializeField] private string leftHandSymbol = "L";
    [SerializeField] private string rightHandSymbol = "R";
    [SerializeField] private string twoHandSymbol = "L + R";
    [SerializeField] private string mouthSymbol = "Mouth";
    [SerializeField] private string drawnWeaponSymbol = "Weapon";

    [Header("Border")]
    [SerializeField] private float borderThickness = 4f;

    private RectTransform root;
    private CanvasGroup canvasGroup;

    private HoldingCardUI leftCard;
    private HoldingCardUI rightCard;
    private HoldingCardUI twoHandCard;
    private HoldingCardUI mouthCard;

    private void Awake()
    {
        root = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (root != null)
        {
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = topLeftOffset;
        }

        BuildUI();
        HideAllCards();
        Hide();
    }

    private void Start()
    {
        if (playerInventory != null)
        {
            playerInventory.OnHeldItemChanged += Refresh;
            playerInventory.OnEquipmentChanged += Refresh;
        }

        if (playerHolding != null)
        {
            playerHolding.OnHoldingChanged += Refresh;
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnHeldItemChanged -= Refresh;
            playerInventory.OnEquipmentChanged -= Refresh;
        }

        if (playerHolding != null)
        {
            playerHolding.OnHoldingChanged -= Refresh;
        }
    }

    private void BuildUI()
    {
        leftCard =
            CreateHoldingCard(
                "LeftHandCard",
                new Vector2(0f, 0f),
                oneHandCardSize
            );

        rightCard =
            CreateHoldingCard(
                "RightHandCard",
                new Vector2(oneHandCardSize.x + cardSpacing, 0f),
                oneHandCardSize
            );

        twoHandCard =
            CreateHoldingCard(
                "BothHandsCard",
                Vector2.zero,
                twoHandCardSize
            );

        mouthCard =
            CreateHoldingCard(
                "MouthCard",
                new Vector2(0f, -oneHandCardSize.y - cardSpacing),
                oneHandCardSize
            );
    }

    private HoldingCardUI CreateHoldingCard(
        string name,
        Vector2 position,
        Vector2 size)
    {
        GameObject cardObject =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

        cardObject.transform.SetParent(transform, false);

        RectTransform cardRect =
            cardObject.GetComponent<RectTransform>();

        cardRect.anchorMin = new Vector2(0f, 1f);
        cardRect.anchorMax = new Vector2(0f, 1f);
        cardRect.pivot = new Vector2(0f, 1f);
        cardRect.anchoredPosition = position;
        cardRect.sizeDelta = size;

        Image cardImage =
            cardObject.GetComponent<Image>();

        cardImage.color = cardColor;
        cardImage.raycastTarget = false;

        CreateBorder(
            cardRect,
            cardBorderColor,
            borderThickness
        );

        GameObject iconObject =
            new GameObject(
                "ItemIcon",
                typeof(RectTransform),
                typeof(Image)
            );

        iconObject.transform.SetParent(cardObject.transform, false);

        RectTransform iconRect =
            iconObject.GetComponent<RectTransform>();

        iconRect.anchorMin = new Vector2(0.5f, 1f);
        iconRect.anchorMax = new Vector2(0.5f, 1f);
        iconRect.pivot = new Vector2(0.5f, 1f);
        iconRect.anchoredPosition = new Vector2(0f, -24f);
        iconRect.sizeDelta = new Vector2(72f, 72f);

        Image iconImage =
            iconObject.GetComponent<Image>();

        iconImage.color = itemIconFallbackColor;
        iconImage.raycastTarget = false;

        TextMeshProUGUI itemNameText =
            CreateText(
                "ItemName",
                cardObject.transform,
                new Vector2(0f, -102f),
                new Vector2(size.x, 26f),
                itemNameFontSize,
                TextAlignmentOptions.Center
            );

        TextMeshProUGUI slotText =
            CreateText(
                "HoldingSlotText",
                cardObject.transform,
                new Vector2(0f, -130f),
                new Vector2(size.x, 24f),
                itemNameFontSize,
                TextAlignmentOptions.Center
            );

        TextMeshProUGUI controlsText =
            CreateText(
                "ControlsText",
                cardObject.transform,
                new Vector2(0f, -154f),
                new Vector2(size.x, 42f),
                controlsFontSize,
                TextAlignmentOptions.Center
            );

        return new HoldingCardUI(
            cardObject,
            cardRect,
            cardImage,
            iconImage,
            itemNameText,
            slotText,
            controlsText,
            cardColor,
            emptyCardColor,
            itemIconFallbackColor,
            textColor,
            emptyTextColor
        );
    }

    private void Refresh()
    {
        if (playerHolding == null)
        {
            HideAllCards();
            Hide();
            return;
        }

        Show();

        RefreshHandCards();
        RefreshMouthCard();
    }

    private void RefreshHandCards()
    {
        ItemData bothHandsItem =
            GetBothHandsItemToShow();

        if (bothHandsItem != null)
        {
            leftCard.SetVisible(false);
            rightCard.SetVisible(false);

            twoHandCard.SetVisible(true);
            twoHandCard.SetPosition(Vector2.zero);
            twoHandCard.SetSize(twoHandCardSize);

            string symbol =
                bothHandsItem == GetDrawnWeapon()
                ? drawnWeaponSymbol
                : twoHandSymbol;

            twoHandCard.SetItem(
                bothHandsItem,
                symbol,
                GetControlsText(bothHandsItem)
            );

            return;
        }

        twoHandCard.SetVisible(false);

        ItemData leftItem =
            playerHolding.LeftHandItem;

        ItemData rightItem =
            playerHolding.RightHandItem;

        string leftSymbol =
            leftHandSymbol;

        string rightSymbol =
            rightHandSymbol;

        ItemData drawnWeapon =
            GetDrawnWeapon();

        ItemData mouseHeldItem =
            GetMouseHeldItemThatCountsAsHeld();

        if (drawnWeapon != null &&
            drawnWeapon.handUsage == ItemHandUsage.OneHanded)
        {
            if (rightItem == null)
            {
                rightItem = drawnWeapon;
                rightSymbol = drawnWeaponSymbol;
            }
            else if (leftItem == null)
            {
                leftItem = drawnWeapon;
                leftSymbol = drawnWeaponSymbol;
            }
        }

        if (mouseHeldItem != null &&
            mouseHeldItem.handUsage == ItemHandUsage.OneHanded)
        {
            if (rightItem == null)
            {
                rightItem = mouseHeldItem;
                rightSymbol = rightHandSymbol;
            }
            else if (leftItem == null)
            {
                leftItem = mouseHeldItem;
                leftSymbol = leftHandSymbol;
            }
        }

        leftCard.SetVisible(true);
        rightCard.SetVisible(true);

        leftCard.SetPosition(Vector2.zero);
        leftCard.SetSize(oneHandCardSize);

        rightCard.SetPosition(
            new Vector2(
                oneHandCardSize.x + cardSpacing,
                0f
            )
        );

        rightCard.SetSize(oneHandCardSize);

        if (leftItem == null)
        {
            leftCard.SetEmpty(leftHandSymbol);
        }
        else
        {
            leftCard.SetItem(
                leftItem,
                leftSymbol,
                GetControlsText(leftItem)
            );
        }

        if (rightItem == null)
        {
            rightCard.SetEmpty(rightHandSymbol);
        }
        else
        {
            rightCard.SetItem(
                rightItem,
                rightSymbol,
                GetControlsText(rightItem)
            );
        }
    }

    private void RefreshMouthCard()
    {
        bool shouldShowMouthSlot =
            playerHolding.CanHoldItemInMouth;

        mouthCard.SetVisible(shouldShowMouthSlot);

        if (!shouldShowMouthSlot)
            return;

        ItemData mouthItem =
            playerHolding.MouthItem;

        ItemData mouseHeldItem =
            GetMouseHeldItemThatCountsAsHeld();

        bool handsAreFull =
            GetVisibleLeftHandItem() != null &&
            GetVisibleRightHandItem() != null;

        if (mouthItem == null &&
            mouseHeldItem != null &&
            handsAreFull)
        {
            mouthItem = mouseHeldItem;
        }

        Vector2 mouthCardSize =
            mouthItem != null &&
            mouthItem.handUsage == ItemHandUsage.TwoHanded
            ? twoHandCardSize
            : oneHandCardSize;

        mouthCard.SetPosition(
            new Vector2(
                0f,
                -oneHandCardSize.y - cardSpacing
            )
        );

        mouthCard.SetSize(mouthCardSize);

        if (mouthItem == null)
        {
            mouthCard.SetEmpty(mouthSymbol);
            return;
        }

        mouthCard.SetItem(
            mouthItem,
            mouthSymbol,
            GetControlsText(mouthItem)
        );
    }

    private ItemData GetBothHandsItemToShow()
    {
        ItemData mouseHeldItem =
            GetMouseHeldItemThatCountsAsHeld();

        if (mouseHeldItem != null &&
            mouseHeldItem.handUsage == ItemHandUsage.TwoHanded)
        {
            return mouseHeldItem;
        }

        if (playerHolding.TwoHandedItem != null)
            return playerHolding.TwoHandedItem;

        ItemData drawnWeapon =
            GetDrawnWeapon();

        if (drawnWeapon != null &&
            drawnWeapon.handUsage == ItemHandUsage.TwoHanded)
        {
            return drawnWeapon;
        }

        return null;
    }

    private ItemData GetVisibleLeftHandItem()
    {
        if (GetBothHandsItemToShow() != null)
            return null;

        ItemData leftItem =
            playerHolding.LeftHandItem;

        ItemData rightItem =
            playerHolding.RightHandItem;

        ItemData drawnWeapon =
            GetDrawnWeapon();

        ItemData mouseHeldItem =
            GetMouseHeldItemThatCountsAsHeld();

        if (drawnWeapon != null &&
            drawnWeapon.handUsage == ItemHandUsage.OneHanded)
        {
            if (rightItem != null &&
                leftItem == null)
            {
                leftItem = drawnWeapon;
            }
        }

        if (mouseHeldItem != null &&
            mouseHeldItem.handUsage == ItemHandUsage.OneHanded)
        {
            if (rightItem != null &&
                leftItem == null)
            {
                leftItem = mouseHeldItem;
            }
        }

        return leftItem;
    }

    private ItemData GetVisibleRightHandItem()
    {
        if (GetBothHandsItemToShow() != null)
            return null;

        ItemData rightItem =
            playerHolding.RightHandItem;

        ItemData drawnWeapon =
            GetDrawnWeapon();

        ItemData mouseHeldItem =
            GetMouseHeldItemThatCountsAsHeld();

        if (drawnWeapon != null &&
            drawnWeapon.handUsage == ItemHandUsage.OneHanded &&
            rightItem == null)
        {
            rightItem = drawnWeapon;
        }

        if (mouseHeldItem != null &&
            mouseHeldItem.handUsage == ItemHandUsage.OneHanded &&
            rightItem == null)
        {
            rightItem = mouseHeldItem;
        }

        return rightItem;
    }

    private ItemData GetMouseHeldItemThatCountsAsHeld()
    {
        if (playerInventory == null)
            return null;

        if (!playerInventory.MouseHeldItemCountsAsHeld)
            return null;

        if (playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemData == null)
        {
            return null;
        }

        return playerInventory.HeldItem.ItemData;
    }

    private ItemData GetDrawnWeapon()
    {
        if (playerInventory == null)
            return null;

        if (!playerInventory.IsWeaponDrawn)
            return null;

        return playerInventory.WeaponSlotItem;
    }

    private string GetControlsText(ItemData item)
    {
        if (item == null)
            return "";

        ItemData drawnWeapon =
            GetDrawnWeapon();

        if (item == drawnWeapon &&
            !string.IsNullOrWhiteSpace(item.weaponControlsText))
        {
            return item.weaponControlsText;
        }

        return item.heldControlsText;
    }

    private void HideAllCards()
    {
        if (leftCard != null)
            leftCard.SetVisible(false);

        if (rightCard != null)
            rightCard.SetVisible(false);

        if (twoHandCard != null)
            twoHandCard.SetVisible(false);

        if (mouthCard != null)
            mouthCard.SetVisible(false);
    }

    private TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        Vector2 anchoredPosition,
        Vector2 size,
        int fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(parent, false);

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = anchoredPosition;
        textRect.sizeDelta = size;

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();

        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.text = "";

        return text;
    }

    private void Show()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Hide()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void CreateBorder(
        RectTransform parent,
        Color color,
        float thickness)
    {
        CreateBorderPiece(
            parent,
            "BorderTop",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            Vector2.zero,
            new Vector2(0f, thickness),
            color
        );

        CreateBorderPiece(
            parent,
            "BorderBottom",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            Vector2.zero,
            new Vector2(0f, thickness),
            color
        );

        CreateBorderPiece(
            parent,
            "BorderLeft",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            Vector2.zero,
            new Vector2(thickness, 0f),
            color
        );

        CreateBorderPiece(
            parent,
            "BorderRight",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0.5f),
            Vector2.zero,
            new Vector2(thickness, 0f),
            color
        );
    }

    private void CreateBorderPiece(
        RectTransform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color)
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

        image.color = color;
        image.raycastTarget = false;
    }

    private class HoldingCardUI
    {
        private readonly GameObject rootObject;
        private readonly RectTransform rootRect;
        private readonly Image cardImage;
        private readonly Image iconImage;
        private readonly TextMeshProUGUI itemNameText;
        private readonly TextMeshProUGUI slotText;
        private readonly TextMeshProUGUI controlsText;

        private readonly Color filledCardColor;
        private readonly Color emptyCardColor;
        private readonly Color fallbackIconColor;
        private readonly Color filledTextColor;
        private readonly Color emptyTextColor;

        public HoldingCardUI(
            GameObject rootObject,
            RectTransform rootRect,
            Image cardImage,
            Image iconImage,
            TextMeshProUGUI itemNameText,
            TextMeshProUGUI slotText,
            TextMeshProUGUI controlsText,
            Color filledCardColor,
            Color emptyCardColor,
            Color fallbackIconColor,
            Color filledTextColor,
            Color emptyTextColor)
        {
            this.rootObject = rootObject;
            this.rootRect = rootRect;
            this.cardImage = cardImage;
            this.iconImage = iconImage;
            this.itemNameText = itemNameText;
            this.slotText = slotText;
            this.controlsText = controlsText;
            this.filledCardColor = filledCardColor;
            this.emptyCardColor = emptyCardColor;
            this.fallbackIconColor = fallbackIconColor;
            this.filledTextColor = filledTextColor;
            this.emptyTextColor = emptyTextColor;
        }

        public void SetVisible(bool visible)
        {
            rootObject.SetActive(visible);
        }

        public void SetPosition(Vector2 position)
        {
            rootRect.anchoredPosition = position;
        }

        public void SetSize(Vector2 size)
        {
            rootRect.sizeDelta = size;

            itemNameText.rectTransform.sizeDelta =
                new Vector2(size.x, 26f);

            slotText.rectTransform.sizeDelta =
                new Vector2(size.x, 24f);

            controlsText.rectTransform.sizeDelta =
                new Vector2(size.x, 42f);
        }

        public void SetEmpty(string holdingSymbol)
        {
            cardImage.color = emptyCardColor;

            itemNameText.text = "Empty";
            slotText.text = holdingSymbol;
            controlsText.text = "";

            itemNameText.color = emptyTextColor;
            slotText.color = emptyTextColor;
            controlsText.color = emptyTextColor;

            iconImage.sprite = null;
            iconImage.color = fallbackIconColor;

            SetVisible(true);
        }

        public void SetItem(
            ItemData item,
            string holdingSymbol,
            string controls)
        {
            if (item == null)
            {
                SetEmpty(holdingSymbol);
                return;
            }

            cardImage.color = filledCardColor;

            itemNameText.text = item.itemName;
            slotText.text = holdingSymbol;
            controlsText.text = controls;

            itemNameText.color = filledTextColor;
            slotText.color = filledTextColor;
            controlsText.color = filledTextColor;

            if (item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.color = fallbackIconColor;
            }

            SetVisible(true);
        }
    }
}