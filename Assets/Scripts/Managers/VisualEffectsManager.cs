using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject HitEffectPrefab;
    public GameObject DeathEffectPrefab;
    public GameObject MuzzleFlashPrefab;
    public GameObject ProjectilePrefab;

    [Header("Selection Visual")]
    public GameObject SelectionRingPrefab;

    private EntityManager entityManager;
    private EntityQuery effectQuery;
    private EntityQuery selectedUnitsQuery;
    private EntityQuery projectileQuery;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        effectQuery = entityManager.CreateEntityQuery(typeof(EffectComponent), typeof(LocalTransform));
        selectedUnitsQuery = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(SelectedTag));
        projectileQuery = entityManager.CreateEntityQuery(typeof(ProjectileComponent), typeof(LocalTransform));

        InvokeRepeating(nameof(UpdateVisualEffects), 0f, 0.1f);
    }

    void UpdateVisualEffects()
    {
        UpdateEffects();
        UpdateSelectionVisuals();
        UpdateProjectileVisuals();
    }

    void UpdateEffects()
    {
        var effects = effectQuery.ToEntityArray(Allocator.TempJob);
        var transforms = effectQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var effectComponents = effectQuery.ToComponentDataArray<EffectComponent>(Allocator.TempJob);

        for (int i = 0; i < effects.Length; i++)
        {
            var effect = effectComponents[i];
            var transform = transforms[i];

            GameObject effectPrefab = GetEffectPrefab(effect.Type);
            if (effectPrefab != null)
            {
                GameObject instance = Instantiate(effectPrefab, transform.Position, transform.Rotation);
                Destroy(instance, effect.Duration);
            }
        }

        effects.Dispose();
        transforms.Dispose();
        effectComponents.Dispose();
    }

    GameObject GetEffectPrefab(EffectType effectType)
    {
        return effectType switch
        {
            EffectType.Hit => HitEffectPrefab,
            EffectType.Death => DeathEffectPrefab,
            EffectType.Muzzleflash => MuzzleFlashPrefab,
            _ => null
        };
    }

    void UpdateSelectionVisuals()
    {
        // Clear existing selection visuals
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("SelectionRing"))
            {
                child.gameObject.SetActive(false);
            }
        }

        // Create selection rings for selected units
        var selectedEntities = selectedUnitsQuery.ToEntityArray(Allocator.TempJob);
        var selectedTransforms = selectedUnitsQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        for (int i = 0; i < selectedEntities.Length; i++)
        {
            if (SelectionRingPrefab != null)
            {
                GameObject ring = GetOrCreateSelectionRing(i);
                ring.transform.position = selectedTransforms[i].Position;
                ring.SetActive(true);
            }
        }

        selectedEntities.Dispose();
        selectedTransforms.Dispose();
    }

    GameObject GetOrCreateSelectionRing(int index)
    {
        string ringName = $"SelectionRing_{index}";
        Transform existingRing = transform.Find(ringName);

        if (existingRing != null)
        {
            return existingRing.gameObject;
        }

        GameObject newRing = Instantiate(SelectionRingPrefab, transform);
        newRing.name = ringName;
        return newRing;
    }

    void UpdateProjectileVisuals()
    {
        var projectileEntities = projectileQuery.ToEntityArray(Allocator.TempJob);
        var projectileTransforms = projectileQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        for (int i = 0; i < projectileEntities.Length; i++)
        {
            // Create or update projectile visual
            if (ProjectilePrefab != null)
            {
                string projectileName = $"Projectile_{projectileEntities[i].Index}";
                GameObject projectileVisual = GameObject.Find(projectileName);

                if (projectileVisual == null)
                {
                    projectileVisual = Instantiate(ProjectilePrefab);
                    projectileVisual.name = projectileName;
                }

                projectileVisual.transform.position = projectileTransforms[i].Position;
                projectileVisual.transform.rotation = projectileTransforms[i].Rotation;
            }
        }

        projectileEntities.Dispose();
        projectileTransforms.Dispose();
    }
}