using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

public class MinimapSystem : MonoBehaviour
{
    [Header("Minimap Settings")]
    public RenderTexture MinimapTexture;
    public Camera MinimapCamera;
    public RectTransform MinimapUI;
    public float MapSize = 100f;

    [Header("Unit Icons")]
    public GameObject PlayerUnitIcon;
    public GameObject EnemyUnitIcon;

    private EntityManager entityManager;
    private Transform iconParent;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        iconParent = MinimapUI.transform;

        InvokeRepeating(nameof(UpdateMinimap), 0f, 0.2f);
    }

    void UpdateMinimap()
    {
        // Clear existing icons
        foreach (Transform child in iconParent)
        {
            if (child.name.StartsWith("MinimapIcon"))
            {
                child.gameObject.SetActive(false);
            }
        }

        int iconIndex = 0;

        // Show player units
        var playerQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadOnly<UnitTypeComponent>(),
            ComponentType.ReadOnly<PlayerUnitTag>(),
            ComponentType.Exclude<DeadTag>()
        );

        using var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);
        using var playerTransforms = playerQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        for (int i = 0; i < playerTransforms.Length; i++)
        {
            CreateMinimapIcon(playerTransforms[i].Position, true, iconIndex++);
        }

        // Show enemy units
        var enemyQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadOnly<UnitTypeComponent>(),
            ComponentType.Exclude<PlayerUnitTag>(),
            ComponentType.Exclude<DeadTag>()
        );

        using var enemyEntities = enemyQuery.ToEntityArray(Allocator.TempJob);
        using var enemyTransforms = enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        for (int i = 0; i < enemyTransforms.Length; i++)
        {
            CreateMinimapIcon(enemyTransforms[i].Position, false, iconIndex++);
        }
    }

    void CreateMinimapIcon(float3 worldPosition, bool isPlayer, int index)
    {
        string iconName = $"MinimapIcon_{index}";
        Transform existingIcon = iconParent.Find(iconName);

        GameObject icon;
        if (existingIcon != null)
        {
            icon = existingIcon.gameObject;
        }
        else
        {
            GameObject prefab = isPlayer ? PlayerUnitIcon : EnemyUnitIcon;
            icon = Instantiate(prefab, iconParent);
            icon.name = iconName;
        }

        icon.SetActive(true);

        // Convert world position to minimap position
        Vector2 minimapPos = WorldToMinimapPosition(worldPosition);
        icon.GetComponent<RectTransform>().anchoredPosition = minimapPos;
    }

    Vector2 WorldToMinimapPosition(float3 worldPos)
    {
        float normalizedX = (worldPos.x + MapSize * 0.5f) / MapSize;
        float normalizedZ = (worldPos.z + MapSize * 0.5f) / MapSize;

        float minimapSize = MinimapUI.rect.width;

        return new Vector2(
            (normalizedX - 0.5f) * minimapSize,
            (normalizedZ - 0.5f) * minimapSize
        );
    }

    public void OnMinimapClick(Vector2 minimapPosition)
    {
        // Convert minimap click to world position
        float3 worldPos = MinimapToWorldPosition(minimapPosition);

        // Move camera to clicked position
        Camera.main.transform.position = new Vector3(worldPos.x, Camera.main.transform.position.y, worldPos.z);
    }

    float3 MinimapToWorldPosition(Vector2 minimapPos)
    {
        float minimapSize = MinimapUI.rect.width;

        float normalizedX = (minimapPos.x / minimapSize) + 0.5f;
        float normalizedZ = (minimapPos.y / minimapSize) + 0.5f;

        return new float3(
            (normalizedX * MapSize) - MapSize * 0.5f,
            0,
            (normalizedZ * MapSize) - MapSize * 0.5f
        );
    }

    void OnDestroy()
    {
        // Clean up any remaining queries if needed
    }
}