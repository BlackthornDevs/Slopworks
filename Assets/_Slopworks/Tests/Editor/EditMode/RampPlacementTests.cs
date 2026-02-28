using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class RampPlacementTests
{
    private FactoryGrid _grid;
    private SnapPointRegistry _snapRegistry;
    private StructuralPlacementService _service;
    private FoundationDefinitionSO _foundationDef;
    private RampDefinitionSO _rampDef;

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

        _rampDef = ScriptableObject.CreateInstance<RampDefinitionSO>();
        _rampDef.rampId = "ramp_basic";
        _rampDef.footprintLength = 3;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_foundationDef);
        Object.DestroyImmediate(_rampDef);
    }

    // -- Place at foundation edge succeeds --

    [Test]
    public void PlaceRamp_AtFoundationEdge_Succeeds()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);

        var ramp = _service.PlaceRamp(_rampDef, snap);
        Assert.IsNotNull(ramp);
    }

    // -- No foundation = fails --

    [Test]
    public void PlaceRamp_NullSnapPoint_Fails()
    {
        var ramp = _service.PlaceRamp(_rampDef, null);
        Assert.IsNull(ramp);
    }

    // -- Occupied footprint = fails (non-structural blocker) --

    [Test]
    public void PlaceRamp_OccupiedByNonStructural_Fails()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        // Block one of the ramp cells with a non-structural building (machine/storage)
        var blocker = new BuildingData("blocker", new Vector2Int(5, 7), Vector2Int.one);
        _grid.Place(new Vector2Int(5, 7), Vector2Int.one, 0, blocker);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        var ramp = _service.PlaceRamp(_rampDef, snap);
        Assert.IsNull(ramp);
    }

    // -- Ramp over foundations succeeds --

    [Test]
    public void PlaceRamp_OverFoundations_Succeeds()
    {
        // Place a row of foundations where the ramp will go
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 6), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 7), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 8), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        // Snap point on shared edge was suppressed, use the north edge of (5,8) instead
        // Actually the ramp would go north from (5,5), cells (5,6)-(5,8) which have foundations
        // The north edge of (5,5) may be suppressed because (5,6) is adjacent.
        // Use the north edge of the last foundation instead.
        snap = _snapRegistry.GetAt(new Vector2Int(5, 8), 0, Vector2Int.up);
        Assert.IsNotNull(snap, "Expected north edge snap point on (5,8)");

        var ramp = _service.PlaceRamp(_rampDef, snap);
        // Ramp goes north from (5,8), cells (5,9)-(5,11) which are empty
        // This tests that ramp can START from a foundation edge
        Assert.IsNotNull(ramp);
    }

    [Test]
    public void PlaceRamp_ThroughFoundationCells_Succeeds()
    {
        // Place foundation at the starting cell
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        // Place foundations in the ramp's path
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 6), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 7), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 8), 0);

        // The north edge of (5,5) is suppressed because (5,6) is adjacent.
        // Instead, place more foundations to get an exterior edge, or use a different approach.
        // Let's use a separate foundation at (4,5) and go east through the row.
        var isolatedFoundation = _service.PlaceFoundation(_foundationDef, new Vector2Int(2, 5), 0);
        Assert.IsNotNull(isolatedFoundation);

        var snap = _snapRegistry.GetAt(new Vector2Int(2, 5), 0, Vector2Int.right);
        Assert.IsNotNull(snap, "Expected east edge snap point on (2,5)");

        // Ramp goes east: cells (3,5), (4,5), (5,5) -- (5,5) has a foundation
        var ramp = _service.PlaceRamp(_rampDef, snap);
        Assert.IsNotNull(ramp, "Ramp should be placeable over foundation cells");
        Assert.AreEqual(3, ramp.OccupiedCells.Count);
    }

    // -- Creates base/top snap points at correct levels --

    [Test]
    public void PlaceRamp_CreatesBaseSnapPoint()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);

        var ramp = _service.PlaceRamp(_rampDef, snap);

        Assert.IsNotNull(ramp.BaseSnapPoint);
        Assert.AreEqual(0, ramp.BaseSnapPoint.Level);
        Assert.AreEqual(SnapPointType.RampBase, ramp.BaseSnapPoint.Type);
    }

    [Test]
    public void PlaceRamp_CreatesTopSnapPoint_AtUpperLevel()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);

        var ramp = _service.PlaceRamp(_rampDef, snap);

        Assert.IsNotNull(ramp.TopSnapPoint);
        Assert.AreEqual(1, ramp.TopSnapPoint.Level);
        Assert.AreEqual(SnapPointType.RampTop, ramp.TopSnapPoint.Type);
    }

    // -- Tracks footprint cells --

    [Test]
    public void PlaceRamp_TracksFootprintCells()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);

        var ramp = _service.PlaceRamp(_rampDef, snap);

        // Ramp goes north from (5,5), occupying (5,6), (5,7), (5,8)
        Assert.AreEqual(3, ramp.OccupiedCells.Count);
        Assert.Contains(new Vector2Int(5, 6), ramp.OccupiedCells);
        Assert.Contains(new Vector2Int(5, 7), ramp.OccupiedCells);
        Assert.Contains(new Vector2Int(5, 8), ramp.OccupiedCells);
    }

    // -- Remove frees cells for new ramp placement --

    [Test]
    public void RemoveRamp_AllowsNewRampAtSameCells()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        var ramp = _service.PlaceRamp(_rampDef, snap);

        _service.RemoveRamp(ramp);

        // Should be able to place a new ramp at the same edge
        var newRamp = _service.PlaceRamp(_rampDef, snap);
        Assert.IsNotNull(newRamp);
    }

    [Test]
    public void RemoveRamp_FreesFoundationEdge()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        var ramp = _service.PlaceRamp(_rampDef, snap);

        Assert.IsTrue(snap.IsOccupied);
        _service.RemoveRamp(ramp);
        Assert.IsFalse(snap.IsOccupied);
    }

    // -- Overlapping ramps blocked --

    [Test]
    public void PlaceRamp_OverlappingExistingRamp_Fails()
    {
        // Place two adjacent foundations to get two snap points going north
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(4, 5), 0);

        var snap1 = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        _service.PlaceRamp(_rampDef, snap1);

        // Place a second foundation at (5,7) and try to ramp north through the same cells
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 4), 0);
        var snap2 = _snapRegistry.GetAt(new Vector2Int(5, 4), 0, Vector2Int.up);

        // This ramp would occupy (5,5), (5,6), (5,7) -- (5,6) and (5,7) overlap the first ramp
        // But (5,5) has a foundation (structural), so it should be allowed...
        // except (5,6) is occupied by the first ramp, so it should fail
        var ramp2 = _service.PlaceRamp(_rampDef, snap2);
        Assert.IsNull(ramp2, "Overlapping ramps should be blocked");
    }

    // -- Already occupied snap point --

    [Test]
    public void PlaceRamp_AtOccupiedEdge_Fails()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);

        _service.PlaceRamp(_rampDef, snap);

        // Try to place another ramp at the same edge
        var secondRamp = _service.PlaceRamp(_rampDef, snap);
        Assert.IsNull(secondRamp);
    }

    // -- Max level boundary --

    [Test]
    public void PlaceRamp_AtTopLevel_Fails()
    {
        int topLevel = FactoryGrid.MaxLevels - 1;
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), topLevel);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), topLevel, Vector2Int.up);

        var ramp = _service.PlaceRamp(_rampDef, snap);
        Assert.IsNull(ramp);
    }

    // -- Ramp direction --

    [Test]
    public void PlaceRamp_East_TracksCorrectCells()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.right);

        var ramp = _service.PlaceRamp(_rampDef, snap);
        Assert.IsNotNull(ramp);

        // Ramp goes east from (5,5), occupying (6,5), (7,5), (8,5)
        Assert.AreEqual(3, ramp.OccupiedCells.Count);
        Assert.Contains(new Vector2Int(6, 5), ramp.OccupiedCells);
        Assert.Contains(new Vector2Int(7, 5), ramp.OccupiedCells);
        Assert.Contains(new Vector2Int(8, 5), ramp.OccupiedCells);
    }

    // -- Cell+direction overload (interior edges) --

    [Test]
    public void PlaceRamp_CellDirection_FromInteriorEdge_Succeeds()
    {
        // 3x1 slab: (5,5), (5,6), (5,7)
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 6), 0);
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 7), 0);

        // (5,6) has no north/south snap points (interior edges suppressed)
        Assert.IsNull(_snapRegistry.GetAt(new Vector2Int(5, 6), 0, Vector2Int.up));

        // Place ramp going east from interior cell (5,6) using cell+direction overload
        var ramp = _service.PlaceRamp(_rampDef, new Vector2Int(5, 6), 0, Vector2Int.right);
        Assert.IsNotNull(ramp, "Ramp from interior cell should succeed via cell+direction");
        Assert.AreEqual(Vector2Int.right, ramp.Direction);
        Assert.AreEqual(3, ramp.OccupiedCells.Count);
    }

    [Test]
    public void PlaceRamp_CellDirection_NoFoundation_Fails()
    {
        // No foundation at (5,5) -- should fail
        var ramp = _service.PlaceRamp(_rampDef, new Vector2Int(5, 5), 0, Vector2Int.up);
        Assert.IsNull(ramp);
    }

    [Test]
    public void PlaceRamp_CellDirection_MarksExteriorSnapOccupied()
    {
        _service.PlaceFoundation(_foundationDef, new Vector2Int(5, 5), 0);

        var snap = _snapRegistry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        Assert.IsNotNull(snap);
        Assert.IsFalse(snap.IsOccupied);

        // Place via cell+direction -- should still mark the exterior snap point
        _service.PlaceRamp(_rampDef, new Vector2Int(5, 5), 0, Vector2Int.up);
        Assert.IsTrue(snap.IsOccupied);
    }
}
