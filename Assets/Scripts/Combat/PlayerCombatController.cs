using System;
using UnityEngine;

[RequireComponent(
    typeof(PlayerWeaponDeployment))]
[RequireComponent(
    typeof(PlayerWeaponDeployment))]
[RequireComponent(typeof(PlayerGameplayState))]

public sealed class PlayerCombatController :
    MonoBehaviour
{
    public event Action<DamageResult>
    OnDamageResolved;

    [SerializeField]
    [Min(0.01f)]
    private float hitRadius = 0.35f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugCast = true;

    [SerializeField]
    [Min(0.1f)]
    private float debugCastDuration = 1f;

    private Vector3 lastCastOrigin;
    private Vector3 lastCastEnd;

    private float lastCastRadius;
    private float debugCastVisibleUntil;

    private bool lastCastHit;

    private readonly RaycastHit[] hitBuffer =
        new RaycastHit[16];

    private PlayerWeaponDeployment
        weaponDeployment;

    private Collider bodyCollider;

    private PlayerGameplayState gameplayState;

    private void Awake()
    {
        weaponDeployment =
            GetComponent<PlayerWeaponDeployment>();

        bodyCollider =
            GetComponent<Collider>();

        gameplayState =
            GetComponent<PlayerGameplayState>();
    }

    public bool TryPrimaryAttack()
    {
        if (gameplayState != null &&
            !gameplayState.Allows(
            PlayerGameplayCapability.Combat))
        {
            return false;
        }

        if (weaponDeployment == null ||
            !weaponDeployment
                .TryGetPrimaryDeployedWeapon(
                    out InventoryItemInstance weapon))
        {
            return false;
        }

        ItemDefinition definition =
            weapon.Definition;

        if (definition == null)
            return false;

        Vector3 origin =
            bodyCollider != null
                ? bodyCollider.bounds.center
                : transform.position;

        Vector3 direction =
            transform.forward;

        RecordDebugCast(
            origin,
            direction,
            definition.attackReach,
            false
        );

        int hitCount =
            Physics.SphereCastNonAlloc(
                origin,
                hitRadius,
                direction,
                hitBuffer,
                definition.attackReach,
                ~0,
                QueryTriggerInteraction.Collide
            );

        if (hitCount <= 0)
            return false;

        CombatHurtbox nearestHurtbox = null;

        Vector3 nearestHitPoint =
            Vector3.zero;

        float nearestDistance =
            float.PositiveInfinity;

        for (int i = 0;
             i < hitCount;
             i++)
        {
            Collider candidate =
                hitBuffer[i].collider;

            if (candidate == null)
                continue;

            Transform candidateTransform =
                candidate.transform;

            if (candidateTransform == transform ||
                candidateTransform.IsChildOf(
                    transform))
            {
                continue;
            }

            if (hitBuffer[i].distance >=
                nearestDistance)
            {
                continue;
            }

            CombatHurtbox hurtbox =
                candidate.GetComponent<
                    CombatHurtbox>();

            if (hurtbox == null)
                continue;

            nearestDistance =
                hitBuffer[i].distance;

            nearestHurtbox =
                hurtbox;

            nearestHitPoint =
                hitBuffer[i].point;
        }

        if (nearestHurtbox == null)
            return false;

        RecordDebugCast(
            origin,
            direction,
            definition.attackReach,
            true
        );

        DamageContext damage =
            new DamageContext(
                definition.baseDamage,
                definition.damageType,
                gameObject,
                nearestHitPoint,
                direction
            );

        DamageResult result =
            nearestHurtbox.TakeDamage(
                damage
            );

        if (result.Target != null)
        {
            OnDamageResolved?.Invoke(
                result
            );
        }

        return result.DidDamage;
    }

    private void RecordDebugCast(
        Vector3 origin,
        Vector3 direction,
        float reach,
        bool hit)
    {
        if (!showDebugCast)
            return;

        lastCastOrigin =
            origin;

        lastCastEnd =
            origin +
            direction.normalized * reach;

        lastCastRadius =
            hitRadius;

        lastCastHit =
            hit;

        debugCastVisibleUntil =
            Time.time +
            debugCastDuration;

        Debug.DrawLine(
            lastCastOrigin,
            lastCastEnd,
            hit
                ? Color.green
                : Color.red,
            debugCastDuration
        );
    }

    private void OnDrawGizmos()
    {
        if (!showDebugCast ||
            !Application.isPlaying ||
            Time.time >
                debugCastVisibleUntil)
        {
            return;
        }

        Gizmos.color =
            lastCastHit
                ? Color.green
                : Color.red;

        Gizmos.DrawLine(
            lastCastOrigin,
            lastCastEnd
        );

        Gizmos.DrawWireSphere(
            lastCastOrigin,
            lastCastRadius
        );

        Gizmos.DrawWireSphere(
            lastCastEnd,
            lastCastRadius
        );
    }
}