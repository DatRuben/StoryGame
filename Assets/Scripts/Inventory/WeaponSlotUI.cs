using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visibility")]
    [SerializeField] private bool onlyShowWhenInventoryOpen = true;

    [Header("Text")]
    [SerializeField] private string emptyText = "Weapon";
    [SerializeField] private string drawnSuffix = " (Drawn)";

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color weaponColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color drawnWeaponColor = new Color(0.45f, 0.45f, 0.45f, 0.65f);
    [SerializeField] private Color canEquipHeldWeaponColor = new Color(0.2f, 1f, 0.2f, 0.85f);
    [SerializeField] private Color invalidHeldItemColor = new Color(1f, 0.2f, 0.2f, 0.85f);

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
        {
            playerInventory.OnHeldItemChanged += Refresh;
            playerInventory.OnEquipmentChanged += Refresh;
        }

        Refresh();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnHeldItemChanged -= Refresh;
            playerInventory.OnEquipmentChanged -= Refresh;
        }
    }

    private void OnSlotClicked()
    {
        if (playerInventory == null)
            return;

        if (playerInventory.IsHoldingItem)
        {
            bool equipped =
                playerInventory.TryEquipHeldItemToWeaponSlot();

            if (!equipped)
            {
                Debug.Log("Held item cannot be equipped to weapon slot.");
            }

            Refresh();
            return;
        }

        if (playerInventory.HasWeaponInSlot)
        {
            playerInventory.ToggleWeaponDrawn();
            Refresh();
        }
    }

    private void Refresh()
    {
        if (playerInventory == null)
        {
            SetSlot(emptyText, emptyColor);
            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            if (playerInventory.CanEquipHeldItemToWeaponSlot())
            {
                SetSlot("Equip Weapon", canEquipHeldWeaponColor);
            }
            else
            {
                SetSlot("Cannot Equip", invalidHeldItemColor);
            }

            return;
        }

        ItemData weapon =
            playerInventory.WeaponSlotItem;

        if (weapon == null)
        {
            SetSlot(emptyText, emptyColor);
            return;
        }

        if (playerInventory.IsWeaponDrawn)
        {
            SetSlot(
                weapon.itemName + drawnSuffix,
                drawnWeaponColor
            );

            return;
        }

        SetSlot(
            weapon.itemName,
            weaponColor
        );
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