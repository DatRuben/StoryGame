using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PlayerGameplayCapability
{
    None = 0,

    Movement = 1 << 0,
    Combat = 1 << 1,
    Inventory = 1 << 2,
    WorldInteraction = 1 << 3,
    ItemHandling = 1 << 4
}

public sealed class PlayerGameplayState : MonoBehaviour
{
    private readonly Dictionary<object, PlayerGameplayCapability>
        restrictionsBySource =
            new Dictionary<object, PlayerGameplayCapability>();

    public PlayerGameplayCapability BlockedCapabilities
    {
        get;
        private set;
    }

    public event Action<PlayerGameplayCapability>
        OnCapabilitiesChanged;

    public event Action<PlayerGameplayCapability>
        OnCapabilitiesInterrupted;

    public bool Allows(
        PlayerGameplayCapability capability)
    {
        return
            (BlockedCapabilities & capability) == 0;
    }

    public void SetRestriction(
        object source,
        PlayerGameplayCapability blockedCapabilities)
    {
        if (source == null)
        {
            throw new ArgumentNullException(
                nameof(source)
            );
        }

        if (blockedCapabilities ==
            PlayerGameplayCapability.None)
        {
            restrictionsBySource.Remove(source);
        }
        else
        {
            restrictionsBySource[source] =
                blockedCapabilities;
        }

        RecalculateBlockedCapabilities();
    }

    public void ClearRestriction(
        object source)
    {
        if (source == null)
            return;

        if (!restrictionsBySource.Remove(source))
            return;

        RecalculateBlockedCapabilities();
    }

    private void RecalculateBlockedCapabilities()
    {
        PlayerGameplayCapability previous =
            BlockedCapabilities;

        PlayerGameplayCapability current =
            PlayerGameplayCapability.None;

        foreach (
            PlayerGameplayCapability restriction
            in restrictionsBySource.Values)
        {
            current |= restriction;
        }

        if (current == previous)
            return;

        BlockedCapabilities = current;

        PlayerGameplayCapability newlyBlocked =
            current & ~previous;

        OnCapabilitiesChanged?.Invoke(
            current
        );

        if (newlyBlocked !=
            PlayerGameplayCapability.None)
        {
            OnCapabilitiesInterrupted?.Invoke(
                newlyBlocked
            );
        }
    }
}