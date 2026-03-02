using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// Joe's combat-focused playtest bootstrapper. Grows each phase.
/// Drop on an empty GameObject, hit Play, and test FPS combat:
/// weapon, enemies, waves, inventory, HUD.
///
/// Controls:
///   WASD - Move, Mouse - Look, Space - Jump, Shift - Sprint
///   B - Toggle build/items hotbar page
///   1-9 - Select hotbar slot
///   Tab - Open/close inventory
///   Escape - Cancel / return to items page
///   G - Spawn enemy wave
///   Left click - Fire weapon
///
/// Everything is created at runtime -- no prefabs or assets required.
/// </summary>
public class DevTestPlaytestSetup : MonoBehaviour
{
    [Header("Arena")]
    [SerializeField] private float _arenaSize = 60f;

    [Header("Inventory")]
    [SerializeField] private int _worldItemCount = 5;

    // -- Definitions (created at runtime) --
    private WeaponDefinitionSO _weaponDef;
    private FaunaDefinitionSO _faunaDef;
    private GameEventSO _enemyDiedEvent;
    private ItemDefinitionSO _ironScrapDef;

    // -- Player / HUD --
    private PlayerHUD _playerHUD;
    private PlayerInventory _playerInventory;
    private WeaponBehaviour _weaponBehaviour;

    // -- Combat infrastructure --
    private GameObject _enemyTemplate;
    private WaveControllerBehaviour _waveController;
    private EnemySpawner _enemySpawner;

    // -- Environment --
    private PlaytestEnvironment _environment;
    private GameObject _groundPlane;

    private void Awake()
    {
        DestroySceneCameras();
        CreateDefinitions();
        CreateRegistries();
        CreateEnvironment();

        var player = CreatePlayer();

        // Match player camera to fog color
        var fpsCam = player.GetComponentInChildren<Camera>();
        if (fpsCam != null && RenderSettings.fog)
        {
            fpsCam.clearFlags = CameraClearFlags.SolidColor;
            fpsCam.backgroundColor = RenderSettings.fogColor;
        }

        WirePlayerCombat(player);
        CreateWorldItems();
        CreateEnemyTemplate();
        CreateSpawnPointsAndWaves();
        BakeNavMesh();
        CreateHUD(player);

        Debug.Log("dev test: setup complete");
        Debug.Log("controls: WASD=move, Mouse=look, Space=jump, Shift=sprint");
        Debug.Log("controls: B=toggle build/items, 1-9=select slot, Tab=inventory");
        Debug.Log("controls: G=spawn next wave, LMB=fire weapon, Esc=cancel");
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        HandleWaveTrigger(kb);
    }

    private void OnGUI()
    {
        float x = 10;
        float y = 10;
        float w = 380;
        float h = 22;

        GUI.Box(new Rect(x - 4, y - 4, w + 8, h * 8 + 8), "");

        DrawLine(ref y, x, w, h, "SLOPWORKS DEV TEST (Joe)", true);

        // Combat stats
        var wc = _waveController != null ? _waveController.Controller : null;
        string waveInfo = wc != null
            ? $"Wave: {wc.CurrentWave}/{wc.TotalWaves}  |  Enemies: {wc.EnemiesRemaining}"
            : "Wave: --";
        var healthBeh = _playerInventory != null ? _playerInventory.GetComponent<HealthBehaviour>() : null;
        var healthComp = healthBeh != null ? healthBeh.Health : null;
        var weapon = _weaponBehaviour != null ? _weaponBehaviour.Weapon : null;
        string healthInfo = healthComp != null ? $"HP: {healthComp.CurrentHealth:F0}/{healthComp.MaxHealth:F0}" : "HP: --";
        string ammoInfo = weapon != null ? $"Ammo: {weapon.CurrentAmmo}/{_weaponDef.magazineSize}" : "Ammo: --";
        DrawLine(ref y, x, w, h, $"{waveInfo}");
        DrawLine(ref y, x, w, h, $"{healthInfo}  |  {ammoInfo}");

        y += 4;
        DrawLine(ref y, x, w, h, "[WASD] Move  [Space] Jump  [Shift] Sprint");
        DrawLine(ref y, x, w, h, "[LMB] Fire  [Tab] Inventory  [B] Build/items");
        DrawLine(ref y, x, w, h, "[G] Spawn wave  [1-9] Select slot  [Esc] Cancel");
    }

    private void OnDestroy()
    {
        if (_playerHUD != null)
        {
            _playerHUD.OnBuildToolSelected -= OnHotbarBuildToolSelected;
            _playerHUD.OnPageChanged -= OnHotbarPageChanged;
        }

        if (_weaponDef != null) DestroyImmediate(_weaponDef);
        if (_faunaDef != null) DestroyImmediate(_faunaDef);
        if (_enemyDiedEvent != null) DestroyImmediate(_enemyDiedEvent);
        if (_ironScrapDef != null) DestroyImmediate(_ironScrapDef);
        if (_enemyTemplate != null) DestroyImmediate(_enemyTemplate);
    }

    // -- Setup --

    private void DestroySceneCameras()
    {
        foreach (var cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            Debug.Log($"dev test: destroying scene camera '{cam.name}'");
            Destroy(cam.gameObject);
        }
    }

    private void CreateDefinitions()
    {
        _weaponDef = ScriptableObject.CreateInstance<WeaponDefinitionSO>();
        _weaponDef.weaponId = "test_rifle";
        _weaponDef.damage = 25f;
        _weaponDef.fireRate = 2f;
        _weaponDef.range = 50f;
        _weaponDef.damageType = DamageType.Kinetic;
        _weaponDef.magazineSize = 12;
        _weaponDef.reloadTime = 1.5f;

        _faunaDef = ScriptableObject.CreateInstance<FaunaDefinitionSO>();
        _faunaDef.faunaId = "test_enemy";
        _faunaDef.maxHealth = 50f;
        _faunaDef.moveSpeed = 3.5f;
        _faunaDef.attackDamage = 10f;
        _faunaDef.attackRange = 2f;
        _faunaDef.attackCooldown = 1f;
        _faunaDef.sightRange = 20f;
        _faunaDef.attackDamageType = DamageType.Kinetic;

        _enemyDiedEvent = ScriptableObject.CreateInstance<GameEventSO>();
        _enemyDiedEvent.name = "EnemyDied_DevTest";

        _ironScrapDef = ScriptableObject.CreateInstance<ItemDefinitionSO>();
        _ironScrapDef.itemId = "iron_scrap";
        _ironScrapDef.displayName = "Iron Scrap";
        _ironScrapDef.category = ItemCategory.RawMaterial;
        _ironScrapDef.isStackable = true;
        _ironScrapDef.maxStackSize = 64;

        Debug.Log("dev test: definitions created");
    }

    private void CreateRegistries()
    {
        var registryObj = new GameObject("Registries");
        registryObj.SetActive(false);

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        var itemRegistry = registryObj.AddComponent<ItemRegistry>();
        typeof(ItemRegistry).GetField("_items", flags)?.SetValue(itemRegistry, new[] { _ironScrapDef });

        registryObj.SetActive(true);
        Debug.Log("dev test: registries created");
    }

    private void CreateEnvironment()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        var envObj = new GameObject("PlaytestEnvironment");
        _environment = envObj.AddComponent<PlaytestEnvironment>();
        typeof(PlaytestEnvironment).GetField("_centerOffset", flags)?.SetValue(_environment, Vector3.zero);
        typeof(PlaytestEnvironment).GetField("_arenaSize", flags)?.SetValue(_environment, _arenaSize);
        _groundPlane = _environment.Generate();
    }

    private GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.layer = PhysicsLayers.Player;
        player.transform.position = new Vector3(0f, 1.5f, -5f);

        var capsule = player.AddComponent<CapsuleCollider>();
        capsule.radius = 0.3f;
        capsule.height = 1.8f;
        capsule.center = new Vector3(0, 0.9f, 0);

        var rb = player.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // FPS camera
        var camObj = new GameObject("PlayerCamera");
        camObj.tag = "MainCamera";
        camObj.transform.SetParent(player.transform, false);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();

        // Components (PlayerInventory before PlayerController so Awake finds it)
        _playerInventory = player.AddComponent<PlayerInventory>();
        player.AddComponent<PlayerController>();
        player.AddComponent<HealthBehaviour>();

        // Pickup trigger (child)
        var pickupObj = new GameObject("PickupTrigger");
        pickupObj.transform.SetParent(player.transform, false);
        pickupObj.layer = PhysicsLayers.Player;
        pickupObj.AddComponent<ItemPickupTrigger>();

        StartCoroutine(PreloadInventory(_playerInventory));

        Debug.Log("dev test: player created at (0, 1.5, -5)");
        return player;
    }

    private IEnumerator PreloadInventory(PlayerInventory inventory)
    {
        yield return null;
        inventory.TryAdd(ItemInstance.Create("iron_scrap"), 10);
        Debug.Log("dev test: preloaded iron scrap into inventory");
    }

    private void WirePlayerCombat(GameObject player)
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var fpsCam = player.GetComponentInChildren<Camera>();

        // Camera effects on FPS camera object (before WeaponBehaviour.Start finds them)
        var camObj = fpsCam.gameObject;
        if (camObj.GetComponent<CameraRecoil>() == null)
            camObj.AddComponent<CameraRecoil>();
        if (camObj.GetComponent<CameraShake>() == null)
            camObj.AddComponent<CameraShake>();

        // Muzzle flash as child of camera
        var muzzleObj = new GameObject("MuzzleFlashPoint");
        muzzleObj.transform.SetParent(camObj.transform);
        muzzleObj.transform.localPosition = new Vector3(0f, -0.1f, 0.5f);
        muzzleObj.AddComponent<MuzzleFlash>();

        // WeaponBehaviour -- must set _weaponDefinition before Awake creates WeaponController
        player.SetActive(false);
        _weaponBehaviour = player.AddComponent<WeaponBehaviour>();
        typeof(WeaponBehaviour).GetField("_weaponDefinition", flags)?.SetValue(_weaponBehaviour, _weaponDef);
        typeof(WeaponBehaviour).GetField("_camera", flags)?.SetValue(_weaponBehaviour, fpsCam);
        player.SetActive(true);

        // HealthBehaviour max health via reflection
        var health = player.GetComponent<HealthBehaviour>();
        typeof(HealthBehaviour).GetField("_maxHealth", flags)?.SetValue(health, 100f);

        Debug.Log("dev test: player combat wired (weapon, recoil, shake, muzzle flash)");
    }

    private void CreateWorldItems()
    {
        float half = _arenaSize * 0.5f;

        for (int i = 0; i < _worldItemCount; i++)
        {
            float x = Random.Range(-half + 3f, half - 3f);
            float z = Random.Range(-half + 3f, half - 3f);
            var pos = new Vector3(x, 0.3f, z);

            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = $"WorldItem_IronScrap_{i}";
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            var renderer = obj.GetComponent<Renderer>();
            renderer.material.color = new Color(0.6f, 0.4f, 0.2f);

            DestroyImmediate(obj.GetComponent<BoxCollider>());

            var worldItem = obj.AddComponent<WorldItem>();
            worldItem.Initialize(_ironScrapDef, Random.Range(1, 4));
        }
        Debug.Log($"dev test: {_worldItemCount} world items created");
    }

    private void CreateEnemyTemplate()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        _enemyTemplate = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        _enemyTemplate.name = "EnemyTemplate";
        _enemyTemplate.layer = PhysicsLayers.Fauna;
        SetColor(_enemyTemplate, new Color(0.8f, 0.2f, 0.2f));

        // Deactivate before adding components to prevent Awake/Start from running
        _enemyTemplate.SetActive(false);

        var rb = _enemyTemplate.AddComponent<Rigidbody>();
        rb.freezeRotation = true;

        var agent = _enemyTemplate.AddComponent<NavMeshAgent>();
        agent.speed = _faunaDef.moveSpeed;
        agent.stoppingDistance = _faunaDef.attackRange * 0.8f;

        var health = _enemyTemplate.AddComponent<HealthBehaviour>();
        typeof(HealthBehaviour).GetField("_maxHealth", flags)?.SetValue(health, _faunaDef.maxHealth);

        var controller = _enemyTemplate.AddComponent<FaunaController>();
        typeof(FaunaController).GetField("_def", flags)?.SetValue(controller, _faunaDef);
        typeof(FaunaController).GetField("_onDeathEvent", flags)?.SetValue(controller, _enemyDiedEvent);

        _enemyTemplate.AddComponent<EnemyHitFlash>();
        _enemyTemplate.AddComponent<EnemyKnockback>();

        // Keep deactivated -- EnemySpawner will Instantiate clones
        Debug.Log("dev test: enemy template created (inactive)");
    }

    private void CreateSpawnPointsAndWaves()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        float spawnDist = _arenaSize * 0.35f;

        var spawnParent = new GameObject("SpawnPoints");
        Vector3[] positions =
        {
            new Vector3(spawnDist, 0, spawnDist),
            new Vector3(-spawnDist, 0, spawnDist),
            new Vector3(spawnDist, 0, -spawnDist),
            new Vector3(-spawnDist, 0, -spawnDist),
        };

        var spawnTransforms = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            var point = new GameObject($"SpawnPoint_{i}");
            point.transform.SetParent(spawnParent.transform);
            point.transform.position = positions[i];
            point.transform.LookAt(Vector3.zero);
            spawnTransforms[i] = point.transform;
        }

        // Wave controller -- inactive so we can set fields before Awake
        var waveObj = new GameObject("WaveController");
        waveObj.SetActive(false);

        _enemySpawner = waveObj.AddComponent<EnemySpawner>();
        typeof(EnemySpawner).GetField("_enemyPrefab", flags)?.SetValue(_enemySpawner, _enemyTemplate);
        typeof(EnemySpawner).GetField("_spawnPoints", flags)?.SetValue(_enemySpawner, spawnTransforms);

        var waves = new List<WaveDefinition>
        {
            new WaveDefinition { enemyCount = 3, spawnDelay = 1f, timeBetweenWaves = 5f },
            new WaveDefinition { enemyCount = 5, spawnDelay = 0.8f, timeBetweenWaves = 5f },
            new WaveDefinition { enemyCount = 8, spawnDelay = 0.5f, timeBetweenWaves = 0f },
        };
        _waveController = waveObj.AddComponent<WaveControllerBehaviour>();
        typeof(WaveControllerBehaviour).GetField("_waves", flags)?.SetValue(_waveController, waves);
        typeof(WaveControllerBehaviour).GetField("_spawner", flags)?.SetValue(_waveController, _enemySpawner);
        typeof(WaveControllerBehaviour).GetField("_enemyDiedEvent", flags)?.SetValue(_waveController, _enemyDiedEvent);
        typeof(WaveControllerBehaviour).GetField("_autoStartDelay", flags)?.SetValue(_waveController, -1f);

        waveObj.SetActive(true);

        Debug.Log("dev test: wave system created (3 waves: 3, 5, 8 enemies, press G to start)");
    }

    private void BakeNavMesh()
    {
#if UNITY_EDITOR
        if (_groundPlane != null)
            _groundPlane.isStatic = true;
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("dev test: navmesh baked");
#else
        Debug.LogWarning("dev test: navmesh baking not available outside editor");
#endif
    }

    private void CreateHUD(GameObject player)
    {
        var canvasObj = new GameObject("HUDCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        canvasObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        _playerHUD = canvasObj.AddComponent<PlayerHUD>();
        canvasObj.AddComponent<RecipeSelectionUI>();
        canvasObj.AddComponent<StorageUI>();
        var inventoryUI = canvasObj.AddComponent<InventoryUI>();
        var hitMarker = canvasObj.AddComponent<HitMarkerUI>();

        // Wire hit marker to weapon
        if (_weaponBehaviour != null)
            _weaponBehaviour.SetHitMarker(hitMarker);

        StartCoroutine(WireHUD(_playerHUD, inventoryUI, player));
    }

    private IEnumerator WireHUD(PlayerHUD hud, InventoryUI inventoryUI, GameObject player)
    {
        yield return null;

        var health = player.GetComponent<HealthBehaviour>();
        var inventory = player.GetComponent<PlayerInventory>();
        var weapon = player.GetComponent<WeaponBehaviour>();
        var cam = player.GetComponentInChildren<Camera>();

        var cameraShake = cam != null ? cam.GetComponent<CameraShake>() : null;
        var wc = _waveController != null ? _waveController.Controller : null;

        hud.Initialize(
            health != null ? health.Health : null,
            weapon != null ? weapon.Weapon : null,
            cameraShake, wc);
        hud.InitializeInventory(inventory, cam);
        inventoryUI.Initialize(inventory);

        // Set up hotbar pages (items only for now, build page empty)
        var itemsPage = new HotbarPage("Items", PlayerInventory.HotbarSlots);
        var buildPage = new HotbarPage("Build", 0);
        hud.SetPages(new[] { itemsPage, buildPage });

        hud.OnBuildToolSelected += OnHotbarBuildToolSelected;
        hud.OnPageChanged += OnHotbarPageChanged;

        Debug.Log("dev test: HUD wired to player (combat + inventory)");
    }

    // -- Input --

    private void HandleWaveTrigger(Keyboard kb)
    {
        if (kb.gKey.wasPressedThisFrame && _waveController != null)
        {
            _waveController.BeginNextWave();
            Debug.Log("dev test: wave triggered via G key");
        }
    }

    // -- Hotbar callbacks (stubs for build page, no tools in Dev_Test yet) --

    private void OnHotbarBuildToolSelected(int pageIndex, int slotIndex, string toolId)
    {
        Debug.Log($"dev test: build tool {toolId} selected on page {pageIndex} slot {slotIndex} (no build tools wired)");
    }

    private void OnHotbarPageChanged(int pageIndex)
    {
        Debug.Log($"dev test: hotbar page changed to {pageIndex}");
    }

    // -- Helpers --

    private static void SetColor(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = color;
    }

    private static GUIStyle _boldStyle;

    private static void DrawLine(ref float y, float x, float w, float h, string text, bool bold = false)
    {
        if (bold)
        {
            if (_boldStyle == null)
            {
                _boldStyle = new GUIStyle(GUI.skin.label);
                _boldStyle.fontStyle = FontStyle.Bold;
            }
            GUI.Label(new Rect(x, y, w, h), text, _boldStyle);
        }
        else
        {
            GUI.Label(new Rect(x, y, w, h), text, GUI.skin.label);
        }
        y += h;
    }
}
