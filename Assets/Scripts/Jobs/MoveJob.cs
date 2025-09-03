using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(ref LocalTransform transform,
                       in CommandComponent command)
    {
        if (command.Type != CommandType.Move) return;

        float3 direction = math.normalize(command.TargetPosition - transform.Position);
        float distance = math.distance(transform.Position, command.TargetPosition);

        if (distance > 0.1f)
        {
            // سرعت ثابت 5 واحد در ثانیه (یا از component دیگری بگیرید)
            float speed = 5f;
            transform.Position += direction * speed * DeltaTime;
            transform.Rotation = quaternion.LookRotationSafe(direction, math.up());
        }
    }
}