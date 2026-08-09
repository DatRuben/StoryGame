using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeldItemUI :
    MonoBehaviour
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
            ItemDefinition item,
            string fallbackName,
            string controls)
        {
            if (item == null)
            {
                ShowEmpty();
                return;
            }

            if (root != null)
                root.SetActive(true);

            bool hasControls =
                !string.IsNullOrWhiteSpace(
                    controls
                );

            if (controlsRoot != null)
            {
                controlsRoot.SetActive(
                    hasControls
                );
            }

            if (iconImage != null)
            {
                iconImage.sprite =
                    item.itemIcon;

                iconImage.enabled =
                    item.itemIcon != null;
            }

            if (nameText != null)
            {
                nameText.text =
                    string.IsNullOrWhiteSpace(
                        item.itemName
                    )
                        ? fallbackName
                        : item.itemName;
            }

            if (controlsText != null)
            {
                controlsText.text =
                    controls;
            }
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);

            if (controlsRoot != null)
            {
                controlsRoot.SetActive(false);
            }

            if (controlsText != null)
                controlsText.text = "";
        }
    }

    [Header("References")]
    [SerializeField]
    private PlayerGripState gripState;

    [SerializeField]
    private PlayerCharacterProfile characterProfile;

    [Header("Cards")]
    [SerializeField]
    private HeldItemCard leftHandCard =
        new HeldItemCard();

    [SerializeField]
    private HeldItemCard rightHandCard =
        new HeldItemCard();

    [SerializeField]
    private HeldItemCard bothHandsCard =
        new HeldItemCard();

    [SerializeField]
    private HeldItemCard mouthCard =
        new HeldItemCard();

    [Header("Labels")]
    [SerializeField]
    private string leftHandEmptyText =
        "Left Hand Empty";

    [SerializeField]
    private string rightHandEmptyText =
        "Right Hand Empty";

    [SerializeField]
    private string bothHandsEmptyText =
        "Both Hands Empty";

    [SerializeField]
    private string mouthEmptyText =
        "Mouth Empty";

    private void Awake()
    {
        EnsureCardsExist();

        if (gripState == null)
        {
            gripState =
                GetComponentInParent<
                    PlayerGripState>();
        }

        if (characterProfile == null)
        {
            characterProfile =
                GetComponentInParent<
                    PlayerCharacterProfile>();
        }
    }

    private void OnValidate()
    {
        EnsureCardsExist();
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
        PlayerGripState newGripState,
        PlayerCharacterProfile
            newCharacterProfile)
    {
        Unsubscribe();

        gripState =
            newGripState;

        characterProfile =
            newCharacterProfile;

        if (isActiveAndEnabled)
            Subscribe();

        Refresh();
    }

    private void Subscribe()
    {
        if (gripState != null)
        {
            gripState.Changed +=
                Refresh;
        }

        if (characterProfile != null)
        {
            characterProfile.AttributesChanged +=
                Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (gripState != null)
        {
            gripState.Changed -=
                Refresh;
        }

        if (characterProfile != null)
        {
            characterProfile.AttributesChanged -=
                Refresh;
        }
    }

    private void EnsureCardsExist()
    {
        if (leftHandCard == null)
        {
            leftHandCard =
                new HeldItemCard();
        }

        if (rightHandCard == null)
        {
            rightHandCard =
                new HeldItemCard();
        }

        if (bothHandsCard == null)
        {
            bothHandsCard =
                new HeldItemCard();
        }

        if (mouthCard == null)
        {
            mouthCard =
                new HeldItemCard();
        }
    }

    private void Refresh()
    {
        EnsureCardsExist();

        leftHandCard.emptyText =
            leftHandEmptyText;

        rightHandCard.emptyText =
            rightHandEmptyText;

        bothHandsCard.emptyText =
            bothHandsEmptyText;

        mouthCard.emptyText =
            mouthEmptyText;

        if (gripState == null)
        {
            leftHandCard.ShowEmpty();
            rightHandCard.ShowEmpty();
            bothHandsCard.Hide();
            mouthCard.Hide();

            return;
        }

        InventoryItemInstance leftItem =
            gripState.GetItem(
                GripType.Hand,
                0
            );

        InventoryItemInstance rightItem =
            gripState.GetItem(
                GripType.Hand,
                1
            );

        InventoryItemInstance mouthItem =
            gripState.GetItem(
                GripType.Mouth,
                0
            );

        bool sameItemInBothHands =
            leftItem != null &&
            ReferenceEquals(
                leftItem,
                rightItem
            );

        RefreshHands(
            leftItem,
            rightItem,
            sameItemInBothHands
        );

        RefreshMouth(
            mouthItem
        );
    }

    private void RefreshHands(
        InventoryItemInstance leftItem,
        InventoryItemInstance rightItem,
        bool sameItemInBothHands)
    {
        if (sameItemInBothHands)
        {
            leftHandCard.Hide();
            rightHandCard.Hide();

            bothHandsCard.ShowItem(
                leftItem.Definition,
                "Both Hands",
                GetControlsText(
                    leftItem,
                    GripType.Hand
                )
            );

            return;
        }

        bothHandsCard.Hide();

        if (gripState.HandGripCount >= 1)
        {
            if (leftItem != null)
            {
                leftHandCard.ShowItem(
                    leftItem.Definition,
                    "Left Hand",
                    GetControlsText(
                        leftItem,
                        GripType.Hand
                    )
                );
            }
            else
            {
                leftHandCard.ShowEmpty();
            }
        }
        else
        {
            leftHandCard.Hide();
        }

        if (gripState.HandGripCount >= 2)
        {
            if (rightItem != null)
            {
                rightHandCard.ShowItem(
                    rightItem.Definition,
                    "Right Hand",
                    GetControlsText(
                        rightItem,
                        GripType.Hand
                    )
                );
            }
            else
            {
                rightHandCard.ShowEmpty();
            }
        }
        else
        {
            rightHandCard.Hide();
        }
    }

    private void RefreshMouth(
        InventoryItemInstance mouthItem)
    {
        if (gripState.MouthGripCount <= 0)
        {
            mouthCard.Hide();
            return;
        }

        if (mouthItem == null)
        {
            mouthCard.ShowEmpty();
            return;
        }

        mouthCard.ShowItem(
            mouthItem.Definition,
            "Mouth",
            GetControlsText(
                mouthItem,
                GripType.Mouth
            )
        );
    }

    private string GetControlsText(
        InventoryItemInstance itemInstance,
        GripType gripType)
    {
        if (itemInstance == null ||
            itemInstance.Definition == null)
        {
            return "";
        }

        ItemDefinition definition =
            itemInstance.Definition;

        if (definition.itemCategory ==
                ItemCategory.Weapon &&
            CanCurrentlyUse(
                itemInstance,
                gripType
            ) &&
            !string.IsNullOrWhiteSpace(
                definition.weaponControlsText
            ))
        {
            return definition
                .weaponControlsText;
        }

        return definition.heldControlsText;
    }

    private bool CanCurrentlyUse(
        InventoryItemInstance itemInstance,
        GripType gripType)
    {
        if (itemInstance == null ||
            itemInstance.Definition == null ||
            gripState == null ||
            characterProfile == null ||
            characterProfile
                .EffectiveHandlingProfile ==
                null)
        {
            return false;
        }

        int assignedGripCount =
            gripState.GetAssignedGripCount(
                itemInstance
            );

        ResolvedItemHandling handling =
            ItemHandlingResolver.Resolve(
                itemInstance.Definition,
                characterProfile
                    .EffectiveHandlingProfile,
                gripType,
                assignedGripCount
            );

        return handling != null &&
               handling.canUse;
    }
}