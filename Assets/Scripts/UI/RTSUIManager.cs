using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RTSUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject SelectionIndicator;
    public RectTransform HealthBarPrefab;
    public Canvas WorldCanvas;
    public Transform HealthBarParent;

    [Header("Info Panel")]
    public UnityEngine.UI.Text SelectedUnitsText;
    public UnityEngine.UI.Text GameStatsText;

    private EntityManager entityManager;
    private RTSGameManager gameManager;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        gameManager = FindObjectOfType<RTSGameManager>();

        InvokeRepeating(nameof(UpdateUI), 0f, 0.1f);
    }

    void UpdateUI()
    {
        UpdateSelectedUnitsInfo();
        UpdateGameStats();
        UpdateHealthBars();
    }

    void UpdateSelectedUnitsInfo()
    {
        if (SelectedUnitsText == null) return;

        var query = entityManager.CreateEntityQuery(typeof(GroupComponent), typeof(SelectedTag), typeof(PlayerUnitTag));
        int selectedCount = query.CalculateEntityCount();

        SelectedUnitsText.text = $"Selected Units: {selectedCount}";
    }

    void UpdateGameStats()
    {
        if (GameStatsText == null) return;

        int playerUnits = 0;
        int enemyUnits = 0;

        // Query all units with UnitTypeComponent, excluding dead ones
        var unitQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<UnitTypeComponent>(),
            ComponentType.Exclude<DeadTag>()
        );

        using var entities = unitQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
        using var unitTypes = unitQuery.ToComponentDataArray<UnitTypeComponent>(Unity.Collections.Allocator.TempJob);

        // Count units by team
        for (int i = 0; i < unitTypes.Length; i++)
        {
            if (unitTypes[i].TeamId == 0)
                playerUnits++;
            else if (unitTypes[i].TeamId == 1)
                enemyUnits++;
        }

        GameStatsText.text = $"Player Units: {playerUnits}\nEnemy Units: {enemyUnits}";
    }

    void UpdateHealthBars()
    {
        // Clear existing health bars
        foreach (Transform child in HealthBarParent)
        {
            if (child.gameObject.activeInHierarchy)
                child.gameObject.SetActive(false);
        }

        // Create health bars for selected units using EntityQuery
        int healthBarIndex = 0;
        var query = entityManager.CreateEntityQuery(new Unity.Entities.EntityQueryDesc
        {
            All = new Unity.Entities.ComponentType[] { typeof(HealthComponent), typeof(LocalTransform), typeof(SelectedTag) },
            None = new Unity.Entities.ComponentType[] { typeof(DeadTag) }
        });
        using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
        {
            foreach (var entity in entities)
            {
                var health = entityManager.GetComponentData<HealthComponent>(entity);
                var localTransform = entityManager.GetComponentData<LocalTransform>(entity);
                CreateHealthBar(localTransform.Position, health, healthBarIndex++);
            }
        }
    }

    void CreateHealthBar(float3 worldPosition, HealthComponent health, int index)
    {
        if (HealthBarPrefab == null || WorldCanvas == null) return;

        RectTransform healthBar;
        if (index < HealthBarParent.childCount)
        {
            healthBar = HealthBarParent.GetChild(index).GetComponent<RectTransform>();
            healthBar.gameObject.SetActive(true);
        }
        else
        {
            healthBar = Instantiate(HealthBarPrefab, HealthBarParent);
        }

        // Convert world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint((Vector3)worldPosition + Vector3.up * 2f);
        healthBar.position = screenPos;

        // Update health bar fill
        var healthBarFill = healthBar.GetComponentInChildren<UnityEngine.UI.Image>();
        if (healthBarFill != null)
        {
            float healthPercent = health.CurrentHealth / health.MaxHealth;
            healthBarFill.fillAmount = healthPercent;

            // Color coding
            if (healthPercent > 0.6f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
    }

    // Public methods for spawning units via UI
    public void SpawnInfantry()
    {
        Vector3 spawnPos = gameManager.PlayerSpawnPoint.position + GetRandomOffset();
        gameManager.SpawnPlayerUnit(UnitType.Infantry, spawnPos);
    }

    public void SpawnCavalry()
    {
        Vector3 spawnPos = gameManager.PlayerSpawnPoint.position + GetRandomOffset();
        gameManager.SpawnPlayerUnit(UnitType.Cavalry, spawnPos);
    }

    public void SpawnArcher()
    {
        Vector3 spawnPos = gameManager.PlayerSpawnPoint.position + GetRandomOffset();
        gameManager.SpawnPlayerUnit(UnitType.Archer, spawnPos);
    }

    public void SpawnDinosaur()
    {
        Vector3 spawnPos = gameManager.PlayerSpawnPoint.position + GetRandomOffset();
        gameManager.SpawnPlayerUnit(UnitType.Dinosaur, spawnPos);
    }

    Vector3 GetRandomOffset()
    {
        return new Vector3(
            UnityEngine.Random.Range(-3f, 3f),
            0f,
            UnityEngine.Random.Range(-3f, 3f)
        );
    }
}
