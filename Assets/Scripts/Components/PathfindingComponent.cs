using Unity.Entities;
using Unity.Mathematics;

public struct PathfindingComponent : IComponentData
{
    public bool NeedsPath;
    public int CurrentWaypointIndex;
    public float3 FinalDestination;
}