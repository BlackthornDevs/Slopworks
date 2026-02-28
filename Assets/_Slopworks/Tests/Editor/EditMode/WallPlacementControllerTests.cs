using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class WallPlacementControllerTests
{
    private FactoryGrid _grid;
    private SnapPointRegistry _snapRegistry;
    private StructuralPlacementService _structService;
    private WallPlacementController _wallController;
    private FoundationDefinitionSO _foundationDef;

    [SetUp]
    public void SetUp()
    {
        _grid = new FactoryGrid();
        _snapRegistry = new SnapPointRegistry();
        _structService = new StructuralPlacementService(_grid, _snapRegistry);
        _wallController = new WallPlacementController(_snapRegistry);

        _foundationDef = ScriptableObject.CreateInstance<FoundationDefinitionSO>();
        _foundationDef.foundationId = "foundation_1x1";
        _foundationDef.size = Vector2Int.one;
        _foundationDef.generatesSnapPoints = true;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_foundationDef);
    }

    [Test]
    public void UpdateFromCursor_NearSnapPoint_FindsIt()
    {
        _structService.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        // Cursor near the north edge of cell (5,5)
        // Cell center is (5.5, 0, 5.5), north edge is (5.5, 0, 6.0)
        _wallController.UpdateFromCursor(new Vector3(5.5f, 0f, 5.9f), _grid, 0);

        Assert.IsNotNull(_wallController.NearestSnapPoint);
        Assert.AreEqual(Vector2Int.up, _wallController.NearestSnapPoint.EdgeDirection);
    }

    [Test]
    public void UpdateFromCursor_NoFoundation_FindsNull()
    {
        _wallController.UpdateFromCursor(new Vector3(50.5f, 0f, 50.5f), _grid, 0);
        Assert.IsNull(_wallController.NearestSnapPoint);
    }

    [Test]
    public void UpdateFromCursor_TooFar_FindsNull()
    {
        _structService.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        // Very far from foundation
        _wallController.UpdateFromCursor(new Vector3(20f, 0f, 20f), _grid, 0);
        Assert.IsNull(_wallController.NearestSnapPoint);
    }

    [Test]
    public void GetSnapWorldPosition_ReturnsEdgeCenter()
    {
        _structService.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        var worldPos = WallPlacementController.GetSnapWorldPosition(snap, _grid);

        // Cell (5,5) center = (5.5, 0, 5.5), north edge center = (5.5, 0, 6.0)
        Assert.AreEqual(5.5f, worldPos.x, 0.001f);
        Assert.AreEqual(0f, worldPos.y, 0.001f);
        Assert.AreEqual(6.0f, worldPos.z, 0.001f);
    }
}
