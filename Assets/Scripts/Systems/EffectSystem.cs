using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct EffectSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float currentTime = (float)SystemAPI.Time.ElapsedTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                          .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (effect, entity) in SystemAPI.Query<RefRO<EffectComponent>>()
                .WithEntityAccess())
        {
            if (currentTime - effect.ValueRO.StartTime >= effect.ValueRO.Duration)
            {
                ecb.DestroyEntity(entity);
            }
        }
    }
}