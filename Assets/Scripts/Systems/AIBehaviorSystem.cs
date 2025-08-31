using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct AIBehaviorSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float currentTime = (float)SystemAPI.Time.ElapsedTime;

        foreach (var (ai, unitType, transform, movement, attackRange) in
                SystemAPI.Query<RefRW<AIComponent>, RefRO<UnitTypeComponent>, RefRO<LocalTransform>,
                               RefRW<MovementComponent>, RefRW<AttackRangeComponent>>()
                .WithNone<DeadTag>()
                .WithNone<PlayerUnitTag>()) // Only AI units
        {
            // Search for targets periodically
            if (currentTime - ai.ValueRO.LastTargetSearchTime > ai.ValueRO.TargetSearchInterval)
            {
                Entity closestEnemy = FindClosestEnemy(ref state, transform.ValueRO.Position,
                                                    ai.ValueRO.DetectionRange, unitType.ValueRO.TeamId);

                if (closestEnemy != Entity.Null)
                {
                    attackRange.ValueRW.Target = closestEnemy;
                    attackRange.ValueRW.HasTarget = true;

                    // Move towards target if not in attack range
                    if (SystemAPI.HasComponent<LocalTransform>(closestEnemy))
                    {
                        float3 targetPos = SystemAPI.GetComponent<LocalTransform>(closestEnemy).Position;
                        float distanceToTarget = math.distance(transform.ValueRO.Position, targetPos);

                        if (distanceToTarget > attackRange.ValueRO.Range)
                        {
                            movement.ValueRW.Destination = targetPos;
                            movement.ValueRW.HasDestination = true;
                        }
                    }
                }

                ai.ValueRW.LastTargetSearchTime = currentTime;
            }
        }
    }

    private Entity FindClosestEnemy(ref SystemState state, float3 position, float detectionRange, int teamId)
    {
        Entity closestEnemy = Entity.Null;
        float closestDistance = float.MaxValue;

        foreach (var (enemyTransform, enemyUnitType, entity) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<UnitTypeComponent>>()
                .WithEntityAccess()
                .WithNone<DeadTag>())
        {
            if (enemyUnitType.ValueRO.TeamId == teamId) continue; // Same team

            float distance = math.distance(position, enemyTransform.ValueRO.Position);
            if (distance <= detectionRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = entity;
            }
        }

        return closestEnemy;
    }
}

