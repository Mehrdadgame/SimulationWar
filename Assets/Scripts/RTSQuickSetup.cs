using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RTSQuickSetup : MonoBehaviour
{
    [Header("Quick Setup Configuration")]
    [Tooltip("Click to automatically setup the RTS scene")]
    public bool AutoSetupScene = false;

    [Header("Scene Settings")]
    public bool CreateGround = true;
    public bool CreateUI = true;
    public bool CreateManagers = true;
    public bool CreateSpawnPoints = true;
    public bool CreateCamera = true;
    public bool CreateLighting = true;

    [Header("Game Configuration")]
    public int InitialPlayerUnits = 10;
    public int InitialEnemyUnits = 15;
    public float SpawnRadius = 20f;
    public float MapSize = 100f;

    [Header("Unit Prefab Creation")]
    public bool CreateUnitPrefabs = true;
    public Material PlayerTeamMaterial;
    public Material EnemyTeamMaterial;

#if UNITY_EDITOR
    void Start()
    {
        if (AutoSetupScene)
        {
            SetupCompleteScene();
        }
    }

    [ContextMenu("Setup Complete RTS Scene")]
    public void SetupCompleteScene()
    {
        Debug.Log("🚀 Starting RTS Scene Quick Setup...");

        // Step 1: Create Ground
        if (CreateGround)
            SetupGround();

        // Step 2: Create Camera
        if (CreateCamera)
            SetupCamera();

        // Step 3: Create Lighting
        if (CreateLighting)
            SetupLighting();

        // Step 4: Create Spawn Points
        if (CreateSpawnPoints)
            SetupSpawnPoints();

        // Step 5: Create Unit Prefabs
        if (CreateUnitPrefabs)
            SetupUnitPrefabs();

        // Step 6: Create Game Managers
        if (CreateManagers)
            SetupGameManagers();

        // Step 7: Create UI System
        if (CreateUI)
            SetupUISystem();

        // Step 8: Final Setup
        FinalSetup();

        Debug.Log("✅ RTS Scene Setup Complete! Ready to play!");
        Debug.Log("🎮 Controls: Left Click = Select, Right Click = Move/Attack, Q/W/E/R = Spawn Units");
    }

    void SetupGround()
    {
        Debug.Log("🌍 Creating Ground...");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);

        // Set ground layer
        ground.layer = 0; // Default layer for ground detection

        // Add materials if available
        var renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.5f, 0.3f); // Green ground
            groundMat.name = "GroundMaterial";
            renderer.material = groundMat;
        }
    }

    void SetupCamera()
    {
        Debug.Log("📹 Setting up Camera...");

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCam = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }

        // Position camera for RTS view
        mainCam.transform.position = new Vector3(0, 15, -10);
        mainCam.transform.rotation = Quaternion.Euler(30, 0, 0);

        // Add camera controller
        RTSCameraController cameraController = mainCam.GetComponent<RTSCameraController>();
        if (cameraController == null)
        {
            cameraController = mainCam.gameObject.AddComponent<RTSCameraController>();
        }

        // Configure camera controller
        cameraController.MoveSpeed = 10f;
        cameraController.ZoomSpeed = 5f;
        cameraController.MinZoom = 5f;
        cameraController.MaxZoom = 30f;
        cameraController.RotationSpeed = 100f;
        cameraController.BoundarySize = 50f;
    }

    void SetupLighting()
    {
        Debug.Log("💡 Setting up Lighting...");

        // Create directional light if none exists
        Light[] lights = FindObjectsOfType<Light>();
        bool hasDirectionalLight = false;

        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                hasDirectionalLight = true;
                break;
            }
        }

        if (!hasDirectionalLight)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.intensity = 1f;
            dirLight.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }

    void SetupSpawnPoints()
    {
        Debug.Log("📍 Creating Spawn Points...");

        // Player spawn point
        GameObject playerSpawn = new GameObject("PlayerSpawnPoint");
        playerSpawn.transform.position = new Vector3(-20, 0, 0);

        // Add visual indicator
        GameObject playerIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerIndicator.name = "PlayerSpawnIndicator";
        playerIndicator.transform.SetParent(playerSpawn.transform);
        playerIndicator.transform.localPosition = Vector3.up;
        playerIndicator.transform.localScale = Vector3.one * 0.5f;

        var playerRenderer = playerIndicator.GetComponent<Renderer>();
        if (PlayerTeamMaterial != null)
            playerRenderer.material = PlayerTeamMaterial;
        else
            playerRenderer.material.color = Color.blue;

        // Enemy spawn point
        GameObject enemySpawn = new GameObject("EnemySpawnPoint");
        enemySpawn.transform.position = new Vector3(20, 0, 0);

        // Add visual indicator
        GameObject enemyIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemyIndicator.name = "EnemySpawnIndicator";
        enemyIndicator.transform.SetParent(enemySpawn.transform);
        enemyIndicator.transform.localPosition = Vector3.up;
        enemyIndicator.transform.localScale = Vector3.one * 0.5f;

        var enemyRenderer = enemyIndicator.GetComponent<Renderer>();
        if (EnemyTeamMaterial != null)
            enemyRenderer.material = EnemyTeamMaterial;
        else
            enemyRenderer.material.color = Color.red;
    }

    void SetupUnitPrefabs()
    {
        Debug.Log("🎭 Creating Unit Prefabs...");

        CreateUnitPrefab("Infantry", PrimitiveType.Capsule, new Vector3(1, 2, 1));
        CreateUnitPrefab("Cavalry", PrimitiveType.Cube, new Vector3(1.5f, 1, 2));
        CreateUnitPrefab("Archer", PrimitiveType.Cylinder, new Vector3(0.8f, 1.5f, 0.8f));
        CreateUnitPrefab("Dinosaur", PrimitiveType.Cube, new Vector3(2, 2.5f, 3));
    }

    void CreateUnitPrefab(string unitName, PrimitiveType primitive, Vector3 scale)
    {
        GameObject prefab = GameObject.CreatePrimitive(primitive);
        prefab.name = $"{unitName}Prefab";
        prefab.transform.localScale = scale;

        // Add unit authoring component
        UnitAuthoring unitAuthoring = prefab.AddComponent<UnitAuthoring>();

        // Configure based on unit type
        switch (unitName)
        {
            case "Infantry":
                unitAuthoring.UnitType = UnitType.Infantry;
                unitAuthoring.Health = 100f;
                unitAuthoring.Damage = 25f;
                unitAuthoring.MovementSpeed = 3f;
                unitAuthoring.AttackRange = 2f;
                break;
            case "Cavalry":
                unitAuthoring.UnitType = UnitType.Cavalry;
                unitAuthoring.Health = 150f;
                unitAuthoring.Damage = 35f;
                unitAuthoring.MovementSpeed = 6f;
                unitAuthoring.AttackRange = 2.5f;
                unitAuthoring.UsePathfinding = true;
                break;
            case "Archer":
                unitAuthoring.UnitType = UnitType.Archer;
                unitAuthoring.Health = 80f;
                unitAuthoring.Damage = 30f;
                unitAuthoring.MovementSpeed = 2.5f;
                unitAuthoring.AttackRange = 8f;
                break;
            case "Dinosaur":
                unitAuthoring.UnitType = UnitType.Dinosaur;
                unitAuthoring.Health = 300f;
                unitAuthoring.Damage = 50f;
                unitAuthoring.MovementSpeed = 4f;
                unitAuthoring.AttackRange = 3f;
                unitAuthoring.UsePathfinding = true;
                break;
        }

        // Convert to prefab
        string prefabPath = $"Assets/{unitName}Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

        DestroyImmediate(prefab);
    }

    void SetupGameManagers()
    {
        Debug.Log("⚙️ Setting up Game Managers...");

        // Create Game Managers parent
        GameObject gameManagers = new GameObject("GameManagers");

        // Add RTSGameManager
        GameObject gameManagerObj = new GameObject("RTSGameManager");
        gameManagerObj.transform.SetParent(gameManagers.transform);
        RTSGameManager gameManager = gameManagerObj.AddComponent<RTSGameManager>();

        // Load and assign prefabs
        gameManager.InfantryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InfantryPrefab.prefab");
        gameManager.CavalryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CavalryPrefab.prefab");
        gameManager.ArcherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArcherPrefab.prefab");
        gameManager.DinosaurPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DinosaurPrefab.prefab");

        // Configure settings
        gameManager.InitialPlayerUnits = InitialPlayerUnits;
        gameManager.InitialEnemyUnits = InitialEnemyUnits;
        gameManager.SpawnRadius = SpawnRadius;

        // Set spawn points
        gameManager.PlayerSpawnPoint = GameObject.Find("PlayerSpawnPoint").transform;
        gameManager.EnemySpawnPoint = GameObject.Find("EnemySpawnPoint").transform;

        // Add GameStateManager
        GameObject stateManagerObj = new GameObject("GameStateManager");
        stateManagerObj.transform.SetParent(gameManagers.transform);
        GameStateManager stateManager = stateManagerObj.AddComponent<GameStateManager>();
        stateManager.GamePaused = false;
        stateManager.GameSpeed = 1f;
        stateManager.CheckVictoryConditions = true;

        // Add RTSInputManager
        GameObject inputManagerObj = new GameObject("RTSInputManager");
        inputManagerObj.transform.SetParent(gameManagers.transform);
        RTSInputManager inputManager = inputManagerObj.AddComponent<RTSInputManager>();
        inputManager.GroundLayerMask = 1; // Default layer

        // Add UnitVisualManager
        GameObject visualManagerObj = new GameObject("UnitVisualManager");
        visualManagerObj.transform.SetParent(gameManagers.transform);
        UnitVisualManager visualManager = visualManagerObj.AddComponent<UnitVisualManager>();

        // Assign models (same as prefabs for now)
        visualManager.InfantryModel = gameManager.InfantryPrefab;
        visualManager.CavalryModel = gameManager.CavalryPrefab;
        visualManager.ArcherModel = gameManager.ArcherPrefab;
        visualManager.DinosaurModel = gameManager.DinosaurPrefab;

        // Create materials if not assigned
        if (PlayerTeamMaterial == null)
        {
            PlayerTeamMaterial = new Material(Shader.Find("Standard"));
            PlayerTeamMaterial.color = Color.blue;
            PlayerTeamMaterial.name = "PlayerTeamMaterial";
        }

        if (EnemyTeamMaterial == null)
        {
            EnemyTeamMaterial = new Material(Shader.Find("Standard"));
            EnemyTeamMaterial.color = Color.red;
            EnemyTeamMaterial.name = "EnemyTeamMaterial";
        }

        visualManager.PlayerTeamMaterial = PlayerTeamMaterial;
        visualManager.EnemyTeamMaterial = EnemyTeamMaterial;
    }

    void SetupUISystem()
    {
        Debug.Log("🖥️ Setting up UI System...");

        // Create Canvas
        GameObject canvasObj = new GameObject("UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem if none exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Add RTSUIManager
        RTSUIManager uiManager = canvasObj.AddComponent<RTSUIManager>();

        // Create UI Elements
        CreateUIPanel(canvas.transform);
        CreateMinimapSystem(canvas.transform);

        // Add VisualEffectsManager
        GameObject effectsManagerObj = new GameObject("VisualEffectsManager");
        effectsManagerObj.transform.SetParent(canvasObj.transform);
        VisualEffectsManager effectsManager = effectsManagerObj.AddComponent<VisualEffectsManager>();

        // Create basic effect prefabs
        CreateEffectPrefabs(effectsManager);
    }

    GameObject CreateHealthBarPrefab()
    {
        // Create Health Bar Prefab
        GameObject healthBarObj = new GameObject("HealthBarPrefab");
        RectTransform healthBarRect = healthBarObj.AddComponent<RectTransform>();
        healthBarRect.sizeDelta = new Vector2(60, 8);

        // Background
        Image healthBarBG = healthBarObj.AddComponent<Image>();
        healthBarBG.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Health Fill
        GameObject healthFillObj = new GameObject("HealthFill");
        healthFillObj.transform.SetParent(healthBarObj.transform);

        RectTransform healthFillRect = healthFillObj.AddComponent<RectTransform>();
        healthFillRect.anchorMin = Vector2.zero;
        healthFillRect.anchorMax = Vector2.one;
        healthFillRect.offsetMin = new Vector2(2, 2);
        healthFillRect.offsetMax = new Vector2(-2, -2);

        Image healthFillImage = healthFillObj.AddComponent<Image>();
        healthFillImage.color = Color.green;
        healthFillImage.type = Image.Type.Filled;
        healthFillImage.fillMethod = Image.FillMethod.Horizontal;

        // Save as prefab
        string prefabPath = "Assets/HealthBarPrefab.prefab";
        GameObject healthBarPrefab = PrefabUtility.SaveAsPrefabAsset(healthBarObj, prefabPath);

        DestroyImmediate(healthBarObj);
        return healthBarPrefab;
    }

    GameObject CreateSelectionIndicator()
    {
        // Create Selection Indicator
        GameObject selectionIndicatorObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        selectionIndicatorObj.name = "SelectionIndicator";
        selectionIndicatorObj.transform.localScale = new Vector3(3, 0.1f, 3);

        // Remove collider
        DestroyImmediate(selectionIndicatorObj.GetComponent<Collider>());

        // Create material
        Material selectionMat = new Material(Shader.Find("Standard"));
        selectionMat.color = new Color(0, 1, 0, 0.5f); // Semi-transparent green
        selectionMat.SetFloat("_Mode", 3); // Transparent mode
        selectionMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        selectionMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        selectionMat.SetInt("_ZWrite", 0);
        selectionMat.DisableKeyword("_ALPHATEST_ON");
        selectionMat.EnableKeyword("_ALPHABLEND_ON");
        selectionMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        selectionMat.renderQueue = 3000;
        selectionMat.name = "SelectionIndicatorMaterial";

        selectionIndicatorObj.GetComponent<Renderer>().sharedMaterial = selectionMat;

        // Save as prefab
        string prefabPath = "Assets/SelectionIndicatorPrefab.prefab";
        GameObject selectionPrefab = PrefabUtility.SaveAsPrefabAsset(selectionIndicatorObj, prefabPath);

        DestroyImmediate(selectionIndicatorObj);
        return selectionPrefab;
    }

    void CreateUIPanel(Transform canvasTransform)
    {
        // Info Panel
        GameObject infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(canvasTransform);

        RectTransform infoPanelRect = infoPanel.AddComponent<RectTransform>();
        infoPanelRect.anchorMin = new Vector2(0, 1);
        infoPanelRect.anchorMax = new Vector2(0, 1);
        infoPanelRect.anchoredPosition = new Vector2(10, -10);
        infoPanelRect.sizeDelta = new Vector2(200, 100);

        Image panelImage = infoPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);

        // Selected Units Text
        GameObject selectedText = new GameObject("SelectedUnitsText");
        selectedText.transform.SetParent(infoPanel.transform);

        RectTransform selectedTextRect = selectedText.AddComponent<RectTransform>();
        selectedTextRect.anchorMin = Vector2.zero;
        selectedTextRect.anchorMax = Vector2.one;
        selectedTextRect.offsetMin = new Vector2(5, 50);
        selectedTextRect.offsetMax = new Vector2(-5, -5);

        Text selectedTextComponent = selectedText.AddComponent<Text>();
        selectedTextComponent.text = "Selected Units: 0";
        selectedTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        selectedTextComponent.fontSize = 14;
        selectedTextComponent.color = Color.white;

        // Game Stats Text
        GameObject gameStatsText = new GameObject("GameStatsText");
        gameStatsText.transform.SetParent(infoPanel.transform);

        RectTransform gameStatsRect = gameStatsText.AddComponent<RectTransform>();
        gameStatsRect.anchorMin = Vector2.zero;
        gameStatsRect.anchorMax = Vector2.one;
        gameStatsRect.offsetMin = new Vector2(5, 5);
        gameStatsRect.offsetMax = new Vector2(-5, -50);

        Text gameStatsComponent = gameStatsText.AddComponent<Text>();
        gameStatsComponent.text = "Player Units: 0\nEnemy Units: 0";
        gameStatsComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameStatsComponent.fontSize = 12;
        gameStatsComponent.color = Color.white;

        // Create Health Bar Prefab
        GameObject healthBarPrefab = CreateHealthBarPrefab();

        // Create World Canvas for health bars
        GameObject worldCanvasObj = new GameObject("WorldCanvas");
        worldCanvasObj.transform.SetParent(canvasTransform);
        Canvas worldCanvas = worldCanvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.worldCamera = Camera.main;
        worldCanvasObj.AddComponent<CanvasScaler>();
        worldCanvasObj.AddComponent<GraphicRaycaster>();

        // Create Health Bar Parent
        GameObject healthBarParent = new GameObject("HealthBarParent");
        healthBarParent.transform.SetParent(canvasTransform);

        // Create Selection Indicator
        GameObject selectionIndicator = CreateSelectionIndicator();

        // Link to UI Manager
        RTSUIManager uiManager = canvasTransform.GetComponent<RTSUIManager>();
        uiManager.SelectedUnitsText = selectedTextComponent;
        uiManager.GameStatsText = gameStatsComponent;
        uiManager.HealthBarPrefab = healthBarPrefab.GetComponent<RectTransform>();
        uiManager.WorldCanvas = worldCanvas;
        uiManager.HealthBarParent = healthBarParent.transform;
        uiManager.SelectionIndicator = selectionIndicator;
    }

    void CreateMinimapSystem(Transform canvasTransform)
    {
        // Minimap Panel
        GameObject minimapPanel = new GameObject("MinimapPanel");
        minimapPanel.transform.SetParent(canvasTransform);

        RectTransform minimapRect = minimapPanel.AddComponent<RectTransform>();
        minimapRect.anchorMin = new Vector2(1, 1);
        minimapRect.anchorMax = new Vector2(1, 1);
        minimapRect.anchoredPosition = new Vector2(-110, -110);
        minimapRect.sizeDelta = new Vector2(200, 200);

        Image minimapImage = minimapPanel.AddComponent<Image>();
        minimapImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Create Minimap Camera
        GameObject minimapCameraObj = new GameObject("MinimapCamera");
        minimapCameraObj.transform.position = new Vector3(0, 50, 0);
        minimapCameraObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        Camera minimapCamera = minimapCameraObj.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = MapSize * 0.5f;
        minimapCamera.depth = -10;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0.1f, 0.3f, 0.1f);

        // Create Render Texture
        RenderTexture minimapTexture = new RenderTexture(512, 512, 16);
        minimapTexture.name = "MinimapTexture";
        minimapCamera.targetTexture = minimapTexture;

        // Add MinimapSystem
        MinimapSystem minimapSystem = minimapCameraObj.AddComponent<MinimapSystem>();
        minimapSystem.MinimapTexture = minimapTexture;
        minimapSystem.MinimapCamera = minimapCamera;
        minimapSystem.MinimapUI = minimapRect;
        minimapSystem.MapSize = MapSize;

        // Set minimap texture to UI
        minimapImage.material = new Material(Shader.Find("Unlit/Texture"));
        minimapImage.material.mainTexture = minimapTexture;
    }

    void CreateEffectPrefabs(VisualEffectsManager effectsManager)
    {
        // Hit Effect
        GameObject hitEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitEffect.name = "HitEffectPrefab";
        hitEffect.transform.localScale = Vector3.one * 0.5f;

        Material hitMaterial = new Material(Shader.Find("Standard"));
        hitMaterial.color = Color.yellow;
        hitMaterial.name = "HitEffectMaterial";
        hitEffect.GetComponent<Renderer>().sharedMaterial = hitMaterial;

        // Death Effect  
        GameObject deathEffect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deathEffect.name = "DeathEffectPrefab";
        deathEffect.transform.localScale = Vector3.one * 0.8f;

        Material deathMaterial = new Material(Shader.Find("Standard"));
        deathMaterial.color = Color.red;
        deathMaterial.name = "DeathEffectMaterial";
        deathEffect.GetComponent<Renderer>().sharedMaterial = deathMaterial;

        // Selection Ring
        GameObject selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        selectionRing.name = "SelectionRingPrefab";
        selectionRing.transform.localScale = new Vector3(2, 0.1f, 2);

        Material selectionMaterial = new Material(Shader.Find("Standard"));
        selectionMaterial.color = Color.green;
        selectionMaterial.name = "SelectionRingMaterial";
        selectionRing.GetComponent<Renderer>().sharedMaterial = selectionMaterial;
        DestroyImmediate(selectionRing.GetComponent<Collider>());

        // Projectile
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "ProjectilePrefab";
        projectile.transform.localScale = Vector3.one * 0.2f;

        Material projectileMaterial = new Material(Shader.Find("Standard"));
        projectileMaterial.color = Color.white;
        projectileMaterial.name = "ProjectileMaterial";
        projectile.GetComponent<Renderer>().sharedMaterial = projectileMaterial;

        // Convert to prefabs and assign
        effectsManager.HitEffectPrefab = CreatePrefabAsset(hitEffect);
        effectsManager.DeathEffectPrefab = CreatePrefabAsset(deathEffect);
        effectsManager.SelectionRingPrefab = CreatePrefabAsset(selectionRing);
        effectsManager.ProjectilePrefab = CreatePrefabAsset(projectile);

        DestroyImmediate(hitEffect);
        DestroyImmediate(deathEffect);
        DestroyImmediate(selectionRing);
        DestroyImmediate(projectile);
    }

    GameObject CreatePrefabAsset(GameObject original)
    {
        string path = $"Assets/{original.name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(original, path);
        return prefab;
    }

    void FinalSetup()
    {
        Debug.Log("🔧 Final Setup...");

        // Set up scene settings
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.3f, 0.2f);

        // Create instructions GameObject
        GameObject instructions = new GameObject("=== RTS INSTRUCTIONS ===");
        var instructionText = instructions.AddComponent<TextMesh>();
        instructionText.text = "Controls:\n" +
                              "Left Click: Select Units\n" +
                              "Right Click: Move/Attack\n" +
                              "Q/W/E/R: Spawn Units\n" +
                              "Space: Pause\n" +
                              "1-5: Group Hotkeys";
        instructionText.fontSize = 12;
        instructionText.color = Color.white;
        instructions.transform.position = new Vector3(0, 10, 0);
        instructions.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(),
            "Assets/RTSGameScene.unity"
        );

        // Final log
        Debug.Log("=== RTS SETUP COMPLETE ===");
        Debug.Log("📝 Scene saved as 'RTSGameScene.unity'");
        Debug.Log("🎮 Press Play to start the game!");
        Debug.Log("⌨️ Use Q/W/E/R to spawn different unit types");
        Debug.Log("🖱️ Left click to select, Right click to command");
    }

    // Public method for runtime setup
    [ContextMenu("Quick Runtime Setup")]
    public void RuntimeSetup()
    {
        if (Application.isPlaying)
        {
            SetupCompleteScene();
        }
        else
        {
            Debug.LogWarning("Runtime setup only works in Play mode!");
        }
    }
#endif
}