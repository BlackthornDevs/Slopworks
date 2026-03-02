using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// Joe's exclusive playtest bootstrapper. Skeleton for Joe to fill in turret and
/// PlaytestEnvironment code from his branch. The only component on the scene GameObject.
///
/// Joe: port your turret code here. See docs/coordination/tasks-joe.md J-023 for details.
///
/// Controls (shared):
///   WASD - Move, Mouse - Look, Space - Jump, Shift - Sprint
///   B - Toggle build/items hotbar page
///   1-8 - Select hotbar slot (items or build tool depending on page)
///   Tab - Open/close inventory
///   E - Interact with machines
///   R - Rotate (wall/ramp/machine placement)
///   Escape - Cancel / return to items page
///   PageUp/PageDown - Change active level
///   F - Fill storage with iron scrap
///   G - Spawn next wave
///   Left click - Fire weapon / place building
///
/// Joe-specific controls (add as you wire them):
///   P - Pre-seed factory with turret chain
///   8 - Turret placement tool (on build page)
/// </summary>
public class JoePlaytestSetup : MonoBehaviour
{
    [Header("Pre-seed")]
    [SerializeField] private bool _preSeedFactory;

    [Header("Automation")]
    [SerializeField] private ushort _beltSpeed = 4;

    // Shared context
    private PlaytestContext _ctx;
    private PlaytestToolController _toolCtrl;

    // -- Ground plane --
    private GameObject _groundPlane;

    // -- Combat --
    private WaveControllerBehaviour _waveController;
    private EnemySpawner _enemySpawner;

    // Joe adds: turret fields here
    // private TurretDefinitionSO _turretDef;

    private void Awake()
    {
        // 1. Shared bootstrap
        _ctx = new PlaytestBootstrap(this, _beltSpeed).Setup();

        // 2. Ground plane (Joe replaces with PlaytestEnvironment later)
        CreateGroundPlane();

        // 3. Shared tool controller
        _toolCtrl = gameObject.AddComponent<PlaytestToolController>();
        _toolCtrl.Initialize(_ctx, CreateBuildPage(), _groundPlane);

        // Joe adds: turret definition and RegisterToolHandler here
        // CreateTurretDefinition();
        // _toolCtrl.RegisterToolHandler(PlaytestToolController.ToolMode.TurretPlace, HandleTurretPlaceInput);

        // 4. Waves
        CreateSpawnPointsAndWaves();
        _toolCtrl.SetWaveController(_waveController);

        // 5. NavMesh
        BakeNavMesh();

        // 6. Pre-seed (optional)
        if (_preSeedFactory)
        {
            _toolCtrl.PreSeedFactory();
            // Joe adds: turret chain on top of pre-seed
        }

        // 7. Joe-specific HUD wiring
        StartCoroutine(WireJoeHUD());

        Debug.Log("playtest: setup complete (Joe)");
        Debug.Log("controls: WASD=move, Mouse=look, Space=jump, Shift=sprint");
        Debug.Log("controls: B=toggle build/items, 1-8=select slot, Tab=inventory, E=interact");
        Debug.Log("controls: R=rotate, Esc=cancel, PgUp/PgDn=level, F=fill storage");
        Debug.Log("controls: G=spawn next wave, LMB=fire weapon (items page)");
    }

    // -- Ground plane --

    private void CreateGroundPlane()
    {
        // Basic ground plane -- Joe replaces with PlaytestEnvironment
        _groundPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _groundPlane.name = "GridPlane";
        _groundPlane.layer = PhysicsLayers.GridPlane;
        _groundPlane.transform.position = new Vector3(
            FactoryGrid.Width * FactoryGrid.CellSize * 0.5f,
            -0.05f,
            FactoryGrid.Height * FactoryGrid.CellSize * 0.5f);
        _groundPlane.transform.localScale = new Vector3(
            FactoryGrid.Width * FactoryGrid.CellSize,
            0.1f,
            FactoryGrid.Height * FactoryGrid.CellSize);

        var renderer = _groundPlane.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
    }

    // -- Build page --

    private HotbarPage CreateBuildPage()
    {
        var page = new HotbarPage("Build", PlayerInventory.HotbarSlots);

        void Set(int slot, string id, string name, Color color)
        {
            page.Entries[slot] = new HotbarEntry { Id = id, DisplayName = name, Color = color };
        }

        Set(0, "foundation", "Found", new Color(0.4f, 0.4f, 0.4f, 0.8f));
        Set(1, "wall", "Wall", new Color(0.5f, 0.5f, 0.5f, 0.8f));
        Set(2, "ramp", "Ramp", new Color(0.5f, 0.6f, 0.5f, 0.8f));
        Set(3, "belt", "Belt", new Color(0.3f, 0.5f, 0.7f, 0.8f));
        Set(4, "machine", "Machine", new Color(0.7f, 0.5f, 0.2f, 0.8f));
        Set(5, "storage", "Storage", new Color(0.6f, 0.4f, 0.3f, 0.8f));
        Set(6, "delete", "Delete", new Color(0.8f, 0.2f, 0.2f, 0.8f));
        // Joe adds: slot 7 for turret
        // Set(7, "turret", "Turret", new Color(0.9f, 0.3f, 0.3f, 0.8f));

        return page;
    }

    // -- Combat --

    private void CreateSpawnPointsAndWaves()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        float centerX = 10f * FactoryGrid.CellSize;
        float centerZ = 10f * FactoryGrid.CellSize;

        var spawnParent = new GameObject("SpawnPoints");
        Vector3[] positions =
        {
            new Vector3(centerX + 20, 0, centerZ + 20),
            new Vector3(Mathf.Max(1f, centerX - 20), 0, centerZ + 20),
            new Vector3(centerX + 20, 0, Mathf.Max(1f, centerZ - 20)),
            new Vector3(Mathf.Max(1f, centerX - 20), 0, Mathf.Max(1f, centerZ - 20)),
        };

        var spawnTransforms = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            var point = new GameObject($"SpawnPoint_{i}");
            point.transform.SetParent(spawnParent.transform);
            point.transform.position = positions[i];
            point.transform.LookAt(new Vector3(centerX, 0, centerZ));
            spawnTransforms[i] = point.transform;
        }

        var waveObj = new GameObject("WaveController");
        waveObj.SetActive(false);

        _enemySpawner = waveObj.AddComponent<EnemySpawner>();
        typeof(EnemySpawner).GetField("_enemyPrefab", flags)?.SetValue(_enemySpawner, _ctx.EnemyTemplate);
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
        typeof(WaveControllerBehaviour).GetField("_enemyDiedEvent", flags)?.SetValue(_waveController, _ctx.EnemyDiedEvent);
        typeof(WaveControllerBehaviour).GetField("_autoStartDelay", flags)?.SetValue(_waveController, -1f);

        waveObj.SetActive(true);

        Debug.Log("playtest: wave system created (3 waves: 3, 5, 8 enemies, press G to start)");
    }

    private void BakeNavMesh()
    {
#if UNITY_EDITOR
        _groundPlane.isStatic = true;
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("playtest: navmesh baked");
#else
        Debug.LogWarning("playtest: navmesh baking not available outside editor");
#endif
    }

    // -- HUD wiring (Joe-specific) --

    private IEnumerator WireJoeHUD()
    {
        yield return null;
        yield return null;

        // Joe adds: turret-specific HUD wiring here

        Debug.Log("playtest: Joe HUD extensions wired");
    }

    // -- Unity callbacks --

    // Joe adds: Update for P key (pre-seed turret chain), turret placement input
    // Joe adds: OnGUI for turret stats

    private void OnDestroy()
    {
        // Joe adds: turret SO cleanup here

        // Destroy shared SOs from bootstrap
        if (_ctx != null && _ctx.RuntimeSOs != null)
        {
            foreach (var so in _ctx.RuntimeSOs)
            {
                if (so != null) DestroyImmediate(so);
            }
        }

        if (_ctx != null && _ctx.EnemyTemplate != null)
            DestroyImmediate(_ctx.EnemyTemplate);
    }
}
