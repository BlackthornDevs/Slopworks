using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool that generates tower floor chunk prefabs from Kenney asset pack models.
/// Creates modular room layouts with walls, floors, cover, and industrial props.
/// Run via Slopworks > Build Tower Floor Prefabs.
/// </summary>
public static class TowerFloorPrefabBuilder
{
    private const string KenneyConveyor = "Assets/_Slopworks/Art/Kenney/conveyor-kit/Models/";
    private const string KenneySurvival = "Assets/_Slopworks/Art/Kenney/survival-kit/Models/";
    private const string PrefabOutputDir = "Assets/_Slopworks/Prefabs/Tower/";

    // Kenney wall panels are 2m wide, 3m tall
    private const float WallPanelWidth = 2f;
    private const float WallHeight = 3f;
    // Floor tiles are 2x2m
    private const float FloorTileSize = 2f;
    // Prop scale multiplier (Kenney props are small, ~0.25m)
    private const float PropScale = 3f;

    // Room sizes
    private const float NormalRoomSize = 20f;
    private const float BossRoomSize = 30f;

    [MenuItem("Slopworks/Build Tower Floor Prefabs")]
    public static void BuildAll()
    {
        EnsureDirectoryExists(PrefabOutputDir);

        BuildFloorPrefab("TowerFloor_Lobby", NormalRoomSize, FloorStyle.Lobby);
        BuildFloorPrefab("TowerFloor_Industrial", NormalRoomSize, FloorStyle.Industrial);
        BuildFloorPrefab("TowerFloor_Storage", NormalRoomSize, FloorStyle.Storage);
        BuildFloorPrefab("TowerFloor_Processing", NormalRoomSize, FloorStyle.Processing);
        BuildFloorPrefab("TowerFloor_Mechanical", NormalRoomSize, FloorStyle.Mechanical);
        BuildFloorPrefab("TowerFloor_Boss", BossRoomSize, FloorStyle.Boss);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("tower floor prefabs: built 6 prefabs in " + PrefabOutputDir);
    }

    private enum FloorStyle
    {
        Lobby,
        Industrial,
        Storage,
        Processing,
        Mechanical,
        Boss
    }

    private static void BuildFloorPrefab(string prefabName, float roomSize, FloorStyle style)
    {
        var root = new GameObject(prefabName);

        // Floor
        BuildFloor(root.transform, roomSize);

        // Ceiling
        BuildCeiling(root.transform, roomSize);

        // Walls with doorway on south side
        BuildWalls(root.transform, roomSize, style);

        // Interior props and cover based on style
        BuildInterior(root.transform, roomSize, style);

        // Markers for gameplay
        BuildMarkers(root.transform, roomSize, style);

        // Set all geometry to BIM_Static layer
        SetLayerRecursive(root, PhysicsLayers.BIM_Static);

        // Save as prefab
        string path = PrefabOutputDir + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        Debug.Log($"tower prefab: saved {prefabName} ({roomSize}x{roomSize}m, {style})");
    }

    private static void BuildFloor(Transform parent, float roomSize)
    {
        var floorTile = LoadModel(KenneyConveyor + "floor-large.fbx");
        if (floorTile == null) return;

        var floorParent = new GameObject("Floor");
        floorParent.transform.SetParent(parent, false);

        int tilesPerSide = Mathf.CeilToInt(roomSize / FloorTileSize);
        for (int x = 0; x < tilesPerSide; x++)
        {
            for (int z = 0; z < tilesPerSide; z++)
            {
                var tile = (GameObject)PrefabUtility.InstantiatePrefab(floorTile);
                tile.name = $"FloorTile_{x}_{z}";
                tile.transform.SetParent(floorParent.transform, false);
                tile.transform.localPosition = new Vector3(
                    x * FloorTileSize + FloorTileSize * 0.5f,
                    0f,
                    z * FloorTileSize + FloorTileSize * 0.5f);
                tile.isStatic = true;
            }
        }
    }

    private static void BuildCeiling(Transform parent, float roomSize)
    {
        // Use a simple scaled cube for ceiling since Kenney doesn't have ceiling tiles
        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.isStatic = true;
        ceiling.transform.SetParent(parent, false);
        ceiling.transform.localPosition = new Vector3(roomSize * 0.5f, WallHeight, roomSize * 0.5f);
        ceiling.transform.localScale = new Vector3(roomSize, 0.1f, roomSize);

        var renderer = ceiling.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.3f, 0.3f, 0.35f);
        renderer.sharedMaterial = mat;
    }

    private static void BuildWalls(Transform parent, float roomSize, FloorStyle style)
    {
        var wallModel = LoadModel(KenneyConveyor + "structure-wall.fbx");
        var doorwayModel = LoadModel(KenneyConveyor + "structure-doorway.fbx");
        var windowModel = LoadModel(KenneyConveyor + "structure-window.fbx");

        if (wallModel == null) return;

        var wallParent = new GameObject("Walls");
        wallParent.transform.SetParent(parent, false);

        int panelsPerSide = Mathf.CeilToInt(roomSize / WallPanelWidth);
        int doorPanel = panelsPerSide / 2; // door in the middle of south wall

        // Decide window placement per style
        bool hasWindows = style != FloorStyle.Boss && windowModel != null;
        var windowPanels = new HashSet<int>();
        if (hasWindows)
        {
            // Windows every 3rd panel on north, east, west walls
            for (int i = 2; i < panelsPerSide; i += 3)
                windowPanels.Add(i);
        }

        // South wall (with doorway)
        for (int i = 0; i < panelsPerSide; i++)
        {
            var model = (i == doorPanel && doorwayModel != null) ? doorwayModel : wallModel;
            PlaceWallPanel(wallParent.transform, model, "South",
                new Vector3(i * WallPanelWidth, 0f, 0f),
                Quaternion.identity, i);
        }

        // North wall
        for (int i = 0; i < panelsPerSide; i++)
        {
            var model = (windowPanels.Contains(i) && windowModel != null) ? windowModel : wallModel;
            PlaceWallPanel(wallParent.transform, model, "North",
                new Vector3(i * WallPanelWidth, 0f, roomSize),
                Quaternion.Euler(0f, 180f, 0f), i);
        }

        // West wall
        for (int i = 0; i < panelsPerSide; i++)
        {
            var model = (windowPanels.Contains(i) && windowModel != null) ? windowModel : wallModel;
            PlaceWallPanel(wallParent.transform, model, "West",
                new Vector3(0f, 0f, i * WallPanelWidth + WallPanelWidth),
                Quaternion.Euler(0f, 90f, 0f), i);
        }

        // East wall
        for (int i = 0; i < panelsPerSide; i++)
        {
            var model = (windowPanels.Contains(i) && windowModel != null) ? windowModel : wallModel;
            PlaceWallPanel(wallParent.transform, model, "East",
                new Vector3(roomSize, 0f, i * WallPanelWidth),
                Quaternion.Euler(0f, -90f, 0f), i);
        }
    }

    private static void PlaceWallPanel(Transform parent, GameObject model, string side,
        Vector3 position, Quaternion rotation, int index)
    {
        var panel = (GameObject)PrefabUtility.InstantiatePrefab(model);
        panel.name = $"Wall_{side}_{index}";
        panel.isStatic = true;
        panel.transform.SetParent(parent, false);
        panel.transform.localPosition = position;
        panel.transform.localRotation = rotation;
    }

    private static void BuildInterior(Transform parent, float roomSize, FloorStyle style)
    {
        var interiorParent = new GameObject("Interior");
        interiorParent.transform.SetParent(parent, false);

        var rng = new System.Random(style.GetHashCode());

        switch (style)
        {
            case FloorStyle.Lobby:
                PlaceLobbyProps(interiorParent.transform, roomSize, rng);
                break;
            case FloorStyle.Industrial:
                PlaceIndustrialProps(interiorParent.transform, roomSize, rng);
                break;
            case FloorStyle.Storage:
                PlaceStorageProps(interiorParent.transform, roomSize, rng);
                break;
            case FloorStyle.Processing:
                PlaceProcessingProps(interiorParent.transform, roomSize, rng);
                break;
            case FloorStyle.Mechanical:
                PlaceMechanicalProps(interiorParent.transform, roomSize, rng);
                break;
            case FloorStyle.Boss:
                PlaceBossArenaProps(interiorParent.transform, roomSize, rng);
                break;
        }
    }

    private static void PlaceLobbyProps(Transform parent, float size, System.Random rng)
    {
        // Lobby: sparse, a few barrels and boxes near walls, welcome area
        var barrel = LoadModel(KenneySurvival + "barrel.fbx");
        var box = LoadModel(KenneySurvival + "box-large.fbx");
        var fence = LoadModel(KenneySurvival + "fence.fbx");

        // Barrels along east wall
        PlacePropsAlongWall(parent, barrel, "barrel", size, "east", 3, PropScale, rng);

        // Boxes near northwest corner
        PlacePropCluster(parent, box, "box", new Vector3(3f, 0f, size - 3f), 4, PropScale, rng);

        // Low fence as reception barrier
        if (fence != null)
        {
            for (int i = 0; i < 4; i++)
            {
                var f = (GameObject)PrefabUtility.InstantiatePrefab(fence);
                f.name = $"fence_{i}";
                f.isStatic = true;
                f.transform.SetParent(parent, false);
                f.transform.localPosition = new Vector3(size * 0.3f + i * 1.5f, 0f, size * 0.4f);
                f.transform.localScale = Vector3.one * PropScale;
            }
        }
    }

    private static void PlaceIndustrialProps(Transform parent, float size, System.Random rng)
    {
        // Industrial: conveyor lines, robot arms, cover objects
        var conveyor = LoadModel(KenneyConveyor + "conveyor.fbx");
        var conveyorStripe = LoadModel(KenneyConveyor + "conveyor-stripe.fbx");
        var robotArm = LoadModel(KenneyConveyor + "robot-arm-a.fbx");
        var cover = LoadModel(KenneyConveyor + "cover.fbx");
        var barrel = LoadModel(KenneySurvival + "barrel.fbx");

        // Conveyor line running through the middle
        if (conveyor != null)
        {
            for (int i = 0; i < 8; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(
                    i % 2 == 0 ? conveyor : (conveyorStripe ?? conveyor));
                c.name = $"conveyor_{i}";
                c.isStatic = true;
                c.transform.SetParent(parent, false);
                c.transform.localPosition = new Vector3(size * 0.5f, 0f, 4f + i * 1.5f);
            }
        }

        // Robot arms flanking the conveyor
        if (robotArm != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var arm = (GameObject)PrefabUtility.InstantiatePrefab(robotArm);
                arm.name = $"robot_arm_L_{i}";
                arm.isStatic = true;
                arm.transform.SetParent(parent, false);
                arm.transform.localPosition = new Vector3(size * 0.5f - 2f, 0f, 5f + i * 4f);
                arm.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

                var armR = (GameObject)PrefabUtility.InstantiatePrefab(robotArm);
                armR.name = $"robot_arm_R_{i}";
                armR.isStatic = true;
                armR.transform.SetParent(parent, false);
                armR.transform.localPosition = new Vector3(size * 0.5f + 2f, 0f, 5f + i * 4f);
                armR.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }
        }

        // Cover objects for combat
        PlaceCoverObjects(parent, cover, size, 6, rng);

        // Scattered barrels
        PlacePropsAlongWall(parent, barrel, "barrel", size, "west", 4, PropScale, rng);
    }

    private static void PlaceStorageProps(Transform parent, float size, System.Random rng)
    {
        // Storage: rows of boxes and barrels, shelving (fences as dividers)
        var box = LoadModel(KenneySurvival + "box-large.fbx");
        var boxOpen = LoadModel(KenneySurvival + "box-large-open.fbx");
        var barrel = LoadModel(KenneySurvival + "barrel.fbx");
        var chest = LoadModel(KenneySurvival + "chest.fbx");
        var cover = LoadModel(KenneyConveyor + "cover.fbx");
        var fence = LoadModel(KenneySurvival + "fence.fbx");

        // Storage rows (3 rows of boxes)
        for (int row = 0; row < 3; row++)
        {
            float rowZ = 5f + row * 5f;
            for (int col = 0; col < 6; col++)
            {
                var model = (rng.Next(3) == 0) ? (boxOpen ?? box) : box;
                if (model == null) continue;

                var b = (GameObject)PrefabUtility.InstantiatePrefab(model);
                b.name = $"storage_box_{row}_{col}";
                b.isStatic = true;
                b.transform.SetParent(parent, false);
                b.transform.localPosition = new Vector3(3f + col * 2.5f, 0f, rowZ);
                b.transform.localScale = Vector3.one * PropScale;
                b.transform.localRotation = Quaternion.Euler(0f, rng.Next(4) * 90f, 0f);
            }
        }

        // Barrels along one wall
        PlacePropsAlongWall(parent, barrel, "barrel", size, "east", 5, PropScale, rng);

        // A couple of chests (loot containers)
        if (chest != null)
        {
            for (int i = 0; i < 2; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(chest);
                c.name = $"chest_{i}";
                c.isStatic = true;
                c.transform.SetParent(parent, false);
                c.transform.localPosition = new Vector3(size - 3f, 0f, 4f + i * 8f);
                c.transform.localScale = Vector3.one * PropScale;
            }
        }

        // Cover between rows
        PlaceCoverObjects(parent, cover, size, 4, rng);
    }

    private static void PlaceProcessingProps(Transform parent, float size, System.Random rng)
    {
        // Processing: workbenches, conveyors, metal panels
        var workbench = LoadModel(KenneySurvival + "workbench.fbx");
        var grinder = LoadModel(KenneySurvival + "workbench-grind.fbx");
        var anvil = LoadModel(KenneySurvival + "workbench-anvil.fbx");
        var conveyor = LoadModel(KenneyConveyor + "conveyor-bars.fbx");
        var cover = LoadModel(KenneyConveyor + "cover.fbx");
        var barrel = LoadModel(KenneySurvival + "barrel.fbx");

        // Workstation clusters
        GameObject[] stations = { workbench, grinder, anvil };
        for (int i = 0; i < 4; i++)
        {
            var model = stations[rng.Next(stations.Length)];
            if (model == null) continue;

            var ws = (GameObject)PrefabUtility.InstantiatePrefab(model);
            ws.name = $"workstation_{i}";
            ws.isStatic = true;
            ws.transform.SetParent(parent, false);
            ws.transform.localPosition = new Vector3(
                4f + (i % 2) * (size - 8f),
                0f,
                6f + (i / 2) * (size - 12f));
            ws.transform.localScale = Vector3.one * PropScale;
            ws.transform.localRotation = Quaternion.Euler(0f, rng.Next(4) * 90f, 0f);
        }

        // Conveyor segments
        if (conveyor != null)
        {
            for (int i = 0; i < 6; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(conveyor);
                c.name = $"conveyor_{i}";
                c.isStatic = true;
                c.transform.SetParent(parent, false);
                c.transform.localPosition = new Vector3(size * 0.5f - 3f + i * 1f, 0f, size * 0.5f);
            }
        }

        PlaceCoverObjects(parent, cover, size, 5, rng);
        PlacePropsAlongWall(parent, barrel, "barrel", size, "north", 3, PropScale, rng);
    }

    private static void PlaceMechanicalProps(Transform parent, float size, System.Random rng)
    {
        // Mechanical: heavy machinery look, scanners, robot arms, pipes (covers)
        var robotA = LoadModel(KenneyConveyor + "robot-arm-a.fbx");
        var robotB = LoadModel(KenneyConveyor + "robot-arm-b.fbx");
        var scanner = LoadModel(KenneyConveyor + "scanner-high.fbx");
        var coverBar = LoadModel(KenneyConveyor + "cover-bar.fbx");
        var coverHopper = LoadModel(KenneyConveyor + "cover-hopper.fbx");
        var cover = LoadModel(KenneyConveyor + "cover.fbx");
        var barrel = LoadModel(KenneySurvival + "barrel-open.fbx");

        // Large machinery (robot arms)
        GameObject[] machines = { robotA, robotB };
        for (int i = 0; i < 4; i++)
        {
            var model = machines[rng.Next(machines.Length)];
            if (model == null) continue;

            var m = (GameObject)PrefabUtility.InstantiatePrefab(model);
            m.name = $"machine_{i}";
            m.isStatic = true;
            m.transform.SetParent(parent, false);
            float x = (i % 2 == 0) ? size * 0.25f : size * 0.75f;
            float z = (i < 2) ? size * 0.35f : size * 0.65f;
            m.transform.localPosition = new Vector3(x, 0f, z);
            m.transform.localRotation = Quaternion.Euler(0f, rng.Next(4) * 90f, 0f);
        }

        // Scanners
        if (scanner != null)
        {
            for (int i = 0; i < 2; i++)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(scanner);
                s.name = $"scanner_{i}";
                s.isStatic = true;
                s.transform.SetParent(parent, false);
                s.transform.localPosition = new Vector3(
                    size * 0.5f, 0f, 3f + i * (size - 6f));
            }
        }

        // Hopper/pipe covers along walls
        PlaceCoverRow(parent, coverHopper ?? coverBar ?? cover, "hopper", size, 4, rng);
        PlaceCoverObjects(parent, cover, size, 6, rng);
        PlacePropsAlongWall(parent, barrel, "barrel_open", size, "west", 3, PropScale, rng);
    }

    private static void PlaceBossArenaProps(Transform parent, float size, System.Random rng)
    {
        // Boss: open arena with pillars (covers) around edges, minimal clutter
        var cover = LoadModel(KenneyConveyor + "cover.fbx");
        var coverCorner = LoadModel(KenneyConveyor + "cover-corner.fbx");
        var barrel = LoadModel(KenneySurvival + "barrel.fbx");

        // Ring of cover pillars around the arena perimeter
        int pillarCount = 8;
        float radius = size * 0.35f;
        var pillarModel = coverCorner ?? cover;

        if (pillarModel != null)
        {
            for (int i = 0; i < pillarCount; i++)
            {
                float angle = (float)i / pillarCount * Mathf.PI * 2f;
                float x = size * 0.5f + Mathf.Cos(angle) * radius;
                float z = size * 0.5f + Mathf.Sin(angle) * radius;

                var pillar = (GameObject)PrefabUtility.InstantiatePrefab(pillarModel);
                pillar.name = $"arena_pillar_{i}";
                pillar.isStatic = true;
                pillar.transform.SetParent(parent, false);
                pillar.transform.localPosition = new Vector3(x, 0f, z);
                pillar.transform.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
            }
        }

        // Corner debris clusters
        Vector3[] corners = {
            new Vector3(3f, 0f, 3f),
            new Vector3(size - 3f, 0f, 3f),
            new Vector3(3f, 0f, size - 3f),
            new Vector3(size - 3f, 0f, size - 3f)
        };

        foreach (var corner in corners)
            PlacePropCluster(parent, barrel, "debris", corner, 3, PropScale, rng);
    }

    // --- Helper methods ---

    private static void PlaceCoverObjects(Transform parent, GameObject coverModel,
        float roomSize, int count, System.Random rng)
    {
        if (coverModel == null) return;

        float margin = 4f;
        for (int i = 0; i < count; i++)
        {
            var c = (GameObject)PrefabUtility.InstantiatePrefab(coverModel);
            c.name = $"cover_{i}";
            c.isStatic = true;
            c.transform.SetParent(parent, false);
            c.transform.localPosition = new Vector3(
                margin + (float)rng.NextDouble() * (roomSize - margin * 2),
                0f,
                margin + (float)rng.NextDouble() * (roomSize - margin * 2));
            c.transform.localRotation = Quaternion.Euler(0f, rng.Next(4) * 90f, 0f);
        }
    }

    private static void PlaceCoverRow(Transform parent, GameObject model, string baseName,
        float roomSize, int count, System.Random rng)
    {
        if (model == null) return;

        for (int i = 0; i < count; i++)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(model);
            obj.name = $"{baseName}_{i}";
            obj.isStatic = true;
            obj.transform.SetParent(parent, false);
            float t = (float)(i + 1) / (count + 1);
            obj.transform.localPosition = new Vector3(t * roomSize, 0f, roomSize * 0.5f);
            obj.transform.localRotation = Quaternion.Euler(0f, rng.Next(2) * 180f, 0f);
        }
    }

    private static void PlacePropsAlongWall(Transform parent, GameObject model, string baseName,
        float roomSize, string wall, int count, float scale, System.Random rng)
    {
        if (model == null) return;

        for (int i = 0; i < count; i++)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(model);
            obj.name = $"{baseName}_{wall}_{i}";
            obj.isStatic = true;
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = Vector3.one * scale;

            float spread = roomSize * 0.8f;
            float offset = roomSize * 0.1f + (float)rng.NextDouble() * spread;

            switch (wall)
            {
                case "north":
                    obj.transform.localPosition = new Vector3(offset, 0f, roomSize - 1.5f);
                    break;
                case "south":
                    obj.transform.localPosition = new Vector3(offset, 0f, 1.5f);
                    break;
                case "east":
                    obj.transform.localPosition = new Vector3(roomSize - 1.5f, 0f, offset);
                    break;
                case "west":
                    obj.transform.localPosition = new Vector3(1.5f, 0f, offset);
                    break;
            }

            obj.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
        }
    }

    private static void PlacePropCluster(Transform parent, GameObject model, string baseName,
        Vector3 center, int count, float scale, System.Random rng)
    {
        if (model == null) return;

        for (int i = 0; i < count; i++)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(model);
            obj.name = $"{baseName}_cluster_{i}";
            obj.isStatic = true;
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = Vector3.one * scale;
            obj.transform.localPosition = center + new Vector3(
                ((float)rng.NextDouble() - 0.5f) * 3f,
                0f,
                ((float)rng.NextDouble() - 0.5f) * 3f);
            obj.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
        }
    }

    private static void BuildMarkers(Transform parent, float roomSize, FloorStyle style)
    {
        var markers = new GameObject("Markers");
        markers.transform.SetParent(parent, false);

        // Elevator point -- south wall center, inside doorway
        CreateMarker(markers.transform, "ElevatorPoint",
            new Vector3(roomSize * 0.5f, 0f, 1.5f));

        // Spawn points -- distributed around back half of room
        int spawnCount = style == FloorStyle.Boss ? 6 : 4;
        var spawnParent = new GameObject("SpawnPoints");
        spawnParent.transform.SetParent(markers.transform, false);

        for (int i = 0; i < spawnCount; i++)
        {
            float t = (float)i / Mathf.Max(1, spawnCount - 1);
            float x, z;
            if (i % 2 == 0)
            {
                x = Mathf.Lerp(3f, roomSize - 3f, t);
                z = roomSize - 3f;
            }
            else
            {
                x = Mathf.Lerp(roomSize - 3f, 3f, t);
                z = roomSize * 0.5f;
            }
            CreateMarker(spawnParent.transform, $"SpawnPoint_{i}", new Vector3(x, 0f, z));
        }

        // Loot node positions -- ring around room center
        int lootCount = style == FloorStyle.Boss ? 4 : 3;
        var lootParent = new GameObject("LootNodes");
        lootParent.transform.SetParent(markers.transform, false);

        for (int i = 0; i < lootCount; i++)
        {
            float angle = (float)i / lootCount * Mathf.PI * 2f;
            float radius = roomSize * 0.3f;
            CreateMarker(lootParent.transform, $"LootNode_{i}",
                new Vector3(
                    roomSize * 0.5f + Mathf.Cos(angle) * radius,
                    0.5f,
                    roomSize * 0.5f + Mathf.Sin(angle) * radius));
        }

        // Fragment position
        CreateMarker(markers.transform, "FragmentPoint",
            new Vector3(roomSize * 0.5f, 0.5f, roomSize * 0.7f));
    }

    private static void CreateMarker(Transform parent, string name, Vector3 position)
    {
        var marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.localPosition = position;
    }

    private static GameObject LoadModel(string path)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (model == null)
            Debug.LogWarning($"tower prefab builder: model not found at {path}");
        return model;
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
        {
            var parts = path.TrimEnd('/').Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
