using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CombatHitbox :
    MonoBehaviour
{
    public event Action<DamageResult>
        OnDamageResolved;

    [Header("Damage")]

    [SerializeField]
    [Min(0f)]
    private float damageAmount = 10f;

    [SerializeField]
    private DamageType damageType =
        DamageType.Physical;

    [Header("Source")]

    [SerializeField]
    private GameObject source;

    private Collider hitboxCollider;

    private bool active;

    private readonly HashSet<CombatHurtbox>
        hitHurtboxes =
            new HashSet<CombatHurtbox>();

    private readonly HashSet<GameObject>
        hitTargets =
            new HashSet<GameObject>();

    public bool IsActive => active;

    private void Awake()
    {
        ResolveReferences();
        DeactivateHitbox();
    }

    private void ResolveReferences()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider =
                GetComponent<Collider>();
        }

        if (source == null)
        {
            EntityResources sourceResources =
                GetComponentInParent<
                    EntityResources>();

            source =
                sourceResources != null
                    ? sourceResources.gameObject
                    : transform.root.gameObject;
        }
    }

    public void ActivateHitbox()
    {
        ResolveReferences();

        if (hitboxCollider == null)
            return;

        hitHurtboxes.Clear();
        hitTargets.Clear();

        active = true;
        hitboxCollider.enabled = true;
    }

    public void DeactivateHitbox()
    {
        active = false;

        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }

        hitHurtboxes.Clear();
        hitTargets.Clear();
    }

    private void OnTriggerEnter(
        Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(
        Collider other)
    {
        TryHit(other);
    }

    private void TryHit(
        Collider other)
    {
        if (!active ||
            other == null)
        {
            return;
        }

        CombatHurtbox hurtbox =
            other.GetComponent<
                CombatHurtbox>();

        if (hurtbox == null)
            return;

        if (source != null &&
            (other.gameObject == source ||
             other.transform.IsChildOf(
                 source.transform)))
        {
            return;
        }

        EntityResources targetResources =
            other.GetComponentInParent<
                EntityResources>();

        GameObject expectedTarget =
            targetResources != null
                ? targetResources.gameObject
                : null;

        if (expectedTarget != null &&
            hitTargets.Contains(
                expectedTarget))
        {
            return;
        }

        if (!hitHurtboxes.Add(hurtbox))
            return;

        Vector3 hitPoint =
            other.ClosestPoint(
                transform.position
            );

        Vector3 hitDirection =
            other.bounds.center -
            transform.position;

        if (hitDirection.sqrMagnitude <
            0.001f)
        {
            hitDirection =
                transform.forward;
        }
        else
        {
            hitDirection.Normalize();
        }

        DamageContext damage =
            new DamageContext(
                damageAmount,
                damageType,
                source,
                hitPoint,
                hitDirection
            );

        DamageResult result =
            hurtbox.TakeDamage(damage);

        if (result.Target == null)
            return;

        hitTargets.Add(result.Target);

        OnDamageResolved?.Invoke(result);
    }

    private void OnDisable()
    {
        DeactivateHitbox();
    }

    private void OnValidate()
    {
        damageAmount =
            Mathf.Max(
                0f,
                damageAmount
            );

        Collider currentCollider =
            GetComponent<Collider>();

        if (currentCollider != null)
        {
            currentCollider.isTrigger = true;
        }
    }
}