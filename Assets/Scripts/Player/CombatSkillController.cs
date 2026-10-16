using UnityEngine;

[RequireComponent(typeof(CombatSkillLoadout))]
[RequireComponent(typeof(PlayerGameplayState))]
public sealed class CombatSkillController :
    MonoBehaviour
{
    private CombatSkillLoadout skillLoadout;

    private PlayerGameplayState gameplayState;

    private CombatSkillExecution
        activeExecution;

    public CombatSkillExecution
        ActiveExecution =>
            activeExecution;

    public bool IsExecuting =>
        activeExecution != null;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (gameplayState != null)
        {
            gameplayState.OnCapabilitiesInterrupted +=
                HandleCapabilitiesInterrupted;
        }
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
        if (activeExecution == null)
            return;

        activeExecution.Tick(
            Time.deltaTime
        );

        if (activeExecution != null &&
            activeExecution.IsComplete)
        {
            activeExecution = null;
        }
    }

    public bool TryRequestSkill(
        int slotIndex)
    {
        return TryRequestSkill(
            slotIndex,
            null
        );
    }

    public bool TryRequestSkill(
        int slotIndex,
        InventoryItemInstance sourceItem)
    {
		ResolveReferences();

		if (skillLoadout == null)
			return false;

		CombatSkillDefinition skillDefinition =
			skillLoadout.GetSkill(
				slotIndex
			);

		if (skillDefinition == null)
			return false;

		if (gameplayState != null &&
			!gameplayState.Allows(
				PlayerGameplayCapability.Combat))
		{
			return false;
		}

		if (activeExecution != null)
		{
			return false;
		}

		CombatSkillRequestContext context =
			new CombatSkillRequestContext(
				gameObject,
				skillDefinition,
				sourceItem
			);

		if (!CombatSkillResolver.TryResolve(
				context,
				out ResolvedCombatSkill skill,
				out string resolveError))
		{
			Debug.LogError(
				resolveError,
				this
			);

			return false;
		}

		if (!CombatSkillOriginResolver.TryResolve(
		    context,
		    skill,
		    out CombatSkillOrigin origin,
		    out string originError))
		{
			Debug.LogError(
				originError,
				this
			);

			return false;
		}

		if (skill.Action == null)
        {
            Debug.LogError(
                $"Cannot use skill '{skill.SkillName}'. " +
                "No gameplay action is configured.",
                this
            );

            return false;
        }

        CombatSkillActionContext actionContext =
            new CombatSkillActionContext(
                gameObject,
                skill,
                sourceItem,
                origin
            );

        if (!skill.Action.TryCreateExecution(
                actionContext,
                out CombatSkillExecution execution,
                out string executionError))
        {
            Debug.LogError(
                executionError,
                this
            );

            return false;
        }

        if (execution == null)
        {
            Debug.LogError(
                $"Skill '{skill.SkillName}' created no execution.",
                this
            );

            return false;
        }

        if (!execution.TryBegin(
                out string beginError))
        {
            Debug.LogError(
                beginError,
                this
            );

            return false;
        }

        activeExecution =
            execution;

        return true;
    }

    private void ResolveReferences()
    {
        if (skillLoadout == null)
        {
            skillLoadout =
                GetComponent<
                    CombatSkillLoadout>();
        }

        if (gameplayState == null)
        {
            gameplayState =
                GetComponent<
                    PlayerGameplayState>();
        }
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
        if (activeExecution == null)
            return;

        activeExecution.Cancel();
        activeExecution = null;
    }
}