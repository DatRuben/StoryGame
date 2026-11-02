
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CombatSkillLoadout))]
[RequireComponent(typeof(PlayerGameplayState))]
[RequireComponent(typeof(EntityResources))]
public sealed class CombatSkillController : MonoBehaviour
{
    private CombatSkillLoadout skillLoadout;
    private PlayerGameplayState gameplayState;
    private EntityResources resources;

    private PendingCast pendingCast;
    private CombatSkillExecution activeExecution;

    private readonly Dictionary<CombatSkillDefinition, float>
        cooldownReadyTimes =
            new Dictionary<CombatSkillDefinition, float>();

    public CombatSkillExecution ActiveExecution =>
        activeExecution;

    public bool IsCasting => pendingCast != null;

    public bool IsExecuting => activeExecution != null;

    public bool IsBusy => IsCasting || IsExecuting;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (gameplayState == null)
            return;

        gameplayState.OnCapabilitiesInterrupted -=
            HandleCapabilitiesInterrupted;

        gameplayState.OnCapabilitiesInterrupted +=
            HandleCapabilitiesInterrupted;
    }

    private void OnDisable()
    {
        if (gameplayState != null)
        {
            gameplayState.OnCapabilitiesInterrupted -=
                HandleCapabilitiesInterrupted;
        }

        CancelActiveExecution();
    }

    private void Update()
    {
        if (pendingCast != null)
        {
            pendingCast.RemainingTime -= Time.deltaTime;

            if (pendingCast.RemainingTime <= 0f)
            {
                PendingCast completedCast = pendingCast;
                pendingCast = null;

                BeginExecution(
                    completedCast.Skill,
                    completedCast.Origin,
                    completedCast.Execution);
            }

            return;
        }

        if (activeExecution == null)
            return;

        activeExecution.Tick(Time.deltaTime);

        if (activeExecution != null &&
            activeExecution.IsComplete)
        {
            activeExecution = null;
        }
    }

    public bool TryRequestSkill(int slotIndex)
    {
        return TryRequestSkill(slotIndex, null);
    }

    public bool TryRequestSkill(
        int slotIndex,
        InventoryItemInstance sourceItem)
    {
        ResolveReferences();

        if (skillLoadout == null ||
            gameplayState == null ||
            resources == null ||
            IsBusy)
        {
            return false;
        }

        if (!gameplayState.Allows(
                PlayerGameplayCapability.Combat))
        {
            return false;
        }

        CombatSkillDefinition definition =
            skillLoadout.GetSkill(slotIndex);

        if (definition == null)
            return false;

        if (IsOnCooldown(definition))
            return false;

        CombatSkillRequestContext request =
            new CombatSkillRequestContext(
                gameObject,
                definition,
                sourceItem);

        if (!CombatSkillResolver.TryResolve(
                request,
                out ResolvedCombatSkill skill,
                out string resolveError))
        {
            Debug.LogError(resolveError, this);
            return false;
        }

        if (!resources.CanSpendAether(skill.AetherCost))
            return false;

        if (!CombatSkillOriginResolver.TryResolve(
                request,
                skill,
                out CombatSkillOrigin origin,
                out string originError))
        {
            Debug.LogError(originError, this);
            return false;
        }

        if (skill.Action == null)
        {
            Debug.LogError(
                $"Skill '{skill.SkillName}' has no gameplay action.",
                this);
            return false;
        }

        CombatSkillActionContext actionContext =
            new CombatSkillActionContext(
                gameObject,
                skill,
                sourceItem,
                origin);

        if (!skill.Action.TryCreateExecution(
                actionContext,
                out CombatSkillExecution execution,
                out string executionError))
        {
            Debug.LogError(executionError, this);
            return false;
        }

        if (execution == null)
        {
            Debug.LogError(
                $"Skill '{skill.SkillName}' created no execution.",
                this);
            return false;
        }

        if (skill.CastTime > 0f)
        {
            pendingCast = new PendingCast(
                skill,
                origin,
                execution);

            return true;
        }

        return BeginExecution(skill, origin, execution);
    }

    private bool BeginExecution(
        ResolvedCombatSkill skill,
        CombatSkillOrigin origin,
        CombatSkillExecution execution)
    {
        if (!gameplayState.Allows(
                PlayerGameplayCapability.Combat))
        {
            execution.Cancel();
            return false;
        }

        if (skill.OriginSource !=
                CombatSkillOriginSource.None &&
            (origin == null ||
             !origin.TryGetPose(out _)))
        {
            execution.Cancel();

            Debug.LogWarning(
                $"Skill '{skill.SkillName}' lost its origin.",
                this);

            return false;
        }

        if (!resources.SpendAether(skill.AetherCost))
        {
            execution.Cancel();
            return false;
        }

        if (!execution.TryBegin(out string beginError))
        {
            // A failed execution must not consume resources.
            if (skill.AetherCost > 0f)
                resources.AddAether(skill.AetherCost);

            execution.Cancel();

            Debug.LogError(beginError, this);
            return false;
        }

        cooldownReadyTimes[skill.Definition] =
            Time.time + skill.Cooldown;

        if (!execution.IsComplete)
        {
            activeExecution = execution;
        }

        return true;
    }

    public bool IsOnCooldown(
        CombatSkillDefinition definition)
    {
        if (definition == null)
            return false;

        return cooldownReadyTimes.TryGetValue(
                   definition,
                   out float readyTime) &&
               Time.time < readyTime;
    }

    public float GetRemainingCooldown(
        CombatSkillDefinition definition)
    {
        if (definition == null ||
            !cooldownReadyTimes.TryGetValue(
                definition,
                out float readyTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, readyTime - Time.time);
    }

    private void HandleCapabilitiesInterrupted(
        PlayerGameplayCapability capabilities)
    {
        if ((capabilities &
             PlayerGameplayCapability.Combat) == 0)
        {
            return;
        }

        CancelActiveExecution();
    }

    public void CancelActiveExecution()
    {
        if (pendingCast != null)
        {
            pendingCast.Execution.Cancel();
            pendingCast = null;
        }

        if (activeExecution != null)
        {
            activeExecution.Cancel();
            activeExecution = null;
        }
    }

    private void ResolveReferences()
    {
        if (skillLoadout == null)
            skillLoadout = GetComponent<CombatSkillLoadout>();

        if (gameplayState == null)
            gameplayState = GetComponent<PlayerGameplayState>();

        if (resources == null)
            resources = GetComponent<EntityResources>();
    }

    private sealed class PendingCast
    {
        public ResolvedCombatSkill Skill { get; }
        public CombatSkillOrigin Origin { get; }
        public CombatSkillExecution Execution { get; }
        public float RemainingTime { get; set; }

        public PendingCast(
            ResolvedCombatSkill skill,
            CombatSkillOrigin origin,
            CombatSkillExecution execution)
        {
            Skill = skill;
            Origin = origin;
            Execution = execution;
            RemainingTime = skill.CastTime;
        }
    }
}
