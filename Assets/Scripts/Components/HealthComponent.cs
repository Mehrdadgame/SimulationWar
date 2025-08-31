using Unity.Entities;

public struct HealthComponent : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;
    public bool IsDead;
}
