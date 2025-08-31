using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RTSInputManager : MonoBehaviour
{
    [Header("Selection")]
    public LayerMask GroundLayerMask = 1;
    public Material SelectionBoxMaterial;

    [Header("UI")]
    public KeyCode[] GroupHotkeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

    private Camera mainCamera;
    private bool isSelecting;
    private Vector3 selectionStartPos;
    private EntityManager entityManager;
    private World world;

    void Start()
    {
        mainCamera = Camera.main;
        world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;
    }

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
        HandleGroupSelection();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }
        else if (Input.GetMouseButton(0) && isSelecting)
        {
            UpdateSelection();
        }
        else if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            EndSelection();
        }

        if (Input.GetMouseButtonDown(1))
        {
            IssueCommand();
        }
    }

    void StartSelection()
    {
        isSelecting = true;
        selectionStartPos = Input.mousePosition;

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            ClearAllSelections();
        }
    }

    void UpdateSelection()
    {
        // Visual feedback handled in OnGUI
    }

    void EndSelection()
    {
        isSelecting = false;

        Vector3 selectionEndPos = Input.mousePosition;
        Rect selectionRect = GetSelectionRect(selectionStartPos, selectionEndPos);

        if (selectionRect.width > 5 && selectionRect.height > 5)
        {
            SelectUnitsInRect(selectionRect);
        }
        else
        {
            SelectSingleUnit();
        }
    }

    void SelectSingleUnit()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            SelectUnitAtWorldPosition(hit.point);
        }
    }

    void SelectUnitsInRect(Rect selectionRect)
    {
        using (var query = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(GroupComponent), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var transforms = query.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.TempJob))
            using (var groups = query.ToComponentDataArray<GroupComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(transforms[i].Position);
                    screenPos.y = Screen.height - screenPos.y;

                    if (selectionRect.Contains(screenPos))
                    {
                        var group = groups[i];
                        group.IsSelected = true;
                        entityManager.SetComponentData(entities[i], group);
                        entityManager.AddComponent<SelectedTag>(entities[i]);
                    }
                }
            }
        }
    }

    void SelectUnitAtWorldPosition(float3 worldPos)
    {
        Entity closestUnit = Entity.Null;
        float closestDistance = float.MaxValue;

        using (var query = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var transforms = query.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    if (entityManager.HasComponent<DeadTag>(entities[i])) continue;
                    float distance = math.distance(worldPos, transforms[i].Position);
                    if (distance < 2f && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestUnit = entities[i];
                    }
                }
            }
        }

        if (closestUnit != Entity.Null)
        {
            var group = entityManager.GetComponentData<GroupComponent>(closestUnit);
            group.IsSelected = true;
            entityManager.SetComponentData(closestUnit, group);
            entityManager.AddComponent<SelectedTag>(closestUnit);
        }
    }

    void IssueCommand()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Entity targetUnit = GetUnitAtWorldPosition(hit.point);

            if (targetUnit != Entity.Null && entityManager.HasComponent<UnitTypeComponent>(targetUnit))
            {
                var targetUnitType = entityManager.GetComponentData<UnitTypeComponent>(targetUnit);
                if (targetUnitType.TeamId != 0)
                {
                    IssueAttackCommand(targetUnit);
                    return;
                }
            }

            IssueMoveCommand(hit.point);
        }
    }

    void IssueMoveCommand(float3 destination)
    {
        using (var query = entityManager.CreateEntityQuery(typeof(CommandComponent), typeof(SelectedTag), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var commands = query.ToComponentDataArray<CommandComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var command = commands[i];
                    command.Type = CommandType.Move;
                    command.TargetPosition = destination;
                    command.CommandTime = Time.time;

                    entityManager.SetComponentData(entities[i], command);
                    entityManager.SetComponentEnabled<CommandComponent>(entities[i], true);
                }
            }
        }
    }

    void IssueAttackCommand(Entity target)
    {
        using (var query = entityManager.CreateEntityQuery(typeof(CommandComponent), typeof(SelectedTag), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var commands = query.ToComponentDataArray<CommandComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var command = commands[i];
                    command.Type = CommandType.Attack;
                    command.TargetEntity = target;
                    command.CommandTime = Time.time;

                    entityManager.SetComponentData(entities[i], command);
                    entityManager.SetComponentEnabled<CommandComponent>(entities[i], true);
                }
            }
        }
    }

    Entity GetUnitAtWorldPosition(float3 worldPos)
    {
        using (var query = entityManager.CreateEntityQuery(typeof(LocalTransform)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var transforms = query.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    float distance = math.distance(worldPos, transforms[i].Position);
                    if (distance < 2f)
                    {
                        return entities[i];
                    }
                }
            }
        }
        return Entity.Null;
    }

    void ClearAllSelections()
    {
        using (var query = entityManager.CreateEntityQuery(typeof(GroupComponent), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var groups = query.ToComponentDataArray<GroupComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var group = groups[i];
                    group.IsSelected = false;
                    entityManager.SetComponentData(entities[i], group);
                    entityManager.RemoveComponent<SelectedTag>(entities[i]);
                }
            }
        }
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            IssueStopCommand();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SetAggressiveMode();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnTestUnit(UnitType.Infantry);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            SpawnTestUnit(UnitType.Cavalry);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnTestUnit(UnitType.Archer);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnTestUnit(UnitType.Dinosaur);
        }
    }

    void IssueStopCommand()
    {
        using (var query = entityManager.CreateEntityQuery(typeof(CommandComponent), typeof(SelectedTag), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var commands = query.ToComponentDataArray<CommandComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var command = commands[i];
                    command.Type = CommandType.Stop;
                    command.CommandTime = Time.time;

                    entityManager.SetComponentData(entities[i], command);
                    entityManager.SetComponentEnabled<CommandComponent>(entities[i], true);
                }
            }
        }
    }

    void SetAggressiveMode()
    {
        using (var query = entityManager.CreateEntityQuery(typeof(AttackRangeComponent), typeof(SelectedTag), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var attackRanges = query.ToComponentDataArray<AttackRangeComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Entity nearestEnemy = FindNearestEnemy(entityManager.GetComponentData<LocalTransform>(entities[i]).Position);
                    if (nearestEnemy != Entity.Null)
                    {
                        var attackRange = attackRanges[i];
                        attackRange.Target = nearestEnemy;
                        attackRange.HasTarget = true;
                        entityManager.SetComponentData(entities[i], attackRange);
                    }
                }
            }
        }
    }

    Entity FindNearestEnemy(float3 position)
    {
        Entity nearest = Entity.Null;
        float nearestDistance = float.MaxValue;

        using (var query = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(UnitTypeComponent)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var transforms = query.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.TempJob))
            using (var unitTypes = query.ToComponentDataArray<UnitTypeComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    if (unitTypes[i].TeamId == 0) continue; // Skip player units
                    if (entityManager.HasComponent<DeadTag>(entities[i])) continue;

                    float distance = math.distance(position, transforms[i].Position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = entities[i];
                    }
                }
            }
        }

        return nearest;
    }

    void SpawnTestUnit(UnitType unitType)
    {
        var gameManager = FindObjectOfType<RTSGameManager>();
        if (gameManager != null)
        {
            Vector3 spawnPos = gameManager.PlayerSpawnPoint.position + new Vector3(
                UnityEngine.Random.Range(-5f, 5f),
                0f,
                UnityEngine.Random.Range(-5f, 5f)
            );
            gameManager.SpawnPlayerUnit(unitType, spawnPos);
        }
    }

    void HandleGroupSelection()
    {
        for (int i = 0; i < GroupHotkeys.Length; i++)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(GroupHotkeys[i]))
            {
                SaveGroup(i + 1);
            }
            else if (Input.GetKeyDown(GroupHotkeys[i]))
            {
                SelectGroup(i + 1);
            }
        }
    }

    void SaveGroup(int groupNumber)
    {
        // Clear previous group assignments
        using (var query = entityManager.CreateEntityQuery(typeof(GroupComponent), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var groups = query.ToComponentDataArray<GroupComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var group = groups[i];
                    if (group.GroupId == groupNumber + 10)
                    {
                        group.GroupId = 0;
                        entityManager.SetComponentData(entities[i], group);
                    }
                }
            }
        }

        // Assign selected units to group
        using (var query = entityManager.CreateEntityQuery(typeof(GroupComponent), typeof(SelectedTag), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var groups = query.ToComponentDataArray<GroupComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var group = groups[i];
                    group.GroupId = groupNumber + 10;
                    entityManager.SetComponentData(entities[i], group);
                }
            }
        }
    }

    void SelectGroup(int groupNumber)
    {
        ClearAllSelections();

        using (var query = entityManager.CreateEntityQuery(typeof(GroupComponent), typeof(PlayerUnitTag)))
        {
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob))
            using (var groups = query.ToComponentDataArray<GroupComponent>(Unity.Collections.Allocator.TempJob))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    if (groups[i].GroupId == groupNumber + 10)
                    {
                        if (!entityManager.HasComponent<DeadTag>(entities[i]))
                        {
                            var group = groups[i];
                            group.IsSelected = true;
                            entityManager.SetComponentData(entities[i], group);
                            entityManager.AddComponent<SelectedTag>(entities[i]);
                        }
                    }
                }
            }
        }
    }

    Rect GetSelectionRect(Vector3 start, Vector3 end)
    {
        float minX = Mathf.Min(start.x, end.x);
        float minY = Mathf.Min(start.y, end.y);
        float width = Mathf.Abs(start.x - end.x);
        float height = Mathf.Abs(start.y - end.y);

        return new Rect(minX, Screen.height - minY - height, width, height);
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            Rect selectionRect = GetSelectionRect(selectionStartPos, Input.mousePosition);
            GUI.Box(selectionRect, "", GUI.skin.box);
        }
    }
}
