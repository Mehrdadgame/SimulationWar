using Unity.Entities;

public struct DamageComponent : IComponentData
{
    public float DamageAmount;
    public float AttackSpeed;
    public float LastAttackTime;
}
