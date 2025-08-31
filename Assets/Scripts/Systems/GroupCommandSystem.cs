using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct GroupCommandSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Handle group movement commands
        foreach (var (group, command, movement, entity) in
                SystemAPI.Query<RefRO<GroupComponent>, RefRO<CommandComponent>, RefRW<MovementComponent>>()
                .WithAll<SelectedTag>()
                .WithNone<DeadTag>()
                .WithEntityAccess())
        {
            if (command.ValueRO.Type == CommandType.Move)
            {
                // For pathfinding units, set up pathfinding
                if (SystemAPI.HasComponent<PathfindingComponent>(entity))
                {
                    var pathfinding = SystemAPI.GetComponentRW<PathfindingComponent>(entity);
                    pathfinding.ValueRW.FinalDestination = command.ValueRO.TargetPosition;
                    pathfinding.ValueRW.NeedsPath = true;
                    pathfinding.ValueRW.CurrentWaypointIndex = 0;
                }
                else
                {
                    // Direct movement
                    movement.ValueRW.Destination = command.ValueRO.TargetPosition;
                    movement.ValueRW.HasDestination = true;
                }
            }
        }
    }
}