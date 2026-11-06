using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Combat/Effects/Damage")]
public sealed class DamageSkillHitEffectDefinition :
    CombatSkillHitEffectDefinition
{
    [Header("Damage")]
    [SerializeField, Min(0f)]
    private float baseDamage = 10f;

    [SerializeField]
    private DamageType damageType = DamageType.Aether;

    [Header("Distance Scaling")]
    [SerializeField, Min(0f)]
    private float startingMultiplier = 1f;

    [SerializeField, Min(0f)]
    private float endingMultiplier = 2f;

    public override ICombatSkillHitEffect CreateRuntimeEffect()
    {
        return new RuntimeDamageEffect(
            Mathf.Max(0f, baseDamage),
            damageType,
            Mathf.Max(0f, startingMultiplier),
            Mathf.Max(0f, endingMultiplier));
    }

    private sealed class RuntimeDamageEffect :
        ICombatSkillHitEffect
    {
        private readonly float damage;
        private readonly DamageType damageType;
        private readonly float startingMultiplier;
        private readonly float endingMultiplier;

        public RuntimeDamageEffect(
            float damage,
            DamageType damageType,
            float startingMultiplier,
            float endingMultiplier)
        {
            this.damage = damage;
            this.damageType = damageType;
            this.startingMultiplier = startingMultiplier;
            this.endingMultiplier = endingMultiplier;
        }

        public void Apply(CombatSkillHitContext context)
        {
            if (context.Target == null ||
                context.Action == null ||
                context.Action.Actor == null)
            {
                return;
            }


            float distanceMultiplier = Mathf.Lerp(
                startingMultiplier,
                endingMultiplier,
                context.NormalizedDistance);

            float damageMultiplier =
                CombatValueResolver.ResolveMultiplier(
                    context.Action.Actor,
                    CombatValueType.Damage);

            float finalDamage =
                damage *
                distanceMultiplier *
                damageMultiplier;

            DamageContext damageContext = new DamageContext(
                finalDamage,
                damageType,
                context.Action.Actor,
                context.Point,
                context.Direction);


            context.Target.TakeDamage(damageContext);
        }
    }
}
