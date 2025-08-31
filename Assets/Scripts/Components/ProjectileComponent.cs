using Unity.Entities;
using Unity.Mathematics;

public struct ProjectileComponent : IComponentData
{
    public float Speed;
    public float3 Direction;
    public Entity Target;
    public float Damage;
    public float LifeTime;
    public int TeamId;
}
