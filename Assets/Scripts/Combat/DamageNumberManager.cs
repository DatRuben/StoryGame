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

    private readonly List<ActiveDamageStack>
        activeStacks =
            new List<ActiveDamageStack>();

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

        for (int i =
                 activeStacks.Count - 1;
             i >= 0;
             i--)
        {
            ActiveDamageStack stack =
                activeStacks[i];

            if (stack.Number == null)
            {
                activeStacks.RemoveAt(i);
                continue;
            }

            if (stack.Source != source ||
                stack.Target != target)
            {
                continue;
            }

            if (Time.time -
                    stack.LastDamageTime >
                stackWindow)
            {
                continue;
            }

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

        activeStacks.Add(
            new ActiveDamageStack(
                source,
                target,
                number,
                Time.time
            )
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
        for (int i =
                 activeStacks.Count - 1;
             i >= 0;
             i--)
        {
            if (activeStacks[i].Number ==
                null)
            {
                activeStacks.RemoveAt(i);
            }
        }
    }

    private sealed class ActiveDamageStack
    {
        public GameObject Source;
        public GameObject Target;

        public WorldDamageNumber Number;

        public float LastDamageTime;

        public ActiveDamageStack(
            GameObject source,
            GameObject target,
            WorldDamageNumber number,
            float lastDamageTime)
        {
            Source =
                source;

            Target =
                target;

            Number =
                number;

            LastDamageTime =
                lastDamageTime;
        }
    }
}