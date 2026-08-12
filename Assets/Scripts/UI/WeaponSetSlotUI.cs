using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WeaponSetSlotUI :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerWeaponLoadout weaponLoadout;

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

    [Header("Weapon Position")]
    [SerializeField]
    [Range(0, PlayerWeaponLoadout.WeaponSetCount - 1)]
    private int weaponSetIndex;

    [SerializeField]
    [Range(0, WeaponSet.SlotCount - 1)]
    private int slotIndex;

    [Header("Visibility")]
    [SerializeField]
    private bool onlyShowWhenInventoryOpen = true;

    [Header("Text")]
    [SerializeField]
    private string emptyText = "Empty";

    [SerializeField]
    private string cannotEquipText =
        "Cannot Equip";

    [SerializeField]
    private string equipText = "Equip";

    [SerializeField]
    private string swapText = "Swap";

    [SerializeField]
    private string activePrefix = "> ";

    [Header("Colors")]
    [SerializeField]
    private Color emptyColor =
        new Color(
            0f,
            0f,
            0f,
            0.35f
        );

    [SerializeField]
    private Color weaponColor =
        new Color(
            1f,
            1f,
            1f,
            0.85f
        );

    [SerializeField]
    private Color activeWeaponColor =
        new Color(
            0.6f,
            0.85f,
            1f,
            0.9f
        );

    [SerializeField]
    private Color canEquipColor =
        new Color(
            0.2f,
            1f,
            0.2f,
            0.85f
        );

    [SerializeField]
    private Color invalidColor =
        new Color(
            1f,
            0.2f,
            0.2f,
            0.85f
        );

    private void Awake()
    {
        if (slotImage == null)
        {
            slotImage =
                GetComponent<Image>();
        }

        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }

        if (button != null)
        {
            button.onClick
                .RemoveAllListeners();

            button.onClick
                .AddListener(
                    OnSlotClicked
                );
        }
    }

    private void OnValidate()
    {
        weaponSetIndex =
            Mathf.Clamp(
                weaponSetIndex,
                0,
                PlayerWeaponLoadout
                    .WeaponSetCount - 1
            );

        slotIndex =
            Mathf.Clamp(
                slotIndex,
                0,
                WeaponSet.SlotCount - 1
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
        PlayerWeaponLoadout newWeaponLoadout,
        InventoryInteractionController
            newInteractionController)
    {
        Unsubscribe();

        weaponLoadout =
            newWeaponLoadout;

        interactionController =
            newInteractionController;

        if (isActiveAndEnabled)
            Subscribe();

        Refresh();
    }

    private void Subscribe()
    {
        if (weaponLoadout != null)
        {
            weaponLoadout.Changed +=
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
        if (weaponLoadout != null)
        {
            weaponLoadout.Changed -=
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
        if (weaponLoadout == null ||
            interactionController == null)
        {
            return;
        }

        if (interactionController
            .HasWeaponSlotSelection)
        {
            interactionController
                .TryAssignSelectedWeapon(
                    weaponSetIndex,
                    slotIndex
                );

            return;
        }

        interactionController
            .TryTakeWeaponFromSlot(
                weaponSetIndex,
                slotIndex
            );
    }

    private void Refresh()
    {
        if (weaponLoadout == null)
        {
            SetSlot(
                GetEmptyText(),
                emptyColor
            );

            return;
        }

        InventoryItemInstance weapon =
            weaponLoadout.GetWeapon(
                weaponSetIndex,
                slotIndex
            );

        if (interactionController != null &&
            interactionController
                .HasWeaponSlotSelection)
        {
            RefreshForSelection(
                weapon
            );

            return;
        }

        if (weapon == null ||
            weapon.Definition == null)
        {
            SetSlot(
                GetEmptyText(),
                emptyColor
            );

            return;
        }

        string weaponName =
            string.IsNullOrWhiteSpace(
                weapon.Definition.itemName
            )
                ? GetDefaultSlotName()
                : weapon.Definition.itemName;

        bool activeSet =
            weaponLoadout
                .ActiveWeaponSetIndex ==
            weaponSetIndex;

        if (activeSet)
        {
            weaponName =
                activePrefix +
                weaponName;
        }

        SetSlot(
            weaponName,
            activeSet
                ? activeWeaponColor
                : weaponColor
        );
    }

    private void RefreshForSelection(
        InventoryItemInstance currentWeapon)
    {
        if (interactionController == null)
            return;

        bool canAssign =
            interactionController
                .CanAssignSelectedWeapon(
                    weaponSetIndex,
                    slotIndex
                );

        if (!canAssign)
        {
            SetSlot(
                cannotEquipText,
                invalidColor
            );

            return;
        }

        if (currentWeapon == null)
        {
            SetSlot(
                equipText,
                canEquipColor
            );

            return;
        }

        SetSlot(
            swapText,
            canEquipColor
        );
    }

    private string GetEmptyText()
    {
        if (!string.IsNullOrWhiteSpace(
            emptyText))
        {
            return emptyText;
        }

        return GetDefaultSlotName();
    }

    private string GetDefaultSlotName()
    {
        return slotIndex == 0
            ? "Weapon 1"
            : "Weapon 2";
    }

    private void SetSlot(
        string text,
        Color color)
    {
        if (slotImage != null)
        {
            slotImage.color =
                color;
        }

        if (slotText != null)
        {
            slotText.text =
                text;
        }
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
            shouldShow
                ? 1f
                : 0f;

        canvasGroup.interactable =
            shouldShow;

        canvasGroup.blocksRaycasts =
            shouldShow;
    }
}