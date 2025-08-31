using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct ProjectileSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                          .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (transform, projectile, entity) in
                SystemAPI.Query<RefRW<LocalTransform>, RefRW<ProjectileComponent>>()
                .WithEntityAccess())
        {
            // Move projectile
            transform.ValueRW.Position += projectile.ValueRO.Direction * projectile.ValueRO.Speed * deltaTime;

            // Check lifetime
            projectile.ValueRW.LifeTime -= deltaTime;
            if (projectile.ValueRO.LifeTime <= 0)
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            // Check collision with target
            if (SystemAPI.Exists(projectile.ValueRO.Target))
            {
                float3 targetPos = SystemAPI.GetComponent<LocalTransform>(projectile.ValueRO.Target).Position;
                float distance = math.distance(transform.ValueRO.Position, targetPos);

                if (distance <= 1f) // Hit threshold
                {
                    // Deal damage
                    if (SystemAPI.HasComponent<HealthComponent>(projectile.ValueRO.Target))
                    {
                        var health = SystemAPI.GetComponent<HealthComponent>(projectile.ValueRO.Target);
                        health.CurrentHealth = math.max(0, health.CurrentHealth - projectile.ValueRO.Damage);
                        health.IsDead = health.CurrentHealth <= 0;

                        SystemAPI.SetComponent(projectile.ValueRO.Target, health);
                    }

                    // Create hit effect
                    CreateHitEffect(ecb, targetPos, ref state);

                    // Destroy projectile
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }

    private void CreateHitEffect(EntityCommandBuffer ecb, float3 position, ref SystemState state)
    {
        Entity effect = ecb.CreateEntity();

        ecb.AddComponent(effect, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        ecb.AddComponent(effect, new EffectComponent
        {
            Type = EffectType.Hit,
            Duration = 1f,
            StartTime = (float)SystemAPI.Time.ElapsedTime,
            Position = position
        });
    }
}