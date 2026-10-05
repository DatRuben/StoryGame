using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterActionPointType
{
    Center,
    Ground,
    Head,
    Mouth,
    LeftHand,
    RightHand,
    PrimaryCast,
    Custom
}

[Serializable]
public sealed class AdditionalCharacterActionPoint
{
    [SerializeField]
    private string id;

    [SerializeField]
    private Transform point;

    public string Id =>
        id;

    public Transform Point =>
        point;
}

public sealed class CharacterActionPoints :
    MonoBehaviour
{
    [Header("Setup")]

    [SerializeField]
    private Transform actionPointsRoot;

    public Transform ActionPointsRoot =>
        actionPointsRoot;

    [Header("Standard Action Points")]

    [SerializeField]
    private Transform center;

    [SerializeField]
    private Transform ground;

    [SerializeField]
    private Transform head;

    [SerializeField]
    private Transform mouth;

    [SerializeField]
    private Transform leftHand;

    [SerializeField]
    private Transform rightHand;

    [SerializeField]
    private Transform primaryCast;

    [Header("Additional Action Points")]

    [SerializeField]
    private List<AdditionalCharacterActionPoint>
        additionalPoints =
            new List<AdditionalCharacterActionPoint>();

    public Transform Center =>
        center;

    public Transform Ground =>
        ground;

    public Transform Head =>
        head;

    public Transform Mouth =>
        mouth;

    public Transform LeftHand =>
        leftHand;

    public Transform RightHand =>
        rightHand;

    public Transform PrimaryCast =>
        primaryCast;

    public Transform GetPoint(
        CharacterActionPointType type)
    {
        switch (type)
        {
            case CharacterActionPointType.Center:
                return center;

            case CharacterActionPointType.Ground:
                return ground;

            case CharacterActionPointType.Head:
                return head;

            case CharacterActionPointType.Mouth:
                return mouth;

            case CharacterActionPointType.LeftHand:
                return leftHand;

            case CharacterActionPointType.RightHand:
                return rightHand;

            case CharacterActionPointType.PrimaryCast:
                return primaryCast;

            default:
                return null;
        }
    }

    public bool TryGetPoint(
        CharacterActionPointType type,
        string customId,
        out Transform point)
    {
        if (type ==
            CharacterActionPointType.Custom)
        {
            return TryGetAdditionalPoint(
                customId,
                out point
            );
        }

        point =
            GetPoint(type);

        return point != null;
    }

    public bool TryGetAdditionalPoint(
        string id,
        out Transform point)
    {
        point = null;

        if (string.IsNullOrWhiteSpace(id) ||
            additionalPoints == null)
        {
            return false;
        }

        for (int i = 0;
             i < additionalPoints.Count;
             i++)
        {
            AdditionalCharacterActionPoint entry =
                additionalPoints[i];

            if (entry == null ||
                entry.Point == null ||
                !string.Equals(
                    entry.Id,
                    id,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            point =
                entry.Point;

            return true;
        }

        return false;
    }

    private void Reset()
    {
        Transform existingRoot =
            transform.Find(
                "ActionPoints"
            );

        if (existingRoot != null)
        {
            actionPointsRoot =
                existingRoot;

            return;
        }

        GameObject root =
            new GameObject(
                "ActionPoints"
            );

        root.transform.SetParent(
            transform,
            false
        );

        actionPointsRoot =
            root.transform;
    }
}