using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    [Header("Unit Stats")]
    public UnitType UnitType;
    public float Health = 100f;
    public float Damage = 25f;
    public float MovementSpeed = 3f;
    public float AttackRange = 2f;
    public float AttackSpeed = 1f;
    public int TeamId = 0;

    [Header("AI Settings")]
    public bool HasAI = false;
    public AIBehaviorType BehaviorType = AIBehaviorType.Aggressive;
    public float DetectionRange = 10f;

    [Header("Pathfinding")]
    public bool UsePathfinding = false;

    public class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new HealthComponent
            {
                CurrentHealth = authoring.Health,
                MaxHealth = authoring.Health,
                IsDead = false
            });

            AddComponent(entity, new DamageComponent
            {
                DamageAmount = authoring.Damage,
                AttackSpeed = authoring.AttackSpeed,
                LastAttackTime = 0f
            });

            AddComponent(entity, new MovementComponent
            {
                Speed = authoring.MovementSpeed,
                Destination = authoring.transform.position,
                HasDestination = false,
                StoppingDistance = 1.5f
            });

            AddComponent(entity, new AttackRangeComponent
            {
                Range = authoring.AttackRange,
                Target = Entity.Null,
                HasTarget = false
            });

            AddComponent(entity, new UnitTypeComponent
            {
                Type = authoring.UnitType,
                TeamId = authoring.TeamId
            });

            AddComponent(entity, new GroupComponent
            {
                GroupId = authoring.TeamId,
                IsSelected = false
            });

            if (authoring.HasAI)
            {
                AddComponent(entity, new AIComponent
                {
                    BehaviorType = authoring.BehaviorType,
                    DetectionRange = authoring.DetectionRange,
                    LastTargetSearchTime = 0f,
                    TargetSearchInterval = 1f
                });
            }
            else
            {
                AddComponent<PlayerUnitTag>(entity);
                AddComponent(entity, new CommandComponent
                {
                    Type = CommandType.Stop,
                    TargetPosition = authoring.transform.position,
                    TargetEntity = Entity.Null,
                    CommandTime = 0f
                });
            }

            if (authoring.UsePathfinding)
            {
                AddComponent(entity, new PathfindingComponent
                {
                    NeedsPath = false,
                    CurrentWaypointIndex = 0,
                    FinalDestination = authoring.transform.position
                });

                AddBuffer<WaypointBuffer>(entity);
            }
        }
    }
}
