using Unity.Entities;

public struct AIComponent : IComponentData
{
    public AIBehaviorType BehaviorType;
    public float DetectionRange;
    public float LastTargetSearchTime;
    public float TargetSearchInterval;
}

public enum AIBehaviorType
{
    Aggressive,
    Defensive,
    Patrol,
    Guard
}