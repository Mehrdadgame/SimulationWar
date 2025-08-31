using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class UnitSpawnerSystem : SystemBase
{
    private EntityArchetype infantryArchetype;
    private EntityArchetype cavalryArchetype;
    private EntityArchetype archerArchetype;
    private EntityArchetype dinosaurArchetype;

    protected override void OnCreate()
    {
        // Create archetypes for efficient spawning
        infantryArchetype = EntityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(HealthComponent),
            typeof(DamageComponent),
            typeof(MovementComponent),
            typeof(AttackRangeComponent),
            typeof(UnitTypeComponent),
            typeof(GroupComponent)
        );

        cavalryArchetype = EntityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(HealthComponent),
            typeof(DamageComponent),
            typeof(MovementComponent),
            typeof(AttackRangeComponent),
            typeof(UnitTypeComponent),
            typeof(GroupComponent),
            typeof(PathfindingComponent)
        );

        archerArchetype = EntityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(HealthComponent),
            typeof(DamageComponent),
            typeof(MovementComponent),
            typeof(AttackRangeComponent),
            typeof(UnitTypeComponent),
            typeof(GroupComponent)
        );

        dinosaurArchetype = EntityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(HealthComponent),
            typeof(DamageComponent),
            typeof(MovementComponent),
            typeof(AttackRangeComponent),
            typeof(UnitTypeComponent),
            typeof(GroupComponent),
            typeof(PathfindingComponent)
        );
    }

    protected override void OnUpdate()
    {
        // This system handles efficient unit spawning
    }

    public Entity SpawnUnitOptimized(UnitType unitType, float3 position, int teamId, bool isPlayer)
    {
        EntityArchetype archetype = GetArchetypeForUnitType(unitType);
        Entity unit = EntityManager.CreateEntity(archetype);

        var stats = GetUnitStats(unitType);

        EntityManager.SetComponentData(unit, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        EntityManager.SetComponentData(unit, new HealthComponent
        {
            CurrentHealth = stats.health,
            MaxHealth = stats.health,
            IsDead = false
        });

        EntityManager.SetComponentData(unit, new DamageComponent
        {
            DamageAmount = stats.damage,
            AttackSpeed = stats.attackSpeed,
            LastAttackTime = 0f
        });

        EntityManager.SetComponentData(unit, new MovementComponent
        {
            Speed = stats.speed,
            Destination = position,
            HasDestination = false,
            StoppingDistance = 1.5f
        });

        EntityManager.SetComponentData(unit, new AttackRangeComponent
        {
            Range = stats.range,
            Target = Entity.Null,
            HasTarget = false
        });

        EntityManager.SetComponentData(unit, new UnitTypeComponent
        {
            Type = unitType,
            TeamId = teamId
        });

        EntityManager.SetComponentData(unit, new GroupComponent
        {
            GroupId = teamId,
            IsSelected = false
        });

        // Add AI for enemy units
        if (!isPlayer)
        {
            EntityManager.SetComponentData(unit, new AIComponent
            {
                BehaviorType = AIBehaviorType.Aggressive,
                DetectionRange = stats.range + 5f,
                LastTargetSearchTime = 0f,
                TargetSearchInterval = UnityEngine.Random.Range(0.8f, 1.2f)
            });
        }
        else
        {
            EntityManager.AddComponent<PlayerUnitTag>(unit);
            EntityManager.AddComponent<CommandComponent>(unit);
            EntityManager.SetComponentData(unit, new CommandComponent
            {
                Type = CommandType.Stop,
                TargetPosition = position,
                TargetEntity = Entity.Null,
                CommandTime = 0f
            });
        }

        // Add pathfinding buffer for units that need it
        if (unitType == UnitType.Cavalry || unitType == UnitType.Dinosaur)
        {
            EntityManager.SetComponentData(unit, new PathfindingComponent
            {
                NeedsPath = false,
                CurrentWaypointIndex = 0,
                FinalDestination = position
            });

            var buffer = EntityManager.AddBuffer<WaypointBuffer>(unit);
        }

        return unit;
    }

    private EntityArchetype GetArchetypeForUnitType(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Infantry => infantryArchetype,
            UnitType.Cavalry => cavalryArchetype,
            UnitType.Archer => archerArchetype,
            UnitType.Dinosaur => dinosaurArchetype,
            _ => infantryArchetype
        };
    }

    private (float health, float damage, float speed, float range, float attackSpeed) GetUnitStats(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Infantry => (100f, 25f, 3f, 2f, 1f),
            UnitType.Cavalry => (150f, 35f, 6f, 2.5f, 0.8f),
            UnitType.Archer => (80f, 30f, 2.5f, 8f, 1.2f),
            UnitType.Dinosaur => (300f, 50f, 4f, 3f, 0.6f),
            _ => (100f, 25f, 3f, 2f, 1f)
        };
    }
}