using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentSlotUI :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerEquipment playerEquipment;

    [SerializeField]
    private InventoryInteractionController
        interactionController;

    [SerializeField]
    private Image slotImage;

    [SerializeField]
    private TextMeshProUGUI slotText;

    [SerializeField]
    private Button button;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("Slot")]
    [SerializeField]
    private EquipmentSlotType equipmentSlotType;

    [SerializeField]
    [Range(0, PlayerEquipment.ArmAttachmentCount - 1)]
    private int slotIndex;

    [Header("Visibility")]
    [SerializeField]
    private bool onlyShowWhenInventoryOpen = true;

    [Header("Text")]
    [SerializeField]
    private string emptyText = "";

    [SerializeField]
    private string equipText = "";

    [SerializeField]
    private string swapText = "";

    [SerializeField]
    private string cannotEquipText =
        "Cannot Equip";

    [SerializeField]
    private string turretSuffix =
        " + Turret";

    [Header("Colors")]
    [SerializeField]
    private Color emptyColor =
        new Color(0f, 0f, 0f, 0.35f);

    [SerializeField]
    private Color equippedColor =
        new Color(1f, 1f, 1f, 0.85f);

    [SerializeField]
    private Color turretColor =
        new Color(0.8f, 0.55f, 1f, 0.9f);

    [SerializeField]
    private Color canEquipColor =
        new Color(0.2f, 1f, 0.2f, 0.85f);

    [SerializeField]
    private Color invalidColor =
        new Color(1f, 0.2f, 0.2f, 0.85f);

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

    private void OnValidate()
    {
        if (equipmentSlotType !=
            EquipmentSlotType.ArmAttachment)
        {
            slotIndex = 0;
            return;
        }

        slotIndex =
            Mathf.Clamp(
                slotIndex,
                0,
                PlayerEquipment.ArmAttachmentCount - 1
            );
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void BindPlayer(
        PlayerEquipment newPlayerEquipment,
        InventoryInteractionController
            newInteractionController)
    {
        Unsubscribe();

        playerEquipment =
            newPlayerEquipment;

        interactionController =
            newInteractionController;

        if (isActiveAndEnabled)
            Subscribe();

        Refresh();
    }

    private void Subscribe()
    {
        if (playerEquipment != null)
        {
            playerEquipment.OnEquipmentChanged +=
                Refresh;
        }

        if (interactionController != null)
        {
            interactionController.Changed +=
                Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (playerEquipment != null)
        {
            playerEquipment.OnEquipmentChanged -=
                Refresh;
        }

        if (interactionController != null)
        {
            interactionController.Changed -=
                Refresh;
        }
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void OnSlotClicked()
    {
        if (playerEquipment == null ||
            interactionController == null)
        {
            return;
        }

        if (interactionController.HasSelection)
        {
            interactionController
                .TryEquipSelectedItem(
                    equipmentSlotType,
                    slotIndex
                );

            return;
        }

        interactionController
            .TryTakeEquipmentFromSlot(
                equipmentSlotType,
                slotIndex
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

        InventoryItemInstance equippedItem =
            playerEquipment.GetEquippedItem(
                equipmentSlotType,
                slotIndex
            );

        if (interactionController != null &&
            interactionController.HasSelection)
        {
            RefreshForSelection(
                equippedItem
            );

            return;
        }

        if (equippedItem == null ||
            equippedItem.Definition == null)
        {
            SetSlot(
                GetEmptyText(),
                emptyColor
            );

            return;
        }

        ItemDefinition definition =
            equippedItem.Definition;

        string text =
            string.IsNullOrWhiteSpace(
                definition.itemName)
                ? GetSlotName()
                : definition.itemName;

        Color color =
            equippedColor;

        if (equipmentSlotType ==
                EquipmentSlotType.Saddle &&
            definition.hasManualSaddleTurret)
        {
            text += turretSuffix;
            color = turretColor;
        }

        SetSlot(
            text,
            color
        );
    }

    private void RefreshForSelection(
        InventoryItemInstance equippedItem)
    {
        bool canEquip =
            interactionController
                .CanEquipSelectedItem(
                    equipmentSlotType,
                    slotIndex
                );

        if (!canEquip)
        {
            SetSlot(
                cannotEquipText,
                invalidColor
            );

            return;
        }

        SetSlot(
            equippedItem == null
                ? GetEquipText()
                : GetSwapText(),
            canEquipColor
        );
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

            case EquipmentSlotType.Gauntlets:
                return "Gauntlets";

            case EquipmentSlotType.ArmAttachment:
                return
                    "Arm Attachment " +
                    (slotIndex + 1);

            default:
                return "Equipment";
        }
    }

    private string GetEmptyText()
    {
        if (!string.IsNullOrWhiteSpace(
            emptyText))
        {
            return emptyText;
        }

        return GetSlotName();
    }

    private string GetEquipText()
    {
        if (!string.IsNullOrWhiteSpace(
            equipText))
        {
            return equipText;
        }

        return "Equip " +
               GetSlotName();
    }

    private string GetSwapText()
    {
        if (!string.IsNullOrWhiteSpace(
            swapText))
        {
            return swapText;
        }

        return "Swap " +
               GetSlotName();
    }

    private void SetSlot(
        string text,
        Color color)
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
            InventoryMenuController
                .IsInventoryOpen;

        canvasGroup.alpha =
            shouldShow ? 1f : 0f;

        canvasGroup.interactable =
            shouldShow;

        canvasGroup.blocksRaycasts =
            shouldShow;
    }
}