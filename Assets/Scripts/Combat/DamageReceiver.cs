public interface DamageReceiver
{
    DamageResult TakeDamage(
        DamageContext damage
    );
}