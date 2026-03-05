using UnityEngine;

/// <summary>
/// Return type for TowerChunkLayoutGenerator. Contains references to key transforms
/// within a generated floor chunk.
/// </summary>
public struct TowerChunkLayout
{
    public GameObject Root;
    public Transform[] EnemySpawnPoints;
    public Transform[] LootNodePositions;
    public Transform FragmentPosition;
    public Transform ElevatorPosition;
}

/// <summary>
/// Static utility that creates tower floor chunks from primitives at runtime.
/// Each chunk is a room with floor, ceiling, walls, and a south-wall doorway for the elevator.
/// Normal chunks are 20x20, boss chunk is 30x30.
/// All geometry on PhysicsLayers.BIM_Static for NavMesh baking, fauna LOS, and weapon hits.
/// </summary>
public static class TowerChunkLayoutGenerator
{
    private const float WallThickness = 0.2f;
    private const float WallHeight = 4f;
    private const float FloorThickness = 0.1f;
    private const float DoorWidth = 2.5f;
    public const float ChunkHeight = 5f;

    public const float NormalSize = 20f;
    public const float BossSize = 30f;

    private static readonly Color NormalFloorColor = new Color(0.3f, 0.3f, 0.35f);
    private static readonly Color NormalWallColor = new Color(0.45f, 0.45f, 0.5f);
    private static readonly Color NormalCeilingColor = new Color(0.35f, 0.35f, 0.4f);
    private static readonly Color BossFloorColor = new Color(0.2f, 0.15f, 0.15f);
    private static readonly Color BossWallColor = new Color(0.35f, 0.2f, 0.2f);
    private static readonly Color BossCeilingColor = new Color(0.25f, 0.15f, 0.15f);

    /// <summary>
    /// Returns the world-space origin for a chunk at the given floor index, stacked vertically.
    /// </summary>
    public static Vector3 GetChunkOrigin(Vector3 towerBase, int floorIndex)
    {
        return towerBase + new Vector3(0f, floorIndex * ChunkHeight, 0f);
    }

    /// <summary>
    /// Generate a single floor chunk at the given origin.
    /// </summary>
    /// <param name="origin">World position of the chunk's bottom-southwest corner.</param>
    /// <param name="floorIndex">Floor number (used for naming).</param>
    /// <param name="isBoss">True for boss chunk (larger room, darker coloring).</param>
    /// <param name="spawnPointCount">Number of enemy spawn points to create.</param>
    /// <param name="lootNodeCount">Number of loot node positions to create.</param>
    /// <param name="hasFragment">Whether this chunk has a fragment node position.</param>
    public static TowerChunkLayout GenerateChunk(
        Vector3 origin, int floorIndex, bool isBoss,
        int spawnPointCount, int lootNodeCount, bool hasFragment)
    {
        float size = isBoss ? BossSize : NormalSize;
        var floorColor = isBoss ? BossFloorColor : NormalFloorColor;
        var wallColor = isBoss ? BossWallColor : NormalWallColor;
        var ceilingColor = isBoss ? BossCeilingColor : NormalCeilingColor;

        var root = new GameObject($"TowerChunk_F{floorIndex}");
        root.transform.position = origin;

        // Floor
        CreateSurface(root.transform, "Floor", origin,
            new Vector3(size * 0.5f, -FloorThickness * 0.5f, size * 0.5f),
            new Vector3(size, FloorThickness, size), floorColor);

        // Ceiling
        CreateSurface(root.transform, "Ceiling", origin,
            new Vector3(size * 0.5f, WallHeight + FloorThickness * 0.5f, size * 0.5f),
            new Vector3(size, FloorThickness, size), ceilingColor);

        // Walls with south doorway
        CreateWalls(root.transform, origin, size, wallColor);

        // Elevator position -- inside the doorway on south wall
        var elevatorPos = CreateMarker(root.transform, "ElevatorPoint",
            origin + new Vector3(size * 0.5f, 0f, 1.5f));

        // Enemy spawn points -- distributed around the room
        var spawnPoints = CreateSpawnPoints(root.transform, origin, size, spawnPointCount, floorIndex);

        // Loot node positions -- along walls and corners
        var lootPositions = CreateLootPositions(root.transform, origin, size, lootNodeCount, floorIndex);

        // Fragment position
        Transform fragmentPos = null;
        if (hasFragment)
        {
            fragmentPos = CreateMarker(root.transform, "FragmentPoint",
                origin + new Vector3(size * 0.5f, 0.5f, size * 0.7f));
        }

        return new TowerChunkLayout
        {
            Root = root,
            EnemySpawnPoints = spawnPoints,
            LootNodePositions = lootPositions,
            FragmentPosition = fragmentPos,
            ElevatorPosition = elevatorPos,
        };
    }

    private static void CreateWalls(Transform parent, Vector3 origin, float size, Color color)
    {
        float halfHeight = WallHeight * 0.5f;
        float doorHalf = DoorWidth * 0.5f;
        float centerX = size * 0.5f;

        // South wall -- two sections with doorway gap in center
        float leftWidth = centerX - doorHalf;
        CreateWall(parent, "Wall_South_L", origin + new Vector3(leftWidth * 0.5f, halfHeight, 0f),
            new Vector3(leftWidth, WallHeight, WallThickness), color);

        float rightStart = centerX + doorHalf;
        float rightWidth = size - rightStart;
        CreateWall(parent, "Wall_South_R", origin + new Vector3(rightStart + rightWidth * 0.5f, halfHeight, 0f),
            new Vector3(rightWidth, WallHeight, WallThickness), color);

        // North wall (solid)
        CreateWall(parent, "Wall_North", origin + new Vector3(size * 0.5f, halfHeight, size),
            new Vector3(size, WallHeight, WallThickness), color);

        // West wall (solid)
        CreateWall(parent, "Wall_West", origin + new Vector3(0f, halfHeight, size * 0.5f),
            new Vector3(WallThickness, WallHeight, size), color);

        // East wall (solid)
        CreateWall(parent, "Wall_East", origin + new Vector3(size, halfHeight, size * 0.5f),
            new Vector3(WallThickness, WallHeight, size), color);
    }

    private static Transform[] CreateSpawnPoints(
        Transform parent, Vector3 origin, float size, int count, int floorIndex)
    {
        var points = new Transform[count];
        float margin = 3f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / Mathf.Max(1, count - 1);
            float x, z;

            if (i % 2 == 0)
            {
                x = Mathf.Lerp(margin, size - margin, t);
                z = size - margin;
            }
            else
            {
                x = Mathf.Lerp(size - margin, margin, t);
                z = size * 0.5f;
            }

            points[i] = CreateMarker(parent, $"SpawnPoint_F{floorIndex}_{i}",
                origin + new Vector3(x, 0f, z));
        }

        return points;
    }

    private static Transform[] CreateLootPositions(
        Transform parent, Vector3 origin, float size, int count, int floorIndex)
    {
        var positions = new Transform[count];
        float margin = 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = (float)i / count * Mathf.PI * 2f;
            float radius = size * 0.35f;
            float x = size * 0.5f + Mathf.Cos(angle) * radius;
            float z = size * 0.5f + Mathf.Sin(angle) * radius;

            x = Mathf.Clamp(x, margin, size - margin);
            z = Mathf.Clamp(z, margin, size - margin);

            positions[i] = CreateMarker(parent, $"LootNode_F{floorIndex}_{i}",
                origin + new Vector3(x, 0.5f, z));
        }

        return positions;
    }

    private static void CreateSurface(
        Transform parent, string name, Vector3 origin,
        Vector3 localOffset, Vector3 scale, Color color)
    {
        var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = name;
        surface.isStatic = true;
        surface.layer = PhysicsLayers.BIM_Static;
        surface.transform.SetParent(parent, true);
        surface.transform.position = origin + localOffset;
        surface.transform.localScale = scale;
        SetColor(surface, color);
    }

    private static void CreateWall(
        Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.isStatic = true;
        wall.layer = PhysicsLayers.BIM_Static;
        wall.transform.SetParent(parent, true);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        SetColor(wall, color);
    }

    private static Transform CreateMarker(Transform parent, string name, Vector3 position)
    {
        var marker = new GameObject(name);
        marker.transform.SetParent(parent, true);
        marker.transform.position = position;
        return marker.transform;
    }

    private static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = new Material(renderer.sharedMaterial);
        mat.color = color;
        renderer.sharedMaterial = mat;
    }
}
