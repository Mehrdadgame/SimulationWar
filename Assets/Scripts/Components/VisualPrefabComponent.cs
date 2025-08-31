using Unity.Entities;
using Unity.Mathematics;

public struct VisualPrefabComponent : IComponentData
{
    public Entity PrefabEntity;
    public float3 Scale;
}