using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct CleanupSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                          .CreateCommandBuffer(state.WorldUnmanaged);

        // Clean up dead entities after a delay
        float currentTime = (float)SystemAPI.Time.ElapsedTime;

        foreach (var (health, entity) in SystemAPI.Query<RefRO<HealthComponent>>()
                .WithEntityAccess()
                .WithAll<DeadTag>())
        {
            // Remove dead entities after 3 seconds
            if (currentTime > 3f) // Simple cleanup timer
            {
                ecb.DestroyEntity(entity);
            }
        }
    }
}