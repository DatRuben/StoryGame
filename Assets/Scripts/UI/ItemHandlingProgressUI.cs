using UnityEngine;
using UnityEngine.UI;

public sealed class ItemHandlingProgressUI :
    MonoBehaviour
{
    [SerializeField]
    private GameObject progressRoot;

    [SerializeField]
    private Image progressFill;

    private PlayerItemHandlingController
        itemHandlingController;

    public void BindPlayer(
        PlayerItemHandlingController controller)
    {
        itemHandlingController =
            controller;

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool visible =
            itemHandlingController != null &&
            itemHandlingController
                .CurrentOperation != null;

        if (progressRoot != null &&
            progressRoot.activeSelf != visible)
        {
            progressRoot.SetActive(
                visible
            );
        }

        if (!visible ||
            progressFill == null)
        {
            return;
        }

        progressFill.fillAmount =
            itemHandlingController
                .OperationProgress01;
    }
}