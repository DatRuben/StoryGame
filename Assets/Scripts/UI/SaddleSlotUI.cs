using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaddleSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerEquipment playerEquipment;

    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visibility")]
    [SerializeField] private bool onlyShowWhenInventoryOpen = true;

    [Header("Text")]
    [SerializeField] private string emptyText = "Saddle";
    [SerializeField] private string equipText = "Equip Saddle";
    [SerializeField] private string swapText = "Swap Saddle";
    [SerializeField] private string cannotEquipText = "Cannot Equip";
    [SerializeField] private string turretSuffix = " + Turret";

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color equippedColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color turretColor = new Color(0.8f, 0.55f, 1f, 0.9f);
    [SerializeField] private Color canEquipColor = new Color(0.2f, 1f, 0.2f, 0.85f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.85f);

    private void Awake()
    {
        if (slotImage == null)
            slotImage = GetComponent<Image>();

        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClicked);
        }
    }

    private void Start()
    {
        if (playerInventory != null)
            playerInventory.OnHeldItemChanged += Refresh;

        if (playerEquipment != null)
            playerEquipment.OnEquipmentChanged += Refresh;

        Refresh();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
            playerInventory.OnHeldItemChanged -= Refresh;

        if (playerEquipment != null)
            playerEquipment.OnEquipmentChanged -= Refresh;
    }

    private void OnSlotClicked()
    {
        if (playerInventory == null ||
            playerEquipment == null)
        {
            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            TryEquipHeldSaddle();
            Refresh();
            return;
        }

        TryPickUpEquippedSaddle();
        Refresh();
    }

    private void TryEquipHeldSaddle()
    {
        if (playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemData == null)
        {
            return;
        }

        ItemData heldItem =
            playerInventory.HeldItem.ItemData;

        if (!playerEquipment.CanEquipSaddle(heldItem))
        {
            Debug.Log("Held item cannot be equipped as a saddle.");
            return;
        }

        bool equipped =
            playerEquipment.TryEquipSaddle(
                heldItem,
                out ItemData replacedSaddle
            );

        if (!equipped)
            return;

        if (replacedSaddle != null)
        {
            playerInventory.SetMouseHeldItemFromExternal(
                replacedSaddle,
                0,
                true
            );
        }
        else
        {
            playerInventory.ClearHeldItemAfterExternalMove();
        }
    }

    private void TryPickUpEquippedSaddle()
    {
        ItemData removedSaddle =
            playerEquipment.UnequipSaddle();

        if (removedSaddle == null)
            return;

        playerInventory.SetMouseHeldItemFromExternal(
            removedSaddle,
            0,
            true
        );
    }

    private void Refresh()
    {
        if (playerEquipment == null)
        {
            SetSlot(emptyText, emptyColor);
            return;
        }

        if (playerInventory != null &&
            playerInventory.IsHoldingItem)
        {
            RefreshWhileHoldingItem();
            return;
        }

        ItemData saddle =
            playerEquipment.EquippedSaddle;

        if (saddle == null)
        {
            SetSlot(emptyText, emptyColor);
            return;
        }

        string text =
            string.IsNullOrWhiteSpace(saddle.itemName)
                ? "Saddle"
                : saddle.itemName;

        Color color =
            equippedColor;

        if (saddle.hasManualSaddleTurret)
        {
            text += turretSuffix;
            color = turretColor;
        }

        SetSlot(text, color);
    }

    private void RefreshWhileHoldingItem()
    {
        if (playerInventory == null ||
            playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemData == null)
        {
            return;
        }

        ItemData heldItem =
            playerInventory.HeldItem.ItemData;

        bool canEquip =
            playerEquipment.CanEquipSaddle(heldItem);

        if (!canEquip)
        {
            SetSlot(cannotEquipText, invalidColor);
            return;
        }

        if (playerEquipment.EquippedSaddle == null)
        {
            SetSlot(equipText, canEquipColor);
        }
        else
        {
            SetSlot(swapText, canEquipColor);
        }
    }

    private void SetSlot(string text, Color color)
    {
        if (slotImage != null)
            slotImage.color = color;

        if (slotText != null)
            slotText.text = text;
    }

    private void UpdateVisibility()
    {
        if (canvasGroup == null)
            return;

        bool shouldShow =
            !onlyShowWhenInventoryOpen ||
            InventoryMenuController.IsInventoryOpen;

        canvasGroup.alpha = shouldShow ? 1f : 0f;
        canvasGroup.interactable = shouldShow;
        canvasGroup.blocksRaycasts = shouldShow;
    }
}