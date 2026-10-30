using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CursorController :
    MonoBehaviour
{
    private readonly HashSet<object>
        unlockRequests =
            new HashSet<object>();

    public bool IsUnlocked =>
        unlockRequests.Count > 0;

    private void Start()
    {
        ApplyCursorState();
    }

    private void OnApplicationFocus(
        bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursorState();
        }
    }

    public void SetUnlocked(
        object source,
        bool unlocked)
    {
        if (source == null)
        {
            throw new ArgumentNullException(
                nameof(source)
            );
        }

        bool changed =
            unlocked
                ? unlockRequests.Add(source)
                : unlockRequests.Remove(source);

        if (!changed)
            return;

        ApplyCursorState();
    }

    public void ClearRequest(
        object source)
    {
        if (source == null)
            return;

        if (!unlockRequests.Remove(source))
            return;

        ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        bool unlocked =
            unlockRequests.Count > 0;

        Cursor.lockState =
            unlocked
                ? CursorLockMode.None
                : CursorLockMode.Locked;

        Cursor.visible =
            unlocked;
    }
}