using System;
using UnityEngine;

public enum CharacterForm
{
    Standing,
    Feral
}

public sealed class PlayerFormState : MonoBehaviour
{
    public CharacterForm CurrentForm
    {
        get;
        private set;
    }

    public bool CanSwitchForm
    {
        get;
        private set;
    }

    public event Action Changed;

    internal void Configure(
        BodyType bodyType)
    {
        CanSwitchForm =
            bodyType ==
            BodyType.StanceSwitching;

        CharacterForm newForm;

        switch (bodyType)
        {
            case BodyType.Quadruped:
                newForm =
                    CharacterForm.Feral;
                break;

            case BodyType.Humanoid:
            case BodyType.StanceSwitching:
            default:
                newForm =
                    CharacterForm.Standing;
                break;
        }

        if (CurrentForm == newForm)
            return;

        CurrentForm = newForm;

        Changed?.Invoke();
    }

    internal bool SetForm(
        CharacterForm form)
    {
        if (!CanSwitchForm ||
            CurrentForm == form)
        {
            return false;
        }

        CurrentForm = form;

        Changed?.Invoke();

        return true;
    }
}