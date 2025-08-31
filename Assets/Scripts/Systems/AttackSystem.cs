using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct AttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float currentTime = (float)SystemAPI.Time.ElapsedTime;
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                          .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (attackRange, damage, unitType, transform, entity) in
                SystemAPI.Query<RefRW<AttackRangeComponent>, RefRO<DamageComponent>,
                               RefRO<UnitTypeComponent>, RefRO<LocalTransform>>()
                .WithNone<DeadTag>()
                .WithEntityAccess())
        {
            if (!attackRange.ValueRO.HasTarget) continue;

            Entity target = attackRange.ValueRO.Target;
            if (!SystemAPI.Exists(target) || SystemAPI.HasComponent<DeadTag>(target))
            {
                attackRange.ValueRW.HasTarget = false;
                continue;
            }

            float3 targetPos = SystemAPI.GetComponent<LocalTransform>(target).Position;
            float distanceToTarget = math.distance(transform.ValueRO.Position, targetPos);

            if (distanceToTarget <= attackRange.ValueRO.Range)
            {
                // Check attack cooldown
                if (currentTime - damage.ValueRO.LastAttackTime >= 1f / damage.ValueRO.AttackSpeed)
                {
                    // Create projectile for ranged units
                    if (unitType.ValueRO.Type == UnitType.Archer)
                    {
                        CreateProjectile(ecb, transform.ValueRO.Position, targetPos, target,
                                       damage.ValueRO.DamageAmount, unitType.ValueRO.TeamId);
                    }
                    else
                    {
                        // Direct damage for melee units
                        DealDamage(ref state, target, damage.ValueRO.DamageAmount);
                    }

                    // Update last attack time
                    SystemAPI.SetComponent(entity,
                                         new DamageComponent
                                         {
                                             DamageAmount = damage.ValueRO.DamageAmount,
                                             AttackSpeed = damage.ValueRO.AttackSpeed,
                                             LastAttackTime = currentTime
                                         });
                }
            }
        }
    }

    private void CreateProjectile(EntityCommandBuffer ecb, float3 startPos, float3 targetPos,
                                Entity target, float damage, int teamId)
    {
        Entity projectile = ecb.CreateEntity();

        float3 direction = math.normalize(targetPos - startPos);

        ecb.AddComponent(projectile, new LocalTransform
        {
            Position = startPos,
            Rotation = quaternion.LookRotationSafe(direction, math.up()),
            Scale = 1f
        });

        ecb.AddComponent(projectile, new ProjectileComponent
        {
            Speed = 20f,
            Direction = direction,
            Target = target,
            Damage = damage,
            LifeTime = 5f,
            TeamId = teamId
        });
    }

    private void DealDamage(ref SystemState state, Entity target, float damageAmount)
    {
        if (SystemAPI.HasComponent<HealthComponent>(target))
        {
            var health = SystemAPI.GetComponent<HealthComponent>(target);
            health.CurrentHealth = math.max(0, health.CurrentHealth - damageAmount);
            health.IsDead = health.CurrentHealth <= 0;

            SystemAPI.SetComponent(target, health);
        }
    }
}