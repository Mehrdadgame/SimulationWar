using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class UnitVisualManager : MonoBehaviour
{
    [Header("Unit Models")]
    public GameObject InfantryModel;
    public GameObject CavalryModel;
    public GameObject ArcherModel;
    public GameObject DinosaurModel;

    [Header("Team Materials")]
    public Material PlayerTeamMaterial;
    public Material EnemyTeamMaterial;

    private EntityManager entityManager;
    private EntityQuery unitQuery;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        unitQuery = entityManager.CreateEntityQuery(
            typeof(LocalTransform),
            typeof(UnitTypeComponent),
            typeof(HealthComponent)
        );

        InvokeRepeating(nameof(UpdateUnitVisuals), 0f, 0.1f);
    }

    void UpdateUnitVisuals()
    {
        var entities = unitQuery.ToEntityArray(Allocator.TempJob);
        var transforms = unitQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var unitTypes = unitQuery.ToComponentDataArray<UnitTypeComponent>(Allocator.TempJob);
        var healths = unitQuery.ToComponentDataArray<HealthComponent>(Allocator.TempJob);

        for (int i = 0; i < entities.Length; i++)
        {
            string visualName = $"UnitVisual_{entities[i].Index}";
            GameObject visual = GameObject.Find(visualName);

            if (healths[i].IsDead)
            {
                if (visual != null)
                {
                    Destroy(visual);
                }
                continue;
            }

            if (visual == null)
            {
                GameObject modelPrefab = GetModelForUnitType(unitTypes[i].Type);
                if (modelPrefab != null)
                {
                    visual = Instantiate(modelPrefab);
                    visual.name = visualName;

                    // Apply team material
                    var renderers = visual.GetComponentsInChildren<Renderer>();
                    Material teamMaterial = unitTypes[i].TeamId == 0 ? PlayerTeamMaterial : EnemyTeamMaterial;

                    foreach (var renderer in renderers)
                    {
                        if (teamMaterial != null)
                            renderer.material = teamMaterial;
                    }
                }
            }

            if (visual != null)
            {
                visual.transform.position = transforms[i].Position;
                visual.transform.rotation = transforms[i].Rotation;
                visual.transform.localScale = Vector3.one * transforms[i].Scale;
            }
        }

        entities.Dispose();
        transforms.Dispose();
        unitTypes.Dispose();
        healths.Dispose();
    }

    GameObject GetModelForUnitType(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Infantry => InfantryModel,
            UnitType.Cavalry => CavalryModel,
            UnitType.Archer => ArcherModel,
            UnitType.Dinosaur => DinosaurModel,
            _ => InfantryModel
        };
    }
}