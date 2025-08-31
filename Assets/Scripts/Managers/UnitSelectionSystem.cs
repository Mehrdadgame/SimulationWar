using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class UnitSelectionSystem : SystemBase
{
    private Camera mainCamera;

    protected override void OnCreate()
    {
        mainCamera = Camera.main;
        RequireForUpdate<PlayerUnitTag>();
    }

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleUnitSelection();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    void HandleUnitSelection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Clear previous selections if not holding shift
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                ClearAllSelections();
            }

            // Try to select unit
            SelectUnitAtPosition(hit.point);
        }
    }

    void HandleRightClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if clicking on enemy unit
            Entity targetUnit = GetUnitAtPosition(hit.point);

            if (targetUnit != Entity.Null && HasComponent<UnitTypeComponent>(targetUnit))
            {
                var targetUnitType = GetComponent<UnitTypeComponent>(targetUnit);
                if (targetUnitType.TeamId != 0) // Enemy team
                {
                    IssueAttackCommand(targetUnit);
                    return;
                }
            }

            // Move command
            IssueMoveCommand(hit.point);
        }
    }

    void ClearAllSelections()
    {
        Entities
            .WithAll<PlayerUnitTag, SelectedTag>()
            .ForEach((Entity entity, ref GroupComponent group) =>
            {
                group.IsSelected = false;
                EntityManager.RemoveComponent<SelectedTag>(entity);
            }).WithStructuralChanges().Run();
    }

    void SelectUnitAtPosition(float3 position)
    {
        Entity closestUnit = Entity.Null;
        float closestDistance = float.MaxValue;

        var unitList = new System.Collections.Generic.List<(Entity entity, float distance)>();

        // Modern DOTS approach using SystemAPI.Query
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
            .WithAll<PlayerUnitTag>()
            .WithNone<DeadTag>()
            .WithEntityAccess())
        {
            float distance = math.distance(position, transform.ValueRO.Position);
            if (distance < 2f)
            {
                unitList.Add((entity, distance));
            }
        }

        foreach (var (entity, distance) in unitList)
        {
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestUnit = entity;
            }
        }

        if (closestUnit != Entity.Null)
        {
            var group = SystemAPI.GetComponent<GroupComponent>(closestUnit);
            group.IsSelected = true;
            SystemAPI.SetComponent(closestUnit, group);
            EntityManager.AddComponent<SelectedTag>(closestUnit);
        }
    }

    Entity GetUnitAtPosition(float3 position)
    {
        Entity foundUnit = Entity.Null;

        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
            .WithNone<DeadTag>()
            .WithEntityAccess())
        {
            float distance = math.distance(position, transform.ValueRO.Position);
            if (distance < 2f)
            {
                foundUnit = entity;
                break; // Exit early once we find a unit
            }
        }

        return foundUnit;
    }

    void IssueMoveCommand(float3 destination)
    {
        foreach (var (command, entity) in SystemAPI.Query<RefRW<CommandComponent>>()
            .WithAll<SelectedTag, PlayerUnitTag>()
            .WithEntityAccess())
        {
            command.ValueRW.Type = CommandType.Move;
            command.ValueRW.TargetPosition = destination;
            command.ValueRW.CommandTime = (float)SystemAPI.Time.ElapsedTime;

            EntityManager.SetComponentEnabled<CommandComponent>(entity, true);
        }
    }

    void IssueAttackCommand(Entity target)
    {
        foreach (var (command, entity) in SystemAPI.Query<RefRW<CommandComponent>>()
            .WithAll<SelectedTag, PlayerUnitTag>()
            .WithEntityAccess())
        {
            command.ValueRW.Type = CommandType.Attack;
            command.ValueRW.TargetEntity = target;
            command.ValueRW.CommandTime = (float)SystemAPI.Time.ElapsedTime;

            EntityManager.SetComponentEnabled<CommandComponent>(entity, true);
        }
    }
}