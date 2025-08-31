using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RTSGameManager : MonoBehaviour
{
    [Header("Unit Prefabs")]
    public GameObject InfantryPrefab;
    public GameObject CavalryPrefab;
    public GameObject ArcherPrefab;
    public GameObject DinosaurPrefab;

    [Header("Game Settings")]
    public int InitialPlayerUnits = 10;
    public int InitialEnemyUnits = 15;
    public float SpawnRadius = 20f;

    [Header("Team Positions")]
    public Transform PlayerSpawnPoint;
    public Transform EnemySpawnPoint;

    private EntityManager entityManager;
    private BlobAssetStore blobAssetStore;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAssetStore = new BlobAssetStore();

        SpawnInitialUnits();
    }

    void SpawnInitialUnits()
    {
        // Spawn player units
        SpawnUnitsForTeam(0, PlayerSpawnPoint.position, InitialPlayerUnits, true);

        // Spawn enemy units
        SpawnUnitsForTeam(1, EnemySpawnPoint.position, InitialEnemyUnits, false);
    }

    void SpawnUnitsForTeam(int teamId, float3 centerPosition, int unitCount, bool isPlayer)
    {
        for (int i = 0; i < unitCount; i++)
        {
            UnitType unitType = GetRandomUnitType();
            float3 spawnPos = centerPosition + GetRandomSpawnOffset();

            SpawnUnit(unitType, spawnPos, teamId, isPlayer);
        }
    }

    UnitType GetRandomUnitType()
    {
        int random = UnityEngine.Random.Range(0, 4);
        return (UnitType)random;
    }

    float3 GetRandomSpawnOffset()
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = UnityEngine.Random.Range(2f, SpawnRadius);

        return new float3(
            math.cos(angle) * distance,
            0,
            math.sin(angle) * distance
        );
    }

    void SpawnUnit(UnitType unitType, float3 position, int teamId, bool isPlayer)
    {
        Entity unit = entityManager.CreateEntity();

        // Add basic components
        entityManager.AddComponent<LocalTransform>(unit);
        entityManager.SetComponentData(unit, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        // Add unit stats based on type
        var stats = GetUnitStats(unitType);

        entityManager.AddComponentData(unit, new HealthComponent
        {
            CurrentHealth = stats.health,
            MaxHealth = stats.health,
            IsDead = false
        });

        entityManager.AddComponentData(unit, new DamageComponent
        {
            DamageAmount = stats.damage,
            AttackSpeed = stats.attackSpeed,
            LastAttackTime = 0f
        });

        entityManager.AddComponentData(unit, new MovementComponent
        {
            Speed = stats.speed,
            Destination = position,
            HasDestination = false,
            StoppingDistance = 1.5f
        });

        entityManager.AddComponentData(unit, new AttackRangeComponent
        {
            Range = stats.range,
            Target = Entity.Null,
            HasTarget = false
        });

        entityManager.AddComponentData(unit, new UnitTypeComponent
        {
            Type = unitType,
            TeamId = teamId
        });

        // Add AI for enemy units
        if (!isPlayer)
        {
            entityManager.AddComponentData(unit, new AIComponent
            {
                BehaviorType = AIBehaviorType.Aggressive,
                DetectionRange = stats.range + 5f,
                LastTargetSearchTime = 0f,
                TargetSearchInterval = 1f
            });
        }
        else
        {
            entityManager.AddComponent<PlayerUnitTag>(unit);
        }

        // Add group component
        entityManager.AddComponentData(unit, new GroupComponent
        {
            GroupId = teamId,
            IsSelected = false
        });

        // Add pathfinding for cavalry and complex units
        if (unitType == UnitType.Cavalry || unitType == UnitType.Dinosaur)
        {
            entityManager.AddComponentData(unit, new PathfindingComponent
            {
                NeedsPath = false,
                CurrentWaypointIndex = 0,
                FinalDestination = position
            });

            entityManager.AddBuffer<WaypointBuffer>(unit);
        }

        // Add command component for player units
        if (isPlayer)
        {
            entityManager.AddComponentData(unit, new CommandComponent
            {
                Type = CommandType.Stop,
                TargetPosition = position,
                TargetEntity = Entity.Null,
                CommandTime = 0f
            });
        }
    }

    (float health, float damage, float speed, float range, float attackSpeed) GetUnitStats(UnitType unitType)
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

    void OnDestroy()
    {
        if (blobAssetStore.IsCreated)
            blobAssetStore.Dispose();
    }

    // Public methods for UI integration
    public void SpawnPlayerUnit(UnitType unitType, Vector3 position)
    {
        SpawnUnit(unitType, position, 0, true);
    }

    public void SpawnEnemyUnit(UnitType unitType, Vector3 position)
    {
        SpawnUnit(unitType, position, 1, false);
    }
}