using UnityEngine;

[RequireComponent(
    typeof(PlayerWeaponDeployment))]
public sealed class PlayerCombatController :
    MonoBehaviour
{
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

    private void Awake()
    {
        weaponDeployment =
            GetComponent<PlayerWeaponDeployment>();

        bodyCollider =
            GetComponent<Collider>();
    }

    public bool TryPrimaryAttack()
    {
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
                QueryTriggerInteraction.Ignore
            );

        if (hitCount > 0)
        {
            RecordDebugCast(
                origin,
                direction,
                definition.attackReach,
                true
            );
        }

        if (hitCount <= 0)
            return false;

        Collider nearestCollider = null;

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

            nearestDistance =
                hitBuffer[i].distance;

            nearestCollider =
                candidate;
        }

        if (nearestCollider == null)
            return false;

        DamageReceiver receiver =
            FindDamageReceiver(
                nearestCollider.transform
            );

        if (receiver == null)
            return false;

        receiver.TakeDamage(
            definition.baseDamage,
            definition.damageType
        );

        return true;
    }

    private DamageReceiver FindDamageReceiver(
        Transform target)
    {
        Transform current =
            target;

        while (current != null)
        {
            MonoBehaviour[] behaviours =
                current.GetComponents<
                    MonoBehaviour>();

            for (int i = 0;
                 i < behaviours.Length;
                 i++)
            {
                if (behaviours[i] is
                    DamageReceiver receiver)
                {
                    return receiver;
                }
            }

            current =
                current.parent;
        }

        return null;
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