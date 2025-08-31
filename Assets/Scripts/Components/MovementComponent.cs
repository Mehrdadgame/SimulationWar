using Unity.Entities;
using Unity.Mathematics;

public struct MovementComponent : IComponentData
{
    public float Speed;
    public float3 Destination;
    public bool HasDestination;
    public float StoppingDistance;
}
