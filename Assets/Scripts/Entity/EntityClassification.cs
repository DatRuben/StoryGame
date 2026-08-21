using System;
using UnityEngine;

[Flags]
public enum EntityTrait
{
    None = 0,

    Organic = 1 << 0,
    Mechanical = 1 << 1,
    Construct = 1 << 2,
    Aetheric = 1 << 3
}

public sealed class EntityClassification :
    MonoBehaviour
{
    [SerializeField]
    private EntityTrait traits =
        EntityTrait.None;

    public EntityTrait Traits =>
        traits;

    public bool HasAny(
        EntityTrait requiredTraits)
    {
        if (requiredTraits ==
            EntityTrait.None)
        {
            return true;
        }

        return (traits & requiredTraits) != 0;
    }
}