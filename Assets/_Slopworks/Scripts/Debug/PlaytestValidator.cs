using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automated playtest validation. Runs checks after bootstrap completes and logs
/// structured [VALIDATE] messages that can be read via MCP console logs.
///
/// Add to a playtest scene alongside KevinPlaytestSetup or JoePlaytestSetup.
/// Waits a configurable number of frames for coroutines to finish, then validates.
/// </summary>
public class PlaytestValidator : MonoBehaviour
{
    [SerializeField] private bool _runOnPlay;
    [SerializeField] private int _delayFrames = 5;
    [SerializeField] private bool _autoQuit;

    private int _pass;
    private int _fail;

    private const string RunKey = "PlaytestValidator_RunOnPlay";

    private IEnumerator Start()
    {
#if UNITY_EDITOR
        if (!_runOnPlay && !UnityEditor.EditorPrefs.GetBool(RunKey, false))
            yield break;
        UnityEditor.EditorPrefs.DeleteKey(RunKey);
#else
        if (!_runOnPlay)
            yield break;
#endif

        for (int i = 0; i < _delayFrames; i++)
            yield return null;

        Debug.Log("[VALIDATE] === playtest validation starting ===");

        _pass = 0;
        _fail = 0;

        ValidatePlayer();
        ValidateGrid();
        ValidateHUD();
        ValidateWorldItems();
        ValidateCombat();
        ValidateBuildings();
        ValidateSupplyChain();
        ValidateNavMesh();

        Debug.Log($"[VALIDATE] === results: {_pass} passed, {_fail} failed ===");

        if (_autoQuit)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void Check(string name, bool condition, string detail = null)
    {
        if (condition)
        {
            _pass++;
            Debug.Log($"[VALIDATE] {name}: PASS");
        }
        else
        {
            _fail++;
            Debug.LogError($"[VALIDATE] {name}: FAIL" + (detail != null ? $" -- {detail}" : ""));
        }
    }

    // -- Player --

    private void ValidatePlayer()
    {
        var player = GameObject.Find("Player");
        Check("player_exists", player != null);
        if (player == null) return;

        Check("player_has_controller", player.GetComponent<PlayerController>() != null);
        Check("player_has_inventory", player.GetComponent<PlayerInventory>() != null);
        Check("player_has_weapon", player.GetComponent<WeaponBehaviour>() != null);
        Check("player_has_health", player.GetComponent<HealthBehaviour>() != null);

        var cam = Camera.main;
        Check("camera_exists", cam != null);
        if (cam != null)
            Check("camera_tagged_main", cam.CompareTag("MainCamera"));

        var rb = player.GetComponent<Rigidbody>();
        Check("player_has_rigidbody", rb != null);
        Check("player_near_ground", player.transform.position.y < 5f,
            $"y={player.transform.position.y}");

        var inventory = player.GetComponent<PlayerInventory>();
        Check("player_inventory_has_slots", inventory != null && PlayerInventory.TotalSlots > 0,
            $"slots={PlayerInventory.TotalSlots}");
    }

    // -- Grid --

    private void ValidateGrid()
    {
        var gridPlane = GameObject.Find("GridPlane");
        Check("grid_plane_exists", gridPlane != null);
        if (gridPlane != null)
            Check("grid_plane_correct_layer", gridPlane.layer == PhysicsLayers.GridPlane,
                $"layer={gridPlane.layer}");
    }

    // -- HUD --

    private void ValidateHUD()
    {
        var hudObj = GameObject.Find("HUDCanvas");
        Check("hud_canvas_exists", hudObj != null);

        var hud = hudObj != null ? hudObj.GetComponent<PlayerHUD>() : null;
        Check("hud_component_exists", hud != null);

        // Canvas
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Check("canvas_exists", canvases.Length > 0, $"count={canvases.Length}");

        // Hotbar
        var hotbarSlots = FindObjectsByType<HotbarSlotUI>(FindObjectsSortMode.None);
        Check("hotbar_has_slots", hotbarSlots.Length > 0, $"count={hotbarSlots.Length}");
    }

    // -- World items --

    private void ValidateWorldItems()
    {
        var worldItems = FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
        Check("world_items_exist", worldItems.Length > 0, $"count={worldItems.Length}");

        if (worldItems.Length > 0)
        {
            var first = worldItems[0];
            var collider = first.GetComponent<Collider>();
            Check("world_item_has_collider", collider != null,
                first.name + " missing collider");
            if (collider != null)
                Check("world_item_collider_is_trigger", collider.isTrigger,
                    first.name + " collider.isTrigger=" + collider.isTrigger);
        }
    }

    // -- Combat --

    private void ValidateCombat()
    {
        // Enemy template is inactive, so Find won't work -- use FindObjectsByType with inactive
        var enemies = Resources.FindObjectsOfTypeAll<FaunaController>();
        var enemyTemplate = enemies.Length > 0 ? enemies[0].gameObject : null;
        Check("enemy_template_exists", enemyTemplate != null, $"FaunaController count={enemies.Length}");
        if (enemyTemplate != null)
            Check("enemy_template_inactive", !enemyTemplate.activeSelf,
                "template should be inactive");

        var waveControllers = FindObjectsByType<WaveControllerBehaviour>(FindObjectsSortMode.None);
        Check("wave_controller_exists", waveControllers.Length > 0,
            $"count={waveControllers.Length}");

        var spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        Check("enemy_spawner_exists", spawners.Length > 0,
            $"count={spawners.Length}");
    }

    // -- Buildings (Kevin-specific) --

    private void ValidateBuildings()
    {
        var entryPortal = GameObject.Find("BuildingEntryPortal");
        Check("building_entry_portal_exists", entryPortal != null);
        if (entryPortal != null)
        {
            var trigger = entryPortal.GetComponent<BuildingEntryTrigger>();
            Check("entry_portal_has_trigger", trigger != null);
            var col = entryPortal.GetComponent<BoxCollider>();
            Check("entry_portal_has_collider", col != null && col.isTrigger);
        }

        var exitPortal = GameObject.Find("BuildingExitPortal");
        Check("building_exit_portal_exists", exitPortal != null);

        var mepPoints = FindObjectsByType<MEPRestorePointBehaviour>(FindObjectsSortMode.None);
        Check("mep_points_exist", mepPoints.Length == 4,
            $"expected 4, got {mepPoints.Length}");
    }

    // -- Supply chain --

    private void ValidateSupplyChain()
    {
        var supplyDock = GameObject.Find("SupplyDock");
        Check("supply_dock_exists", supplyDock != null);
        if (supplyDock != null)
        {
            var storageBeh = supplyDock.GetComponent<StorageBehaviour>();
            Check("supply_dock_has_storage_behaviour", storageBeh != null);
            Check("supply_dock_on_interactable_layer",
                supplyDock.layer == PhysicsLayers.Interactable,
                $"layer={supplyDock.layer}");
        }

        var mapUI = FindAnyObjectByType<OverworldMapUI>();
        Check("overworld_map_ui_exists", mapUI != null);
    }

    // -- NavMesh --

    private void ValidateNavMesh()
    {
        var hasNavMesh = UnityEngine.AI.NavMesh.SamplePosition(
            Vector3.zero, out _, 100f, UnityEngine.AI.NavMesh.AllAreas);
        Check("navmesh_exists", hasNavMesh);
    }
}
