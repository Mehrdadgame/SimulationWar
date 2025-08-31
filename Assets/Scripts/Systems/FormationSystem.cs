using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct FormationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Handle formation movement for selected groups
        var selectedUnits = new NativeList<Entity>(Allocator.Temp);
        var selectedPositions = new NativeList<float3>(Allocator.Temp);

        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithEntityAccess()
                .WithAll<SelectedTag, PlayerUnitTag>()
                .WithNone<DeadTag>())
        {
            selectedUnits.Add(entity);
            selectedPositions.Add(transform.ValueRO.Position);
        }

        if (selectedUnits.Length > 1)
        {
            // Calculate formation positions
            float3 centerPosition = CalculateCenter(selectedPositions);

            for (int i = 0; i < selectedUnits.Length; i++)
            {
                if (SystemAPI.HasComponent<CommandComponent>(selectedUnits[i]))
                {
                    var command = SystemAPI.GetComponent<CommandComponent>(selectedUnits[i]);
                    if (command.Type == CommandType.Move)
                    {
                        // Offset position based on formation
                        float3 formationOffset = GetFormationOffset(i, selectedUnits.Length);
                        float3 newDestination = command.TargetPosition + formationOffset;

                        SystemAPI.SetComponent(selectedUnits[i], new MovementComponent
                        {
                            Speed = SystemAPI.GetComponent<MovementComponent>(selectedUnits[i]).Speed,
                            Destination = newDestination,
                            HasDestination = true,
                            StoppingDistance = 1.5f
                        });
                    }
                }
            }
        }

        selectedUnits.Dispose();
        selectedPositions.Dispose();
    }

    private float3 CalculateCenter(NativeList<float3> positions)
    {
        float3 sum = float3.zero;
        for (int i = 0; i < positions.Length; i++)
        {
            sum += positions[i];
        }
        return sum / positions.Length;
    }

    private float3 GetFormationOffset(int index, int totalUnits)
    {
        int unitsPerRow = math.max(1, (int)math.sqrt(totalUnits));
        int row = index / unitsPerRow;
        int col = index % unitsPerRow;

        float spacing = 2f;
        return new float3(
            (col - unitsPerRow * 0.5f) * spacing,
            0,
            row * spacing
        );
    }
}
