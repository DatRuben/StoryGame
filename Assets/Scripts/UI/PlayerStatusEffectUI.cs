using System.Text;
using TMPro;
using UnityEngine;

public sealed class PlayerStatusEffectsUI :
    MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI effectsText;

    private StatusEffects statusEffects;

    private void Awake()
    {
        if (effectsText == null)
        {
            effectsText =
                GetComponent<TextMeshProUGUI>();
        }
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
        StatusEffects newStatusEffects)
    {
        Unsubscribe();

        statusEffects =
            newStatusEffects;

        if (isActiveAndEnabled)
        {
            Subscribe();
        }

        Refresh();
    }

    private void Subscribe()
    {
        if (statusEffects == null)
            return;

        statusEffects.EffectsChanged -=
            Refresh;

        statusEffects.EffectsChanged +=
            Refresh;
    }

    private void Unsubscribe()
    {
        if (statusEffects == null)
            return;

        statusEffects.EffectsChanged -=
            Refresh;
    }

    private void Refresh()
    {
        if (effectsText == null)
            return;

        if (statusEffects == null ||
            statusEffects.ActiveEffects.Count == 0)
        {
            effectsText.text = "";
            return;
        }

        StringBuilder builder =
            new StringBuilder();

        for (int i = 0;
             i < statusEffects
                 .ActiveEffects.Count;
             i++)
        {
            ActiveStatusEffect activeEffect =
                statusEffects.ActiveEffects[i];

            if (activeEffect == null ||
                activeEffect.definition == null)
            {
                continue;
            }

            StatusEffectDefinition definition =
                activeEffect.definition;

            string displayName =
                string.IsNullOrWhiteSpace(
                    definition.displayName)
                    ? definition.name
                    : definition.displayName;

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(
                displayName
            );

            if (activeEffect.stacks > 1)
            {
                builder.Append(
                    " x"
                );

                builder.Append(
                    activeEffect.stacks
                );
            }
        }

        effectsText.text =
            builder.ToString();
    }
}