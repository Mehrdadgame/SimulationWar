using Unity.Entities;
using Unity.Mathematics;

public struct EffectComponent : IComponentData
{
    public EffectType Type;
    public float Duration;
    public float StartTime;
    public float3 Position;
}
public enum EffectType
{
    Death,
    Hit,
    Muzzleflash,
    Explosion
}