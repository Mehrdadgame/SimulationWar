using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct FindTargetsJob : IJob
{
    [ReadOnly] public NativeArray<Entity> PotentialTargets;
    [ReadOnly] public NativeArray<LocalTransform> TargetTransforms;
    [ReadOnly] public NativeArray<UnitTypeComponent> TargetUnitTypes;

    public NativeArray<Entity> AIEntities;
    [ReadOnly] public NativeArray<LocalTransform> AITransforms;
    [ReadOnly] public NativeArray<AIComponent> AIComponents;
    [ReadOnly] public NativeArray<UnitTypeComponent> AIUnitTypes;

    public NativeArray<AttackRangeComponent> AttackRanges;

    public void Execute()
    {
        for (int aiIndex = 0; aiIndex < AIEntities.Length; aiIndex++)
        {
            var aiPos = AITransforms[aiIndex].Position;
            var ai = AIComponents[aiIndex];
            var aiUnitType = AIUnitTypes[aiIndex];
            var attackRange = AttackRanges[aiIndex];

            Entity closestEnemy = Entity.Null;
            float closestDistance = float.MaxValue;

            for (int targetIndex = 0; targetIndex < PotentialTargets.Length; targetIndex++)
            {
                var targetUnitType = TargetUnitTypes[targetIndex];
                if (targetUnitType.TeamId == aiUnitType.TeamId) continue;

                float distance = math.distance(aiPos, TargetTransforms[targetIndex].Position);
                if (distance <= ai.DetectionRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = PotentialTargets[targetIndex];
                }
            }

            if (closestEnemy != Entity.Null)
            {
                attackRange.Target = closestEnemy;
                attackRange.HasTarget = true;
                AttackRanges[aiIndex] = attackRange;
            }
        }
    }
}