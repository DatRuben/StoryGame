using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeldItemUI : MonoBehaviour
{
    [Serializable]
    private class HeldItemCard
    {
        public GameObject root;
        public GameObject controlsRoot;

        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI controlsText;

        public string emptyText = "Empty";

        public void ShowEmpty()
        {
            if (root != null)
                root.SetActive(true);

            if (controlsRoot != null)
                controlsRoot.SetActive(false);

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (nameText != null)
                nameText.text = emptyText;

            if (controlsText != null)
                controlsText.text = "";
        }

        public void ShowItem(
            ItemData item,
            string fallbackName,
            string controls)
        {
            if (root != null)
                root.SetActive(true);

            bool hasControls =
                !string.IsNullOrWhiteSpace(controls);

            if (controlsRoot != null)
                controlsRoot.SetActive(hasControls);

            if (item == null)
            {
                ShowEmpty();
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.enabled = item.itemIcon != null;
            }

            if (nameText != null)
            {
                nameText.text =
                    string.IsNullOrWhiteSpace(item.itemName)
                        ? fallbackName
                        : item.itemName;
            }

            if (controlsText != null)
                controlsText.text = controls;
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);

            if (controlsRoot != null)
                controlsRoot.SetActive(false);

            if (controlsText != null)
                controlsText.text = "";
        }
    }

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerHolding playerHolding;
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    [Header("Cards")]
    [SerializeField] private HeldItemCard leftHandCard = new HeldItemCard();
    [SerializeField] private HeldItemCard rightHandCard = new HeldItemCard();
    [SerializeField] private HeldItemCard bothHandsCard = new HeldItemCard();
    [SerializeField] private HeldItemCard mouthCard = new HeldItemCard();
    [SerializeField] private HeldItemCard saddleTurretCard = new HeldItemCard();

    [Header("Labels")]
    [SerializeField] private string leftHandEmptyText = "Left Hand Empty";
    [SerializeField] private string rightHandEmptyText = "Right Hand Empty";
    [SerializeField] private string bothHandsEmptyText = "Both Hands Empty";
    [SerializeField] private string mouthEmptyText = "Mouth Empty";
    [SerializeField] private string saddleTurretEmptyText = "Saddle Turret";

    private void Awake()
    {
        EnsureCardsExist();

        if (playerInventory == null)
            playerInventory = GetComponentInParent<PlayerInventory>();

        if (playerHolding == null)
            playerHolding = GetComponentInParent<PlayerHolding>();

        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponentInParent<PlayerWeaponSlots>();
    }

    private void OnValidate()
    {
        EnsureCardsExist();
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.OnHeldItemChanged += Refresh;

        if (playerHolding != null)
            playerHolding.OnHoldingChanged += Refresh;

        if (playerWeaponSlots != null)
            playerWeaponSlots.OnWeaponSlotsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnHeldItemChanged -= Refresh;

        if (playerHolding != null)
            playerHolding.OnHoldingChanged -= Refresh;

        if (playerWeaponSlots != null)
            playerWeaponSlots.OnWeaponSlotsChanged -= Refresh;
    }

    private void EnsureCardsExist()
    {
        if (leftHandCard == null)
            leftHandCard = new HeldItemCard();

        if (rightHandCard == null)
            rightHandCard = new HeldItemCard();

        if (bothHandsCard == null)
            bothHandsCard = new HeldItemCard();

        if (mouthCard == null)
            mouthCard = new HeldItemCard();

        if (saddleTurretCard == null)
            saddleTurretCard = new HeldItemCard();
    }

    private void Refresh()
    {
        EnsureCardsExist();

        leftHandCard.emptyText = leftHandEmptyText;
        rightHandCard.emptyText = rightHandEmptyText;
        bothHandsCard.emptyText = bothHandsEmptyText;
        mouthCard.emptyText = mouthEmptyText;
        saddleTurretCard.emptyText = saddleTurretEmptyText;

        ItemData leftHandItem = null;
        ItemData rightHandItem = null;
        ItemData bothHandsItem = null;
        ItemData mouthItem = null;
        ItemData saddleTurretSource = null;

        bool leftIsWeaponUse = false;
        bool rightIsWeaponUse = false;
        bool bothIsWeaponUse = false;
        bool mouthIsWeaponUse = false;

        LoadPlayerHoldingItems(
            ref leftHandItem,
            ref rightHandItem,
            ref bothHandsItem,
            ref mouthItem,
            ref leftIsWeaponUse,
            ref rightIsWeaponUse,
            ref bothIsWeaponUse,
            ref mouthIsWeaponUse
        );

        LoadDrawnWeaponSetItems(
            ref leftHandItem,
            ref rightHandItem,
            ref bothHandsItem,
            ref mouthItem,
            ref saddleTurretSource,
            ref leftIsWeaponUse,
            ref rightIsWeaponUse,
            ref bothIsWeaponUse,
            ref mouthIsWeaponUse
        );

        LoadMouseHeldItem(
            ref leftHandItem,
            ref rightHandItem,
            ref bothHandsItem,
            ref mouthItem,
            ref leftIsWeaponUse,
            ref rightIsWeaponUse,
            ref bothIsWeaponUse,
            ref mouthIsWeaponUse
        );

        DrawCards(
            leftHandItem,
            rightHandItem,
            bothHandsItem,
            mouthItem,
            saddleTurretSource,
            leftIsWeaponUse,
            rightIsWeaponUse,
            bothIsWeaponUse,
            mouthIsWeaponUse
        );
    }

    private void LoadPlayerHoldingItems(
        ref ItemData leftHandItem,
        ref ItemData rightHandItem,
        ref ItemData bothHandsItem,
        ref ItemData mouthItem,
        ref bool leftIsWeaponUse,
        ref bool rightIsWeaponUse,
        ref bool bothIsWeaponUse,
        ref bool mouthIsWeaponUse)
    {
        if (playerHolding == null)
            return;

        leftHandItem = playerHolding.LeftHandItem;
        rightHandItem = playerHolding.RightHandItem;
        bothHandsItem = playerHolding.TwoHandedItem;
        mouthItem = playerHolding.MouthItem;

        leftIsWeaponUse =
            IsUsableHandWeapon(leftHandItem);

        rightIsWeaponUse =
            IsUsableHandWeapon(rightHandItem);

        bothIsWeaponUse =
            IsUsableHandWeapon(bothHandsItem);

        mouthIsWeaponUse =
            IsUsableMouthWeapon(mouthItem);
    }

    private void LoadDrawnWeaponSetItems(
        ref ItemData leftHandItem,
        ref ItemData rightHandItem,
        ref ItemData bothHandsItem,
        ref ItemData mouthItem,
        ref ItemData saddleTurretSource,
        ref bool leftIsWeaponUse,
        ref bool rightIsWeaponUse,
        ref bool bothIsWeaponUse,
        ref bool mouthIsWeaponUse)
    {
        if (playerWeaponSlots == null)
            return;

        if (!playerWeaponSlots.WeaponsDrawn)
            return;

        ItemData drawnTwoHandedWeapon =
            playerWeaponSlots.GetDrawnTwoHandedWeapon();

        if (drawnTwoHandedWeapon != null)
        {
            bothHandsItem = drawnTwoHandedWeapon;
            leftHandItem = null;
            rightHandItem = null;

            bothIsWeaponUse = true;
            leftIsWeaponUse = false;
            rightIsWeaponUse = false;

            return;
        }

        ItemData drawnLeftWeapon =
            playerWeaponSlots.GetDrawnLeftHandWeapon();

        if (drawnLeftWeapon != null)
        {
            leftHandItem = drawnLeftWeapon;
            leftIsWeaponUse = true;
        }

        ItemData drawnRightWeapon =
            playerWeaponSlots.GetDrawnRightHandWeapon();

        if (drawnRightWeapon != null)
        {
            rightHandItem = drawnRightWeapon;
            rightIsWeaponUse = true;
        }

        ItemData drawnMouthWeapon =
            playerWeaponSlots.GetDrawnMouthWeapon();

        if (drawnMouthWeapon != null)
        {
            mouthItem = drawnMouthWeapon;
            mouthIsWeaponUse = true;
        }

        saddleTurretSource =
            playerWeaponSlots.GetDrawnManualSaddleTurretSource();
    }

    private void LoadMouseHeldItem(
        ref ItemData leftHandItem,
        ref ItemData rightHandItem,
        ref ItemData bothHandsItem,
        ref ItemData mouthItem,
        ref bool leftIsWeaponUse,
        ref bool rightIsWeaponUse,
        ref bool bothIsWeaponUse,
        ref bool mouthIsWeaponUse)
    {
        if (playerInventory == null)
            return;

        if (!playerInventory.MouseHeldItemCountsAsHeld)
            return;

        if (playerInventory.HeldItem == null ||
            playerInventory.HeldItem.ItemData == null)
        {
            return;
        }

        ItemData mouseHeldItem =
            playerInventory.HeldItem.ItemData;

        bool isUsableHandWeapon =
            IsUsableHandWeapon(mouseHeldItem);

        bool isUsableMouthWeapon =
            IsUsableMouthWeapon(mouseHeldItem);

        bool canHoldInMouth =
            playerHolding != null &&
            playerHolding.CanHoldItemInMouth;

        if (isUsableMouthWeapon &&
            canHoldInMouth &&
            mouthItem == null)
        {
            mouthItem = mouseHeldItem;
            mouthIsWeaponUse = true;
            return;
        }

        if (mouseHeldItem.handUsage == ItemHandUsage.TwoHanded)
        {
            if (bothHandsItem == null &&
                leftHandItem == null &&
                rightHandItem == null)
            {
                bothHandsItem = mouseHeldItem;
                bothIsWeaponUse = isUsableHandWeapon;
                return;
            }

            if (canHoldInMouth &&
                mouthItem == null)
            {
                mouthItem = mouseHeldItem;
                mouthIsWeaponUse = false;
                return;
            }

            return;
        }

        if (rightHandItem == null &&
            bothHandsItem == null)
        {
            rightHandItem = mouseHeldItem;
            rightIsWeaponUse = isUsableHandWeapon;
            return;
        }

        if (leftHandItem == null &&
            bothHandsItem == null)
        {
            leftHandItem = mouseHeldItem;
            leftIsWeaponUse = isUsableHandWeapon;
            return;
        }

        if (canHoldInMouth &&
            mouthItem == null)
        {
            mouthItem = mouseHeldItem;
            mouthIsWeaponUse = false;
        }
    }

    private void DrawCards(
        ItemData leftHandItem,
        ItemData rightHandItem,
        ItemData bothHandsItem,
        ItemData mouthItem,
        ItemData saddleTurretSource,
        bool leftIsWeaponUse,
        bool rightIsWeaponUse,
        bool bothIsWeaponUse,
        bool mouthIsWeaponUse)
    {
        bool hasBothHandsItem =
            bothHandsItem != null;

        if (hasBothHandsItem)
        {
            leftHandCard.Hide();
            rightHandCard.Hide();

            bothHandsCard.ShowItem(
                bothHandsItem,
                "Both Hands",
                GetControlsText(
                    bothHandsItem,
                    bothIsWeaponUse
                )
            );
        }
        else
        {
            bothHandsCard.Hide();

            if (leftHandItem != null)
            {
                leftHandCard.ShowItem(
                    leftHandItem,
                    "Left Hand",
                    GetControlsText(
                        leftHandItem,
                        leftIsWeaponUse
                    )
                );
            }
            else
            {
                leftHandCard.ShowEmpty();
            }

            if (rightHandItem != null)
            {
                rightHandCard.ShowItem(
                    rightHandItem,
                    "Right Hand",
                    GetControlsText(
                        rightHandItem,
                        rightIsWeaponUse
                    )
                );
            }
            else
            {
                rightHandCard.ShowEmpty();
            }
        }

        bool shouldShowMouth =
            mouthItem != null ||
            (
                playerHolding != null &&
                playerHolding.CanHoldItemInMouth
            );

        if (shouldShowMouth)
        {
            if (mouthItem != null)
            {
                mouthCard.ShowItem(
                    mouthItem,
                    "Mouth",
                    GetControlsText(
                        mouthItem,
                        mouthIsWeaponUse
                    )
                );
            }
            else
            {
                mouthCard.ShowEmpty();
            }
        }
        else
        {
            mouthCard.Hide();
        }

        if (saddleTurretSource != null)
        {
            saddleTurretCard.ShowItem(
                saddleTurretSource,
                "Manual Saddle Turret",
                saddleTurretSource.manualSaddleTurretControlsText
            );
        }
        else
        {
            saddleTurretCard.Hide();
        }
    }

    private bool IsUsableHandWeapon(ItemData item)
    {
        return item != null &&
               item.itemCategory == ItemCategory.Weapon &&
               item.weaponUseType == WeaponUseType.HandWeapon;
    }

    private bool IsUsableMouthWeapon(ItemData item)
    {
        return item != null &&
               item.itemCategory == ItemCategory.Weapon &&
               item.weaponUseType == WeaponUseType.MouthWeapon &&
               playerWeaponSlots != null &&
               playerWeaponSlots.CanUseMouthWeapons;
    }

    private string GetControlsText(
        ItemData item,
        bool isWeaponUse)
    {
        if (item == null)
            return "";

        if (isWeaponUse &&
            !string.IsNullOrWhiteSpace(item.weaponControlsText))
        {
            return item.weaponControlsText;
        }

        return item.heldControlsText;
    }
}