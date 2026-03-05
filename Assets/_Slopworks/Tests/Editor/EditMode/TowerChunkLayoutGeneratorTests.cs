using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TowerChunkLayoutGeneratorTests
{
    private TowerChunkLayout _layout;

    [TearDown]
    public void TearDown()
    {
        if (_layout.Root != null)
            Object.DestroyImmediate(_layout.Root);
    }

    // -- Root and basic structure --

    [Test]
    public void GenerateChunk_CreatesRootWithFloorAndCeiling()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, false);

        Assert.IsNotNull(_layout.Root);
        Assert.IsNotNull(_layout.Root.transform.Find("Floor"), "Floor missing");
        Assert.IsNotNull(_layout.Root.transform.Find("Ceiling"), "Ceiling missing");
    }

    [Test]
    public void GenerateChunk_AllGeometryOnBIMStaticLayer()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, false);

        var renderers = _layout.Root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            Assert.AreEqual(PhysicsLayers.BIM_Static, r.gameObject.layer,
                $"{r.name} should be on BIM_Static layer");
        }
    }

    [Test]
    public void GenerateChunk_AllGeometryIsStatic()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, false);

        var renderers = _layout.Root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            Assert.IsTrue(r.gameObject.isStatic,
                $"{r.name} should be static for NavMesh baking");
        }
    }

    [Test]
    public void GenerateChunk_HasWalls()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, false);

        Assert.IsNotNull(_layout.Root.transform.Find("Wall_North"), "North wall missing");
        Assert.IsNotNull(_layout.Root.transform.Find("Wall_West"), "West wall missing");
        Assert.IsNotNull(_layout.Root.transform.Find("Wall_East"), "East wall missing");
        Assert.IsNotNull(_layout.Root.transform.Find("Wall_South_L"), "South left wall missing");
        Assert.IsNotNull(_layout.Root.transform.Find("Wall_South_R"), "South right wall missing");
    }

    // -- Spawn points and loot nodes --

    [Test]
    public void GenerateChunk_SpawnPointCountMatchesRequest()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 5, 2, false);

        Assert.AreEqual(5, _layout.EnemySpawnPoints.Length);
    }

    [Test]
    public void GenerateChunk_LootNodeCountMatchesRequest()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 4, false);

        Assert.AreEqual(4, _layout.LootNodePositions.Length);
    }

    [Test]
    public void GenerateChunk_SpawnPointsAreNonNull()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 4, 2, false);

        for (int i = 0; i < _layout.EnemySpawnPoints.Length; i++)
            Assert.IsNotNull(_layout.EnemySpawnPoints[i], $"SpawnPoint[{i}] is null");
    }

    [Test]
    public void GenerateChunk_LootPositionsAreNonNull()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 3, false);

        for (int i = 0; i < _layout.LootNodePositions.Length; i++)
            Assert.IsNotNull(_layout.LootNodePositions[i], $"LootNode[{i}] is null");
    }

    // -- Boss vs normal size --

    [Test]
    public void GenerateChunk_BossChunkLargerThanNormal()
    {
        var normal = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, false);
        var boss = TowerChunkLayoutGenerator.GenerateChunk(new Vector3(100, 0, 0), 6, true, 8, 4, false);

        var normalFloor = normal.Root.transform.Find("Floor");
        var bossFloor = boss.Root.transform.Find("Floor");

        Assert.Greater(bossFloor.localScale.x, normalFloor.localScale.x,
            "Boss floor should be wider than normal");
        Assert.Greater(bossFloor.localScale.z, normalFloor.localScale.z,
            "Boss floor should be deeper than normal");

        Object.DestroyImmediate(normal.Root);
        Object.DestroyImmediate(boss.Root);
    }

    // -- Fragment position --

    [Test]
    public void GenerateChunk_WithFragment_HasFragmentPosition()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, true);

        Assert.IsNotNull(_layout.FragmentPosition);
    }

    [Test]
    public void GenerateChunk_WithoutFragment_FragmentPositionIsNull()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, false);

        Assert.IsNull(_layout.FragmentPosition);
    }

    // -- Elevator position --

    [Test]
    public void GenerateChunk_HasElevatorPosition()
    {
        _layout = TowerChunkLayoutGenerator.GenerateChunk(Vector3.zero, 0, false, 3, 2, false);

        Assert.IsNotNull(_layout.ElevatorPosition);
    }

    // -- GetChunkOrigin --

    [Test]
    public void GetChunkOrigin_Floor0_AtBase()
    {
        var basePos = new Vector3(100f, 0f, 100f);
        var origin = TowerChunkLayoutGenerator.GetChunkOrigin(basePos, 0);

        Assert.AreEqual(basePos, origin);
    }

    [Test]
    public void GetChunkOrigin_Floor3_CorrectYOffset()
    {
        var basePos = new Vector3(100f, 0f, 100f);
        var origin = TowerChunkLayoutGenerator.GetChunkOrigin(basePos, 3);

        Assert.AreEqual(basePos.x, origin.x);
        Assert.AreEqual(3 * TowerChunkLayoutGenerator.ChunkHeight, origin.y, 0.01f);
        Assert.AreEqual(basePos.z, origin.z);
    }
}
