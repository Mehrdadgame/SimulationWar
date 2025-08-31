using Unity.Entities;

public struct GroupComponent : IComponentData
{
    public int GroupId;
    public bool IsSelected;
}
