using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillLibraryUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private SkillDefinitionLibrary skillLibrary;

    [Header("Panel")]
    [SerializeField]
    private GameObject panelRoot;

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

    private void Awake()
    {
        Close();
    }

    public void Open()
    {
        if (skillLibrary == null ||
            panelRoot == null ||
            skillListParent == null ||
            skillButtonPrefab == null)
        {
            Debug.LogWarning(
                "SkillLibraryUI has missing references.",
                this
            );

            return;
        }

        panelRoot.SetActive(true);
        Rebuild();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void Rebuild()
    {
        ClearButtons();

        CombatSkillDefinition firstSkill = null;

        foreach (CombatSkillDefinition skill
                 in skillLibrary.SkillDefinitions)
        {
            if (skill == null)
                continue;

            if (firstSkill == null)
                firstSkill = skill;

            Button button = Instantiate(
                skillButtonPrefab,
                skillListParent
            );

            TMP_Text label =
                button.GetComponentInChildren<TMP_Text>(
                    true
                );

            if (label != null)
                label.text = skill.name;

            CombatSkillDefinition capturedSkill = skill;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(
                () => SelectSkill(capturedSkill)
            );

            spawnedButtons.Add(button);
        }

        SelectSkill(firstSkill);
    }

    private void SelectSkill(
        CombatSkillDefinition skill)
    {
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
            skillStatsText.text =
                skill != null
                    ? $"Aether: {skill.aetherCost:0.#}\n" +
                      $"Cast Time: {skill.castTime:0.#}s\n" +
                      $"Cooldown: {skill.cooldown:0.#}s"
                    : "";
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
            if (button != null)
                Destroy(button.gameObject);
        }

        spawnedButtons.Clear();
    }
}
