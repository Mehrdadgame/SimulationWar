using Unity.Entities;

public struct AttackRangeComponent : IComponentData
{
    public float Range;
    public Entity Target;
    public bool HasTarget;
}