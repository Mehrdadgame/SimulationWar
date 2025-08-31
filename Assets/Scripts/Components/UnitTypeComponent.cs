using Unity.Entities;
public enum UnitType
{
    Infantry,
    Cavalry,
    Archer,
    Dinosaur
}
public struct UnitTypeComponent : IComponentData
{
    public UnitType Type;
    public int TeamId;
}