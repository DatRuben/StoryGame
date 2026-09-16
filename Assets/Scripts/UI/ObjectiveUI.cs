using TMPro;
using UnityEngine;

public sealed class ObjectiveUI :
    MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI objectiveText;

    [SerializeField]
    private GameObjective objective;

    private void Awake()
    {
        if (objectiveText == null)
        {
            objectiveText =
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

    public void BindObjective(
        GameObjective newObjective)
    {
        Unsubscribe();

        objective = newObjective;

        if (isActiveAndEnabled)
        {
            Subscribe();
        }

        Refresh();
    }

    private void Subscribe()
    {
        if (objective == null)
            return;

        objective.Completed -=
            HandleObjectiveCompleted;

        objective.Completed +=
            HandleObjectiveCompleted;
    }

    private void Unsubscribe()
    {
        if (objective == null)
            return;

        objective.Completed -=
            HandleObjectiveCompleted;
    }

    private void HandleObjectiveCompleted(
        GameObjective completedObjective)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (objectiveText == null)
            return;

        if (objective == null)
        {
            objectiveText.text = "";
            return;
        }

        objectiveText.text =
            objective.IsComplete
                ? "Complete: " +
                    objective.DisplayText
                : "Objective: " +
                    objective.DisplayText;
    }
}