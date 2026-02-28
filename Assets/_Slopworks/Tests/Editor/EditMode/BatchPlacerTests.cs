using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BatchPlacerTests
{
    private BatchPlacer _placer;
    private FactoryGrid _grid;

    [SetUp]
    public void SetUp()
    {
        _placer = new BatchPlacer();
        _grid = new FactoryGrid();
    }

    // -- Lifecycle --

    [Test]
    public void NewPlacer_IsNotPlacing()
    {
        Assert.IsFalse(_placer.IsPlacing);
    }

    [Test]
    public void StartPlacement_SetsIsPlacing()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        Assert.IsTrue(_placer.IsPlacing);
    }

    [Test]
    public void FinishPlacement_ClearsIsPlacing()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        _placer.FinishPlacement();
        Assert.IsFalse(_placer.IsPlacing);
    }

    [Test]
    public void Cancel_ClearsIsPlacing()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        _placer.Cancel();
        Assert.IsFalse(_placer.IsPlacing);
    }

    // -- Rectangle normalization --

    [Test]
    public void Rectangle_DragRightDown_NormalizedMinMax()
    {
        _placer.StartPlacement(new Vector2Int(2, 8), 0);
        _placer.UpdateDrag(new Vector2Int(5, 3));

        var (min, max) = _placer.Rectangle;
        Assert.AreEqual(new Vector2Int(2, 3), min);
        Assert.AreEqual(new Vector2Int(5, 8), max);
    }

    [Test]
    public void Rectangle_DragLeftUp_NormalizedMinMax()
    {
        _placer.StartPlacement(new Vector2Int(10, 10), 0);
        _placer.UpdateDrag(new Vector2Int(7, 12));

        var (min, max) = _placer.Rectangle;
        Assert.AreEqual(new Vector2Int(7, 10), min);
        Assert.AreEqual(new Vector2Int(10, 12), max);
    }

    // -- Cell count --

    [Test]
    public void CellCount_1x1()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        Assert.AreEqual(1, _placer.CellCount);
    }

    [Test]
    public void CellCount_3x4()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        _placer.UpdateDrag(new Vector2Int(7, 8));
        Assert.AreEqual(12, _placer.CellCount);
    }

    [Test]
    public void CellCount_NotPlacing_ReturnsZero()
    {
        Assert.AreEqual(0, _placer.CellCount);
    }

    // -- Preview cells --

    [Test]
    public void PreviewCells_2x2_ReturnsFourCells()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        _placer.UpdateDrag(new Vector2Int(6, 6));

        var cells = _placer.PreviewCells;
        Assert.AreEqual(4, cells.Count);
        Assert.Contains(new Vector2Int(5, 5), cells);
        Assert.Contains(new Vector2Int(5, 6), cells);
        Assert.Contains(new Vector2Int(6, 5), cells);
        Assert.Contains(new Vector2Int(6, 6), cells);
    }

    // -- Validation --

    [Test]
    public void ValidateBatch_EmptyGrid_AllValid()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        _placer.UpdateDrag(new Vector2Int(7, 7));

        var (valid, invalidCount) = _placer.ValidateBatch(_grid);
        Assert.AreEqual(9, valid.Count);
        Assert.AreEqual(0, invalidCount);
    }

    [Test]
    public void ValidateBatch_PartiallyOccupied_SplitsCorrectly()
    {
        var blocker = new BuildingData("blocker", new Vector2Int(6, 6), Vector2Int.one);
        _grid.Place(new Vector2Int(6, 6), Vector2Int.one, blocker);

        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        _placer.UpdateDrag(new Vector2Int(7, 7));

        var (valid, invalidCount) = _placer.ValidateBatch(_grid);
        Assert.AreEqual(8, valid.Count);
        Assert.AreEqual(1, invalidCount);
    }

    // -- Level awareness --

    [Test]
    public void ValidateBatch_Level1_IgnoresLevel0Occupancy()
    {
        var blocker = new BuildingData("blocker", new Vector2Int(6, 6), Vector2Int.one, 0, 0);
        _grid.Place(new Vector2Int(6, 6), Vector2Int.one, 0, blocker);

        _placer.StartPlacement(new Vector2Int(5, 5), 1);
        _placer.UpdateDrag(new Vector2Int(7, 7));

        var (valid, invalidCount) = _placer.ValidateBatch(_grid);
        Assert.AreEqual(9, valid.Count);
        Assert.AreEqual(0, invalidCount);
    }

    [Test]
    public void StartPlacement_PreservesLevel()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 2);
        Assert.AreEqual(2, _placer.Level);
    }

    // -- FinishPlacement returns rectangle --

    [Test]
    public void FinishPlacement_ReturnsNormalizedRectangle()
    {
        _placer.StartPlacement(new Vector2Int(10, 10), 0);
        _placer.UpdateDrag(new Vector2Int(7, 12));

        var result = _placer.FinishPlacement();
        Assert.IsNotNull(result);

        var (min, max) = result.Value;
        Assert.AreEqual(new Vector2Int(7, 10), min);
        Assert.AreEqual(new Vector2Int(10, 12), max);
    }

    [Test]
    public void FinishPlacement_NotPlacing_ReturnsNull()
    {
        var result = _placer.FinishPlacement();
        Assert.IsNull(result);
    }

    // -- Cancel --

    [Test]
    public void Cancel_PreviewCells_Empty()
    {
        _placer.StartPlacement(new Vector2Int(5, 5), 0);
        _placer.UpdateDrag(new Vector2Int(7, 7));
        _placer.Cancel();

        Assert.AreEqual(0, _placer.PreviewCells.Count);
        Assert.AreEqual(0, _placer.CellCount);
    }
}
