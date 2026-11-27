using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillLibraryUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private SkillDefinitionLibrary skillLibrary;

    [SerializeField]
    private Transform skillListParent;

    [SerializeField]
    private Button skillButtonPrefab;

    [Header("Selected Skill Details")]
    [SerializeField]
    private TMP_Text skillNameText;

    [SerializeField]
    private TMP_Text skillDescriptionText;

    [SerializeField]
    private TMP_Text skillStatsText;

    [SerializeField]
    private Image skillIconImage;

    private readonly List<Button> spawnedButtons = new();

    private PlayerCharacterProfile characterProfile;
    private CombatSkillDefinition selectedSkill;

    private void OnDestroy()
    {
        if (characterProfile != null)
            characterProfile.CombatSkillsChanged -= HandleCombatSkillsChanged;
    }

    public void BindPlayer(PlayerCharacterProfile profile)
    {
        if (characterProfile != null)
            characterProfile.CombatSkillsChanged -= HandleCombatSkillsChanged;

        characterProfile = profile;

        if (characterProfile != null)
            characterProfile.CombatSkillsChanged += HandleCombatSkillsChanged;

        RefreshIfOpen();
    }

    private void HandleCombatSkillsChanged()
    {
        RefreshIfOpen();
    }

    private void RefreshIfOpen()
    {
        if (!isActiveAndEnabled)
            return;

        if (skillLibrary == null || skillListParent == null || skillButtonPrefab == null)
        {
            Debug.LogWarning("SkillLibraryUI has missing references.", this);
            return;
        }

        Rebuild();
    }

    private void OnEnable()
    {
        RefreshIfOpen();
    }

    private void Rebuild()
    {
        ClearButtons();

        CombatSkillDefinition firstSkill = null;
        bool selectedSkillExists = false;

        foreach (CombatSkillDefinition skill in skillLibrary.SkillDefinitions)
        {
            if (skill == null)
                continue;

            if (firstSkill == null)
                firstSkill = skill;

            if (skill == selectedSkill)
                selectedSkillExists = true;

            Button button = Instantiate(skillButtonPrefab, skillListParent);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
            {
                label.text = skill.name;

                if (characterProfile?.ProfileData != null)
                {
                    bool learned = characterProfile.ProfileData.HasLearnedCombatSkill(skill.Id);
                    label.text += learned ? " (Learned)" : " (Locked)";
                }
            }

            CombatSkillDefinition capturedSkill = skill;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectSkill(capturedSkill));

            spawnedButtons.Add(button);
        }

        SelectSkill(selectedSkillExists ? selectedSkill : firstSkill);
    }

    private void SelectSkill(
        CombatSkillDefinition skill)
    {
        selectedSkill = skill;

        if (skillNameText != null)
        {
            skillNameText.text =
                skill != null
                    ? skill.name
                    : "No Skills";
        }

        if (skillDescriptionText != null)
        {
            skillDescriptionText.text =
                skill != null
                    ? skill.description
                    : "";
        }

        if (skillStatsText != null)
        {
            if (skill == null)
            {
                skillStatsText.text = "";
            }
            else
            {
                string status = "No Character";

                if (characterProfile?.ProfileData != null)
                {
                    status = characterProfile.ProfileData.HasLearnedCombatSkill(skill.Id)
                        ? "Learned"
                        : "Locked";
                }

                skillStatsText.text =
                    $"Status: {status}\n" +
                    $"Aether: {skill.aetherCost:0.#}\n" +
                    $"Cast Time: {skill.castTime:0.#}s\n" +
                    $"Cooldown: {skill.cooldown:0.#}s";
            }
        }

        if (skillIconImage != null)
        {
            skillIconImage.sprite =
                skill != null ? skill.icon : null;

            skillIconImage.enabled =
                skill != null && skill.icon != null;
        }
    }

    private void ClearButtons()
    {
        foreach (Button button in spawnedButtons)
        {
            if (button == null)
                continue;

            button.gameObject.SetActive(false);
            Destroy(button.gameObject);
        }

        spawnedButtons.Clear();
    }
}
