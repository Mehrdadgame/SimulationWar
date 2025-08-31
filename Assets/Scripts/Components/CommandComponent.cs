using Unity.Entities;
using Unity.Mathematics;

public struct CommandComponent : IComponentData, IEnableableComponent
{
    public CommandType Type;
    public float3 TargetPosition;
    public Entity TargetEntity;
    public float CommandTime;
}
public enum CommandType
{
    Move,
    Attack,
    Stop,
    Patrol
}