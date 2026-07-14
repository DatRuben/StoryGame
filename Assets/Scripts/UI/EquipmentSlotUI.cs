using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerEquipment playerEquipment;

    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Slot")]
    [SerializeField] private EquipmentSlotType equipmentSlotType;

    [Header("Visibility")]
    [SerializeField] private bool onlyShowWhenInventoryOpen = true;

    [Header("Text")]
    [SerializeField] private string emptyText = "";
    [SerializeField] private string equipText = "";
    [SerializeField] private string swapText = "";
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
            TryEquipHeldItem();
            Refresh();
            return;
        }

        TryPickUpEquippedItem();
        Refresh();
    }

    private void TryEquipHeldItem()
    {
        if (playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemDefinition == null)
        {
            return;
        }

        ItemDefinition heldItem =
            playerInventory.HeldItem.ItemDefinition;

        if (!playerEquipment.CanEquipItemToSlot(
                heldItem,
                equipmentSlotType))
        {
            Debug.Log(
                "Held item cannot be equipped to " +
                GetSlotName() +
                "."
            );

            return;
        }

        bool equipped =
            playerEquipment.TryEquipItemToSlot(
                heldItem,
                equipmentSlotType,
                out ItemDefinition replacedItem
            );

        if (!equipped)
            return;

        if (replacedItem != null)
        {
            playerInventory.SetMouseHeldItemFromExternal(
                replacedItem,
                0,
                true
            );
        }
        else
        {
            playerInventory.ClearHeldItemAfterExternalMove();
        }
    }

    private void TryPickUpEquippedItem()
    {
        ItemDefinition removedItem =
            playerEquipment.UnequipSlot(
                equipmentSlotType
            );

        if (removedItem == null)
            return;

        playerInventory.SetMouseHeldItemFromExternal(
            removedItem,
            0,
            true
        );
    }

    private void Refresh()
    {
        if (playerEquipment == null)
        {
            SetSlot(
                GetEmptyText(),
                emptyColor
            );

            return;
        }

        if (playerInventory != null &&
            playerInventory.IsHoldingItem)
        {
            RefreshWhileHoldingItem();
            return;
        }

        ItemDefinition equippedItem =
            playerEquipment.GetEquippedItem(
                equipmentSlotType
            );

        if (equippedItem == null)
        {
            SetSlot(
                GetEmptyText(),
                emptyColor
            );

            return;
        }

        string text =
            string.IsNullOrWhiteSpace(equippedItem.itemName)
                ? GetSlotName()
                : equippedItem.itemName;

        Color color =
            equippedColor;

        if (equipmentSlotType == EquipmentSlotType.Saddle &&
            equippedItem.hasManualSaddleTurret)
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
            playerInventory.HeldItem.ItemDefinition == null)
        {
            return;
        }

        ItemDefinition heldItem =
            playerInventory.HeldItem.ItemDefinition;

        bool canEquip =
            playerEquipment.CanEquipItemToSlot(
                heldItem,
                equipmentSlotType
            );

        if (!canEquip)
        {
            SetSlot(
                cannotEquipText,
                invalidColor
            );

            return;
        }

        ItemDefinition equippedItem =
            playerEquipment.GetEquippedItem(
                equipmentSlotType
            );

        if (equippedItem == null)
        {
            SetSlot(
                GetEquipText(),
                canEquipColor
            );
        }
        else
        {
            SetSlot(
                GetSwapText(),
                canEquipColor
            );
        }
    }

    private string GetSlotName()
    {
        switch (equipmentSlotType)
        {
            case EquipmentSlotType.Saddle:
                return "Saddle";

            case EquipmentSlotType.Armor:
                return "Armor";

            case EquipmentSlotType.Helmet:
                return "Helmet";

            case EquipmentSlotType.Accessory:
                return "Accessory";

            default:
                return "Equipment";
        }
    }

    private string GetEmptyText()
    {
        if (!string.IsNullOrWhiteSpace(emptyText))
            return emptyText;

        return GetSlotName();
    }

    private string GetEquipText()
    {
        if (!string.IsNullOrWhiteSpace(equipText))
            return equipText;

        return "Equip " + GetSlotName();
    }

    private string GetSwapText()
    {
        if (!string.IsNullOrWhiteSpace(swapText))
            return swapText;

        return "Swap " + GetSlotName();
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