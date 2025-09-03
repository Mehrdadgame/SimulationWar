using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct AttackJob : IJobEntity
{
    public float CurrentTime;
    public EntityCommandBuffer.ParallelWriter ECB;

    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    public ComponentLookup<HealthComponent> HealthLookup;
    [ReadOnly] public ComponentLookup<DeadTag> DeadTagLookup;

    public void Execute([EntityIndexInQuery] int entityInQueryIndex,
                       Entity entity,
                       ref AttackRangeComponent attackRange,
                       ref DamageComponent damage,
                       in UnitTypeComponent unitType,
                       in LocalTransform transform)
    {
        if (!attackRange.HasTarget) return;

        Entity target = attackRange.Target;

        // بررسی وجود هدف
        if (!TransformLookup.HasComponent(target) || DeadTagLookup.HasComponent(target))
        {
            attackRange.HasTarget = false;
            return;
        }

        float3 targetPos = TransformLookup[target].Position;
        float distanceToTarget = math.distance(transform.Position, targetPos);

        if (distanceToTarget <= attackRange.Range)
        {
            // بررسی کولداون حمله
            if (CurrentTime - damage.LastAttackTime >= 1f / damage.AttackSpeed)
            {
                if (unitType.Type == UnitType.Archer)
                {
                    CreateProjectile(entityInQueryIndex, transform.Position, targetPos,
                                   target, damage.DamageAmount, unitType.TeamId);
                }
                else
                {
                    DealDirectDamage(target, damage.DamageAmount);
                }

                damage.LastAttackTime = CurrentTime;
            }
        }
    }

    private void CreateProjectile(int jobIndex, float3 startPos, float3 targetPos,
                                Entity target, float damage, int teamId)
    {
        Entity projectile = ECB.CreateEntity(jobIndex);
        float3 direction = math.normalize(targetPos - startPos);

        ECB.AddComponent(jobIndex, projectile, new LocalTransform
        {
            Position = startPos,
            Rotation = quaternion.LookRotationSafe(direction, math.up()),
            Scale = 1f
        });

        ECB.AddComponent(jobIndex, projectile, new ProjectileComponent
        {
            Speed = 20f,
            Direction = direction,
            Target = target,
            Damage = damage,
            LifeTime = 5f,
            TeamId = teamId
        });
    }

    private void DealDirectDamage(Entity target, float damageAmount)
    {
        if (HealthLookup.HasComponent(target))
        {
            var health = HealthLookup[target];
            health.CurrentHealth = math.max(0, health.CurrentHealth - damageAmount);
            health.IsDead = health.CurrentHealth <= 0;
            HealthLookup[target] = health;
        }
    }
}