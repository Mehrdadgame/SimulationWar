using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct CollisionDetectionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                          .CreateCommandBuffer(state.WorldUnmanaged);

        // Check projectile collisions
        foreach (var (projectileTransform, projectile, projectileEntity) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<ProjectileComponent>>()
                .WithEntityAccess())
        {
            foreach (var (unitTransform, health, unitType, unitEntity) in
                    SystemAPI.Query<RefRO<LocalTransform>, RefRW<HealthComponent>, RefRO<UnitTypeComponent>>()
                    .WithEntityAccess()
                    .WithNone<DeadTag>())
            {
                // Skip same team
                if (unitType.ValueRO.TeamId == projectile.ValueRO.TeamId) continue;

                float distance = math.distance(projectileTransform.ValueRO.Position, unitTransform.ValueRO.Position);

                if (distance <= 1f) // Collision threshold
                {
                    // Deal damage
                    health.ValueRW.CurrentHealth = math.max(0, health.ValueRO.CurrentHealth - projectile.ValueRO.Damage);
                    health.ValueRW.IsDead = health.ValueRO.CurrentHealth <= 0;

                    // Create hit effect
                    Entity hitEffect = ecb.CreateEntity();
                    ecb.AddComponent(hitEffect, new LocalTransform
                    {
                        Position = unitTransform.ValueRO.Position,
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                    ecb.AddComponent(hitEffect, new EffectComponent
                    {
                        Type = EffectType.Hit,
                        Duration = 0.5f,
                        StartTime = (float)SystemAPI.Time.ElapsedTime,
                        Position = unitTransform.ValueRO.Position
                    });

                    // Destroy projectile
                    ecb.DestroyEntity(projectileEntity);
                    break;
                }
            }
        }
    }
}
