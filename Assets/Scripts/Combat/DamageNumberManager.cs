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

    private PlayerCombatController combatController;

    private float nextCleanupTime;

    private readonly List<ActiveDamageStack>
        activeStacks =
            new List<ActiveDamageStack>();

    public void BindPlayer(
        PlayerCombatController
            newCombatController,
        Camera camera)
    {
        if (combatController != null)
        {
            combatController.OnDamageResolved -=
                HandleDamageResolved;
        }

        combatController =
            newCombatController;

        viewingCamera =
            camera;

        if (combatController != null)
        {
            combatController.OnDamageResolved +=
                HandleDamageResolved;
        }
    }

    public void ShowDamage(
        DamageResult result)
    {
        if (!result.DidDamage ||
            result.Target == null ||
            damageNumberPrefab == null ||
            viewingCamera == null)
        {
            return;
        }

        GameObject source =
            result.Context.Source;

        GameObject target =
            result.Target;

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
                result.HealthDamage,
                result.SoulBarrierDamage
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
            result.HealthDamage,
            result.SoulBarrierDamage,
            result.Context.HitPoint,
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

    private void HandleDamageResolved(
        DamageResult result)
    {
        ShowDamage(
            result
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

    private void OnDestroy()
    {
        if (combatController != null)
        {
            combatController.OnDamageResolved -=
                HandleDamageResolved;
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