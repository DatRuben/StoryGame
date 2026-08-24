using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DamageNumberManager :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private WorldDamageNumber
        damageNumberPrefab;

    [Header("Stacking")]

    [SerializeField]
    [Min(0.01f)]
    private float stackWindow = 0.35f;

    [Header("Cleanup")]

    [SerializeField]
    [Min(0.1f)]
    private float cleanupInterval = 1f;

    private Camera viewingCamera;

    private float nextCleanupTime;

    private readonly Dictionary<
        DamageStackKey,
        ActiveDamageStack>
        activeStacks =
            new Dictionary<
                DamageStackKey,
                ActiveDamageStack>();

    private readonly List<
        DamageStackKey>
        cleanupBuffer =
            new List<
                DamageStackKey>();

    public void BindViewer(
        Camera camera)
    {
        viewingCamera =
            camera;
    }

    public void ShowDamage(
        float amount,
        GameObject source,
        GameObject target,
        DamageType damageType,
        Vector3 hitPoint)
    {
        amount =
            Mathf.Max(
                0f,
                amount
            );

        if (amount <= 0f ||
            target == null ||
            damageNumberPrefab == null ||
            viewingCamera == null)
        {
            return;
        }

        DamageStackKey key =
            new DamageStackKey(
                source != null
                    ? source.GetInstanceID()
                    : 0,
                target.GetInstanceID(),
                damageType
            );

        if (activeStacks.TryGetValue(
                key,
                out ActiveDamageStack stack) &&
            stack.Number != null &&
            Time.time -
                stack.LastDamageTime <=
                stackWindow)
        {
            stack.Number.AddDamage(
                amount
            );

            stack.LastDamageTime =
                Time.time;

            return;
        }

        WorldDamageNumber number =
            Instantiate(
                damageNumberPrefab
            );

        number.Initialize(
            amount,
            hitPoint,
            viewingCamera
        );

        activeStacks[key] =
            new ActiveDamageStack(
                number,
                Time.time
            );
    }

    private void Update()
    {
        if (Time.time <
            nextCleanupTime)
        {
            return;
        }

        nextCleanupTime =
            Time.time +
            cleanupInterval;

        CleanupDestroyedStacks();
    }

    private void CleanupDestroyedStacks()
    {
        cleanupBuffer.Clear();

        foreach (
            KeyValuePair<
                DamageStackKey,
                ActiveDamageStack>
                pair in activeStacks)
        {
            if (pair.Value.Number == null)
            {
                cleanupBuffer.Add(
                    pair.Key
                );
            }
        }

        for (int i = 0;
             i < cleanupBuffer.Count;
             i++)
        {
            activeStacks.Remove(
                cleanupBuffer[i]
            );
        }
    }

    private sealed class ActiveDamageStack
    {
        public WorldDamageNumber Number;
        public float LastDamageTime;

        public ActiveDamageStack(
            WorldDamageNumber number,
            float lastDamageTime)
        {
            Number =
                number;

            LastDamageTime =
                lastDamageTime;
        }
    }

    private readonly struct DamageStackKey :
        IEquatable<DamageStackKey>
    {
        private readonly int sourceId;
        private readonly int targetId;

        private readonly DamageType
            damageType;

        public DamageStackKey(
            int sourceId,
            int targetId,
            DamageType damageType)
        {
            this.sourceId =
                sourceId;

            this.targetId =
                targetId;

            this.damageType =
                damageType;
        }

        public bool Equals(
            DamageStackKey other)
        {
            return
                sourceId ==
                    other.sourceId &&
                targetId ==
                    other.targetId &&
                damageType ==
                    other.damageType;
        }

        public override bool Equals(
            object obj)
        {
            return
                obj is DamageStackKey
                    other &&
                Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash =
                    hash * 31 +
                    sourceId;

                hash =
                    hash * 31 +
                    targetId;

                hash =
                    hash * 31 +
                    (int)damageType;

                return hash;
            }
        }
    }
}