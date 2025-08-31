using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Settings")]
    public bool GamePaused = false;
    public float GameSpeed = 1f;

    [Header("Victory Conditions")]
    public bool CheckVictoryConditions = true;

    private EntityManager entityManager;
    private bool gameEnded = false;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        InvokeRepeating(nameof(CheckGameState), 1f, 1f);
    }

    void Update()
    {
        HandleGameControls();
    }

    void HandleGameControls()
    {
        // Pause/Resume
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }

        // Speed controls
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
        {
            IncreaseGameSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            DecreaseGameSpeed();
        }

        // Apply game speed
        Time.timeScale = GamePaused ? 0f : GameSpeed;
    }

    void TogglePause()
    {
        GamePaused = !GamePaused;
        Debug.Log($"Game {(GamePaused ? "Paused" : "Resumed")}");
    }

    void IncreaseGameSpeed()
    {
        GameSpeed = Mathf.Min(GameSpeed * 1.5f, 4f);
        Debug.Log($"Game Speed: {GameSpeed:F1}x");
    }

    void DecreaseGameSpeed()
    {
        GameSpeed = Mathf.Max(GameSpeed / 1.5f, 0.25f);
        Debug.Log($"Game Speed: {GameSpeed:F1}x");
    }

    void CheckGameState()
    {
        if (!CheckVictoryConditions || gameEnded) return;

        // Count living units per team
        int playerUnits = 0;
        int enemyUnits = 0;

        // Create query for living units with team information
        var unitQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<UnitTypeComponent>(),
            ComponentType.Exclude<DeadTag>()
        );

        using var entities = unitQuery.ToEntityArray(Allocator.TempJob);
        using var unitTypes = unitQuery.ToComponentDataArray<UnitTypeComponent>(Allocator.TempJob);

        for (int i = 0; i < unitTypes.Length; i++)
        {
            if (unitTypes[i].TeamId == 0)
                playerUnits++;
            else if (unitTypes[i].TeamId == 1)
                enemyUnits++;
        }

        // Check victory conditions
        if (playerUnits == 0)
        {
            OnGameEnd(false);
        }
        else if (enemyUnits == 0)
        {
            OnGameEnd(true);
        }
    }

    void OnGameEnd(bool playerWon)
    {
        gameEnded = true;
        GamePaused = true;

        string message = playerWon ? "Victory!" : "Defeat!";
        Debug.Log($"Game Over: {message}");

        // Show end game UI (implement based on your UI needs)
        ShowEndGameUI(playerWon);
    }

    void ShowEndGameUI(bool playerWon)
    {
        // Implementation depends on your UI setup
        // You could show a victory/defeat screen here
    }

    public void RestartGame()
    {
        gameEnded = false;
        GamePaused = false;
        GameSpeed = 1f;

        // Clear all entities
        using var allEntities = entityManager.GetAllEntities(Allocator.TempJob);
        entityManager.DestroyEntity(allEntities);

        // Restart game
        var gameManager = FindObjectOfType<RTSGameManager>();
        if (gameManager != null)
        {
            gameManager.enabled = false;
            gameManager.enabled = true;
        }
    }
}