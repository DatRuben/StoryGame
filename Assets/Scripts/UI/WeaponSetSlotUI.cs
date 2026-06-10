using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSetSlotUI : MonoBehaviour
{
    private enum WeaponSetSlotViewType
    {
        LeftHand,
        RightHand,
        BothHands,
        Mouth,
        SaddleTurret
    }

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Slot")]
    [Tooltip("0 = Weapon Set 1, 1 = Weapon Set 2")]
    [SerializeField] private int weaponSetIndex = 0;

    [SerializeField] private WeaponSetSlotViewType slotType;

    [Header("Visibility")]
    [SerializeField] private bool onlyShowWhenInventoryOpen = true;

    [Header("Text")]
    [SerializeField] private string emptyText = "Empty";
    [SerializeField] private string cannotEquipText = "Cannot Equip";
    [SerializeField] private string equipText = "Equip";
    [SerializeField] private string swapText = "Swap";
    [SerializeField] private string activePrefix = "> ";
    [SerializeField] private string drawnSuffix = " (Drawn)";

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color weaponColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color activeWeaponColor = new Color(0.6f, 0.85f, 1f, 0.9f);
    [SerializeField] private Color drawnWeaponColor = new Color(0.25f, 0.85f, 1f, 0.95f);
    [SerializeField] private Color canEquipColor = new Color(0.2f, 1f, 0.2f, 0.85f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.85f);
    [SerializeField] private Color reservedColor = new Color(0.8f, 0.55f, 1f, 0.9f);

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

        if (playerWeaponSlots != null)
            playerWeaponSlots.OnWeaponSlotsChanged += Refresh;

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

        if (playerWeaponSlots != null)
            playerWeaponSlots.OnWeaponSlotsChanged -= Refresh;
    }

    private void OnSlotClicked()
    {
        if (playerInventory == null ||
            playerWeaponSlots == null)
        {
            return;
        }

        if (slotType == WeaponSetSlotViewType.SaddleTurret)
            return;

        if (playerInventory.IsHoldingItem)
        {
            TryEquipHeldItemToSlot();
            Refresh();
            return;
        }

        TryPickUpWeaponFromSlot();
        Refresh();
    }

    private void TryEquipHeldItemToSlot()
    {
        if (playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemData == null)
        {
            return;
        }

        WeaponEquipPoint equipPoint =
            GetEquipPoint();

        ItemData heldItem =
            playerInventory.HeldItem.ItemData;

        int heldRotationSteps =
            playerInventory.HeldItemRotationSteps;

        ItemData oldWeapon =
            playerWeaponSlots.RemoveWeaponFromSetSlot(
                weaponSetIndex,
                equipPoint
            );

        bool equipped =
            playerWeaponSlots.TryEquipWeaponToSet(
                weaponSetIndex,
                heldItem,
                equipPoint
            );

        if (equipped)
        {
            if (oldWeapon != null)
            {
                playerInventory.SetMouseHeldItemFromExternal(
                    oldWeapon,
                    0,
                    true
                );
            }
            else
            {
                playerInventory.ClearHeldItemAfterExternalMove();
            }

            return;
        }

        if (oldWeapon != null)
        {
            playerWeaponSlots.TryEquipWeaponToSet(
                weaponSetIndex,
                oldWeapon,
                equipPoint
            );
        }

        Debug.Log("Held item cannot be equipped to this weapon slot.");
    }

    private void TryPickUpWeaponFromSlot()
    {
        WeaponEquipPoint equipPoint =
            GetEquipPoint();

        ItemData removedWeapon =
            playerWeaponSlots.RemoveWeaponFromSetSlot(
                weaponSetIndex,
                equipPoint
            );

        if (removedWeapon == null)
            return;

        playerInventory.SetMouseHeldItemFromExternal(
            removedWeapon,
            0,
            true
        );
    }

    private void Refresh()
    {
        if (playerWeaponSlots == null)
        {
            SetSlot(emptyText, emptyColor);
            return;
        }

        if (slotType == WeaponSetSlotViewType.SaddleTurret)
        {
            RefreshSaddleTurretSlot();
            return;
        }

        WeaponEquipPoint equipPoint =
            GetEquipPoint();

        ItemData slotWeapon =
            playerWeaponSlots.GetWeaponInSetSlot(
                weaponSetIndex,
                equipPoint
            );

        if (playerInventory != null &&
            playerInventory.IsHoldingItem)
        {
            RefreshWhileHoldingItem(slotWeapon);
            return;
        }

        if (slotWeapon == null)
        {
            SetSlot(
                GetDefaultEmptyText(),
                emptyColor
            );

            return;
        }

        string text =
            slotWeapon.itemName;

        Color color =
            weaponColor;

        bool isActiveSet =
            playerWeaponSlots.ActiveWeaponSetIndex == weaponSetIndex;

        if (isActiveSet)
        {
            text = activePrefix + text;
            color = activeWeaponColor;
        }

        if (isActiveSet &&
            playerWeaponSlots.WeaponsDrawn)
        {
            text += drawnSuffix;
            color = drawnWeaponColor;
        }

        SetSlot(text, color);
    }

    private void RefreshWhileHoldingItem(ItemData slotWeapon)
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
            CanHeldItemEquipHere(heldItem);

        if (!canEquip)
        {
            SetSlot(cannotEquipText, invalidColor);
            return;
        }

        if (slotWeapon == null)
        {
            SetSlot(equipText, canEquipColor);
        }
        else
        {
            SetSlot(swapText, canEquipColor);
        }
    }

    private void RefreshSaddleTurretSlot()
    {
        WeaponSet set =
            playerWeaponSlots.GetWeaponSet(weaponSetIndex);

        if (set == null ||
            set.Mode != WeaponSetMode.ManualSaddleTurret ||
            set.SaddleSourceEquipment == null)
        {
            SetSlot("Saddle Turret", emptyColor);
            return;
        }

        string text =
            set.SaddleSourceEquipment.itemName;

        bool isActiveSet =
            playerWeaponSlots.ActiveWeaponSetIndex == weaponSetIndex;

        if (isActiveSet)
            text = activePrefix + text;

        if (isActiveSet &&
            playerWeaponSlots.WeaponsDrawn)
        {
            text += drawnSuffix;
        }

        SetSlot(text, reservedColor);
    }

    private bool CanHeldItemEquipHere(ItemData item)
    {
        if (item == null)
            return false;

        if (item.itemCategory != ItemCategory.Weapon)
            return false;

        switch (slotType)
        {
            case WeaponSetSlotViewType.LeftHand:
            case WeaponSetSlotViewType.RightHand:
                return item.weaponUseType == WeaponUseType.HandWeapon &&
                       item.handUsage == ItemHandUsage.OneHanded;

            case WeaponSetSlotViewType.BothHands:
                return item.weaponUseType == WeaponUseType.HandWeapon &&
                       item.handUsage == ItemHandUsage.TwoHanded;

            case WeaponSetSlotViewType.Mouth:
                return playerWeaponSlots.CanUseMouthWeapons &&
                       item.weaponUseType == WeaponUseType.MouthWeapon;

            default:
                return false;
        }
    }

    private WeaponEquipPoint GetEquipPoint()
    {
        switch (slotType)
        {
            case WeaponSetSlotViewType.LeftHand:
                return WeaponEquipPoint.LeftHand;

            case WeaponSetSlotViewType.RightHand:
                return WeaponEquipPoint.RightHand;

            case WeaponSetSlotViewType.BothHands:
                return WeaponEquipPoint.BothHands;

            case WeaponSetSlotViewType.Mouth:
                return WeaponEquipPoint.Mouth;

            default:
                return WeaponEquipPoint.RightHand;
        }
    }

    private string GetDefaultEmptyText()
    {
        switch (slotType)
        {
            case WeaponSetSlotViewType.LeftHand:
                return "Left Weapon";

            case WeaponSetSlotViewType.RightHand:
                return "Right Weapon";

            case WeaponSetSlotViewType.BothHands:
                return "Two-Hand Weapon";

            case WeaponSetSlotViewType.Mouth:
                return "Mouth Weapon";

            case WeaponSetSlotViewType.SaddleTurret:
                return "Saddle Turret";

            default:
                return emptyText;
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