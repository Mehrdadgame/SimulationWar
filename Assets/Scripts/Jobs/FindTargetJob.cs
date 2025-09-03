using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct FindTargetJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    [ReadOnly] public ComponentLookup<UnitTypeComponent> UnitTypeLookup;
    [ReadOnly] public ComponentLookup<DeadTag> DeadTagLookup;

    public void Execute(Entity entity,
                       ref AttackRangeComponent attackRange,
                       in LocalTransform transform,
                       in UnitTypeComponent unitType)
    {
        if (attackRange.HasTarget) return;

        float closestDistance = float.MaxValue;
        Entity closestTarget = Entity.Null;

        // این روش کارآمد نیست - باید از Spatial Partitioning استفاده کنید
        // فقط برای نمونه نوشته شده

        // بهتر است از NativeMultiHashMap یا Grid system استفاده کنید
    }
}