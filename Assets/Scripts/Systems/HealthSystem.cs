using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct HealthSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                          .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (health, entity) in SystemAPI.Query<RefRO<HealthComponent>>()
                .WithEntityAccess()
                .WithNone<DeadTag>())
        {
            if (health.ValueRO.IsDead)
            {
                // Add dead tag
                ecb.AddComponent<DeadTag>(entity);

                // Create death effect
                if (SystemAPI.HasComponent<LocalTransform>(entity))
                {
                    float3 deathPos = SystemAPI.GetComponent<LocalTransform>(entity).Position;
                    CreateDeathEffect(ref state, ecb, deathPos);
                }

                // Remove from groups and clear targets
                ecb.RemoveComponent<GroupComponent>(entity);
                ecb.RemoveComponent<SelectedTag>(entity);
            }
        }
    }

    private void CreateDeathEffect(ref SystemState state, EntityCommandBuffer ecb, float3 position)
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
            Type = EffectType.Death,
            Duration = 2f,
            StartTime = (float)SystemAPI.Time.ElapsedTime,
            Position = position
        });
    }
}
