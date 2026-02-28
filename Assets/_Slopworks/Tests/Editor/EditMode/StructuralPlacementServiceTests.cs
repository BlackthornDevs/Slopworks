using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class StructuralPlacementServiceTests
{
    private FactoryGrid _grid;
    private SnapPointRegistry _snapRegistry;
    private StructuralPlacementService _service;
    private FoundationDefinitionSO _foundationDef;
    private WallDefinitionSO _wallDef;

    [SetUp]
    public void SetUp()
    {
        _grid = new FactoryGrid();
        _snapRegistry = new SnapPointRegistry();
        _service = new StructuralPlacementService(_grid, _snapRegistry);

        _foundationDef = ScriptableObject.CreateInstance<FoundationDefinitionSO>();
        _foundationDef.foundationId = "foundation_1x1";
        _foundationDef.size = Vector2Int.one;
        _foundationDef.generatesSnapPoints = true;

        _wallDef = ScriptableObject.CreateInstance<WallDefinitionSO>();
        _wallDef.wallId = "wall_basic";
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_foundationDef);
        Object.DestroyImmediate(_wallDef);
    }

    // -- Foundation creates snap points --

    [Test]
    public void PlaceFoundation_Creates4EdgeSnapPoints()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        Assert.AreEqual(4, _snapRegistry.Count);
    }

    [Test]
    public void PlaceFoundation_SnapPointsOnAllEdges()
    {
        var data = _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up));
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.right));
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.down));
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.left));
    }

    // -- Adjacent foundations suppress shared edge --

    [Test]
    public void AdjacentFoundations_SuppressSharedEdge()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(6, 5), 0);

        // Shared edge: (5,5) east and (6,5) west should be suppressed
        Assert.IsNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.right));
        Assert.IsNull(_snapRegistry.GetAt(new Vector2Int(6, 5), 0, Vector2Int.left));

        // Non-shared edges should still exist
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.left));
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(6, 5), 0, Vector2Int.right));
    }

    // -- Foundation removal restores neighbor edges --

    [Test]
    public void RemoveFoundation_RestoresNeighborEdges()
    {
        var data1 = _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var data2 = _service.PlaceFoundation(_foundationDef, new Vector2Int(6, 5), 0);

        // Shared edges are suppressed
        Assert.IsNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.right));

        // Remove second foundation
        _service.RemoveFoundation(data2);

        // First foundation's east edge should be restored
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.right));
    }

    // -- Wall placement --

    [Test]
    public void PlaceWall_AtSnapPoint_Succeeds()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        var wall = _service.PlaceWall(_wallDef, snap);

        Assert.IsNotNull(wall);
        Assert.IsTrue(snap.IsOccupied);
    }

    [Test]
    public void PlaceWall_AtOccupiedSnapPoint_Fails()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        _service.PlaceWall(_wallDef, snap);

        var secondWall = _service.PlaceWall(_wallDef, snap);
        Assert.IsNull(secondWall);
    }

    [Test]
    public void PlaceWall_NullSnapPoint_Fails()
    {
        var wall = _service.PlaceWall(_wallDef, null);
        Assert.IsNull(wall);
    }

    // -- Wall removal frees snap point --

    [Test]
    public void RemoveWall_FreesSnapPoint()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        var wall = _service.PlaceWall(_wallDef, snap);
        Assert.IsTrue(snap.IsOccupied);

        _service.RemoveWall(wall);
        Assert.IsFalse(snap.IsOccupied);
    }

    // -- Foundation removal blocked by attached walls --

    [Test]
    public void RemoveFoundation_WithAttachedWall_Fails()
    {
        var data = _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        _service.PlaceWall(_wallDef, snap);

        bool removed = _service.RemoveFoundation(data);
        Assert.IsFalse(removed);
    }

    [Test]
    public void RemoveFoundation_AfterWallRemoved_Succeeds()
    {
        var data = _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        var wall = _service.PlaceWall(_wallDef, snap);
        _service.RemoveWall(wall);

        bool removed = _service.RemoveFoundation(data);
        Assert.IsTrue(removed);
    }

    // -- 2x2 foundation perimeter --

    [Test]
    public void PlaceFoundation_2x2_Creates8ExternalEdgeSnapPoints()
    {
        var largeDef = ScriptableObject.CreateInstance<FoundationDefinitionSO>();
        largeDef.foundationId = "foundation_2x2";
        largeDef.size = new Vector2Int(2, 2);
        largeDef.generatesSnapPoints = true;

        _service.PlaceFoundation(largeDef, new Vector2Int(5, 5), 0);

        // 2x2 = 4 cells, each has 4 edges = 16 total
        // Internal edges: 4 (between cells), each shared = 4 suppressed
        // External edges: 8
        Assert.AreEqual(8, _snapRegistry.Count);

        Object.DestroyImmediate(largeDef);
    }

    // -- Level awareness --

    [Test]
    public void PlaceFoundation_DifferentLevels_IndependentSnapPoints()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 1);

        // Each foundation should have 4 snap points = 8 total
        Assert.AreEqual(8, _snapRegistry.Count);

        // Level 0 north edge
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up));
        // Level 1 north edge
        Assert.IsNotNull(_snapRegistry.GetAt(new Vector2Int(5, 5), 1, Vector2Int.up));
    }

    // -- Occupied cell placement fails --

    [Test]
    public void PlaceFoundation_OccupiedCell_ReturnsNull()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var result = _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        Assert.IsNull(result);
    }

    // -- GetAvailableSnapPoints --

    [Test]
    public void GetAvailableSnapPoints_AllUnoccupied_ReturnsFour()
    {
        var data = _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var available = _service.GetAvailableSnapPoints(data);
        Assert.AreEqual(4, available.Count);
    }

    [Test]
    public void GetAvailableSnapPoints_OneOccupied_ReturnsThree()
    {
        var data = _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        _service.PlaceWall(_wallDef, snap);

        var available = _service.GetAvailableSnapPoints(data);
        Assert.AreEqual(3, available.Count);
    }
}
