using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EntityResources))]
public sealed class SimpleEnemyAttackAI :
    MonoBehaviour
{
    private static readonly int AttackTrigger =
        Animator.StringToHash("Attack");

    [Header("Attack")]

    [SerializeField]
    [Min(0f)]
    private float attackRange = 5.5f;

    [SerializeField]
    [Min(0f)]
    private float attackCooldown = 2f;

    [Header("Turning")]

    [SerializeField]
    [Min(0f)]
    private float turnSpeed = 360f;

    private Animator animator;
    private EntityResources resources;

    private Transform target;
    private EntityResources targetResources;

    private float nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        resources = GetComponent<EntityResources>();
    }

    private void Update()
    {
        if (resources.IsHealthDepleted)
        {
            animator.enabled = false;
            enabled = false;
            return;
        }

        if (target == null)
        {
            FindPlayer();
        }

        if (target == null)
            return;

        if (targetResources != null &&
            targetResources.IsHealthDepleted)
        {
            return;
        }

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude >
            attackRange * attackRange)
        {
            return;
        }

        FaceTarget(direction);

        if (Time.time < nextAttackTime)
            return;

        animator.SetTrigger(AttackTrigger);

        nextAttackTime =
            Time.time + attackCooldown;
    }

    private void FindPlayer()
    {
        PlayerCharacterProfile player =
            FindFirstObjectByType<
                PlayerCharacterProfile>();

        if (player == null)
            return;

        target = player.transform;

        targetResources =
            player.GetComponent<EntityResources>();
    }

    private void FaceTarget(
        Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
    }

    private void OnDisable()
    {
        CombatHitbox[] hitboxes =
            GetComponentsInChildren<
                CombatHitbox>(true);

        for (int i = 0;
             i < hitboxes.Length;
             i++)
        {
            hitboxes[i].DeactivateHitbox();
        }
    }
}