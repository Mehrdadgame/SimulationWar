using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct PathfindingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (pathfinding, movement, transform, waypoints) in
                SystemAPI.Query<RefRW<PathfindingComponent>, RefRW<MovementComponent>,
                               RefRO<LocalTransform>, DynamicBuffer<WaypointBuffer>>()
                .WithNone<DeadTag>())
        {
            if (pathfinding.ValueRO.NeedsPath)
            {
                // Simple pathfinding - direct line for now
                // In a real implementation, you'd use A* algorithm here
                waypoints.Clear();
                waypoints.Add(new WaypointBuffer { Position = pathfinding.ValueRO.FinalDestination });

                pathfinding.ValueRW.NeedsPath = false;
                pathfinding.ValueRW.CurrentWaypointIndex = 0;
            }

            // Follow waypoints
            if (waypoints.Length > 0 && pathfinding.ValueRO.CurrentWaypointIndex < waypoints.Length)
            {
                float3 currentWaypoint = waypoints[pathfinding.ValueRO.CurrentWaypointIndex].Position;
                float distance = math.distance(transform.ValueRO.Position, currentWaypoint);

                if (distance <= movement.ValueRO.StoppingDistance)
                {
                    pathfinding.ValueRW.CurrentWaypointIndex++;

                    if (pathfinding.ValueRO.CurrentWaypointIndex >= waypoints.Length)
                    {
                        movement.ValueRW.HasDestination = false;
                    }
                    else
                    {
                        movement.ValueRW.Destination = waypoints[pathfinding.ValueRO.CurrentWaypointIndex].Position;
                    }
                }
                else
                {
                    movement.ValueRW.Destination = currentWaypoint;
                    movement.ValueRW.HasDestination = true;
                }
            }
        }
    }
}