using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CombatHitbox))]
public sealed class CombatProjectile :
    MonoBehaviour
{
    private Rigidbody projectileBody;
    private CombatHitbox hitbox;

    private GameObject source;

    private float speed;
    private float remainingLifetime;

    private bool initialized;

    private void Awake()
    {
        projectileBody =
            GetComponent<Rigidbody>();

        hitbox =
            GetComponent<CombatHitbox>();

        projectileBody.useGravity = false;
        projectileBody.isKinematic = true;
    }

    public void Initialize(
        GameObject source,
        float damage,
        DamageType damageType,
        float speed,
        float lifetime)
    {
        this.source =
            source;

        this.speed =
            Mathf.Max(
                0f,
                speed
            );

        remainingLifetime =
            Mathf.Max(
                0.01f,
                lifetime
            );

        hitbox.Configure(
            damage,
            damageType,
            source
        );

        hitbox.ActivateHitbox();

        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        projectileBody.MovePosition(
            projectileBody.position +
            transform.forward *
            speed *
            Time.fixedDeltaTime
        );

        remainingLifetime -=
            Time.fixedDeltaTime;

        if (remainingLifetime <= 0f)
        {
            Destroy(
                gameObject
            );
        }
    }

    private void OnTriggerEnter(
        Collider other)
    {
        if (!initialized ||
            other == null)
        {
            return;
        }

        if (source != null &&
            (other.gameObject == source ||
             other.transform.IsChildOf(
                 source.transform)))
        {
            return;
        }

        Destroy(
            gameObject
        );
    }
}