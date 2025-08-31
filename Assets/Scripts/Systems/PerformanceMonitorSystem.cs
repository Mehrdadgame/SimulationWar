using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial class PerformanceMonitorSystem : SystemBase
{
    private float lastUpdateTime;
    private int lastEntityCount;

    protected override void OnUpdate()
    {
        if (SystemAPI.Time.ElapsedTime - lastUpdateTime > 1f) // Update every second
        {
            var allEntitiesQuery = GetEntityQuery(ComponentType.ReadOnly<LocalTransform>());
            int currentEntityCount = allEntitiesQuery.CalculateEntityCount();

            Debug.Log($"[RTS Performance] Entities: {currentEntityCount}, FPS: {1f / SystemAPI.Time.DeltaTime:F1}");

            lastUpdateTime = (float)SystemAPI.Time.ElapsedTime;
            lastEntityCount = currentEntityCount;
        }
    }
}