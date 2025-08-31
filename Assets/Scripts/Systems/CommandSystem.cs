using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct CommandSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (command, movement, attackRange, entity) in
                SystemAPI.Query<RefRW<CommandComponent>, RefRW<MovementComponent>, RefRW<AttackRangeComponent>>()
                .WithEntityAccess()
                .WithNone<DeadTag>())
        {
            switch (command.ValueRO.Type)
            {
                case CommandType.Move:
                    movement.ValueRW.Destination = command.ValueRO.TargetPosition;
                    movement.ValueRW.HasDestination = true;
                    attackRange.ValueRW.HasTarget = false;
                    break;

                case CommandType.Attack:
                    if (command.ValueRO.TargetEntity != Entity.Null)
                    {
                        attackRange.ValueRW.Target = command.ValueRO.TargetEntity;
                        attackRange.ValueRW.HasTarget = true;
                    }
                    break;

                case CommandType.Stop:
                    movement.ValueRW.HasDestination = false;
                    attackRange.ValueRW.HasTarget = false;
                    break;
            }

            // Clear command after processing
            SystemAPI.SetComponentEnabled<CommandComponent>(entity, false);
        }
    }
}